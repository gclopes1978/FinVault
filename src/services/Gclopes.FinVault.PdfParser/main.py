import os
import io
import json
import asyncio
import pdfplumber
import httpx
from contextlib import asynccontextmanager
from fastapi import FastAPI, UploadFile, File, HTTPException, status
import aio_pika
from classifier import DocumentClassifier
from parsers.bank_statement import BankStatementParser
from parsers.payroll import PayrollParser
from parsers.pension_insurance import PensionInsuranceParser
from parsers.pension_fund import PensionFundParser

class PDFProcessorService:
    def __init__(self, service_urls: dict):
        self.service_urls = service_urls

    async def process_pdf(self, file_bytes: bytes, filename: str) -> dict:
        if not filename.endswith('.pdf'):
            raise ValueError("Only PDF files are supported.")
            
        with pdfplumber.open(io.BytesIO(file_bytes)) as pdf:
            first_page_text = pdf.pages[0].extract_text() or ""
            doc_type = DocumentClassifier.classify(first_page_text)
            
            if doc_type == "unknown":
                raise ValueError("Document type could not be classified.")
                
            payload = self._delegate_to_parser(doc_type, pdf)
            await self._forward_to_dotnet_service(doc_type, payload)
            
            return {"status": "success", "documentType": doc_type, "data": payload}

    def _delegate_to_parser(self, doc_type: str, pdf: pdfplumber.PDF) -> dict:
        parsers = {
            "bank_statement": BankStatementParser.parse,
            "payroll": PayrollParser.parse,
            "pension_insurance": PensionInsuranceParser.parse,
            "pension_fund": PensionFundParser.parse
        }
        if doc_type not in parsers:
            raise ValueError(f"Parser for {doc_type} not implemented.")
        return parsers[doc_type](pdf)

    async def _forward_to_dotnet_service(self, doc_type: str, payload: dict):
        target_url = self.service_urls.get(doc_type)
        if not target_url:
            raise Exception("Target URL configuration missing.")
        async with httpx.AsyncClient() as client:
            response = await client.post(target_url, json=payload, timeout=10.0)
            response.raise_for_status()

# Configuration
service_urls = {
    "bank_statement": os.getenv("CURRENTACCOUNT_SERVICE_URL"),
    "payroll": os.getenv("FOLHASALARIAL_SERVICE_URL"),
    "pension_insurance": os.getenv("PPR_SERVICE_URL"),
    "pension_fund": os.getenv("PPR_SERVICE_URL"),
}

processor = PDFProcessorService(service_urls)

# RabbitMQ Consumer Logic
async def consume_rabbitmq():
    rabbitmq_url = os.getenv("RABBITMQ_URL")
    if not rabbitmq_url:
        return
        
    connection = await aio_pika.connect_robust(rabbitmq_url)
    channel = await connection.channel()
    queue = await channel.declare_queue("pdf_processing_queue")

    async with queue.iterator() as queue_iter:
        async for message in queue_iter:
            async with message.process():
                try:
                    data = json.loads(message.body)
                    await processor.process_pdf(data["bytes"], data["filename"])
                except Exception as e:
                    print(f"Error processing rabbit message: {e}")

# Lifespan handler replacing on_event
@asynccontextmanager
async def lifespan(app: FastAPI):
    # Startup: Start RabbitMQ consumer task
    task = None
    if os.getenv("ENABLE_RABBITMQ") == "true":
        task = asyncio.create_task(consume_rabbitmq())
        
    yield # App is running
    
    # Shutdown: Clean up task
    if task:
        task.cancel()

app = FastAPI(title="FinVault PDF Parser Hybrid Service", lifespan=lifespan)

@app.post("/parse")
async def parse_http(file: UploadFile = File(...)):
    try:
        content = await file.read()
        result = await processor.process_pdf(content, file.filename)
        return result
    except Exception as e:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail=str(e))