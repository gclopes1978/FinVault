import os
import io
import json
import asyncio
import base64
import uvicorn
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

# Service logic encapsulated with Primary Constructor approach
class PDFProcessorService:
    def __init__(self, service_urls: dict):
        self.service_urls = service_urls

    async def process_pdf(self, file_bytes: bytes, filename: str, source_document_id: str) -> dict:
        """Core logic to process the PDF bytes and forward to .NET services."""
        if not filename.endswith('.pdf'):
            raise ValueError("Only PDF files are supported.")
            
        with pdfplumber.open(io.BytesIO(file_bytes)) as pdf:
            first_page_text = pdf.pages[0].extract_text() or ""
            doc_type = DocumentClassifier.classify(first_page_text)
            
            if doc_type == "unknown":
                raise ValueError("Document type could not be classified.")
                
            payload = self._delegate_to_parser(doc_type, pdf)
            
            # Constructing the exact envelope required by .NET 9.0 services
            envelope = {
                "sourceDocumentId": source_document_id,
                "data": payload
            }
            
            await self._forward_to_dotnet_service(doc_type, envelope)
            
            return {"status": "success", "documentType": doc_type, "data": envelope}

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
            raise Exception(f"Target URL for {doc_type} configuration missing.")
        
        async with httpx.AsyncClient() as client:
            response = await client.post(target_url, json=payload, timeout=30.0)
            response.raise_for_status()

# Configuration
service_urls = {
    "bank_statement": os.getenv("CURRENTACCOUNT_SERVICE_URL"),
    "payroll": os.getenv("PAYROLL_SERVICE_URL"),
    "pension_insurance": os.getenv("PENSION_INSURANCE_SERVICE_URL"),
    "pension_fund": os.getenv("PENSION_FUND_SERVICE_URL"),
}

processor = PDFProcessorService(service_urls)

# RabbitMQ Consumer Logic
async def consume_rabbitmq():
    # Constructing URL from host to ensure compatibility with docker-compose
    rabbit_host = os.getenv("RABBITMQ_HOST", "localhost")
    rabbitmq_url = f"amqp://guest:guest@{rabbit_host}/"
    
    try:
        connection = await aio_pika.connect_robust(rabbitmq_url)
        channel = await connection.channel()
        queue = await channel.declare_queue("pdf_processing_queue")

        async with queue.iterator() as queue_iter:
            async for message in queue_iter:
                async with message.process():
                    try:
                        data = json.loads(message.body)
                        # Decoding Base64 correctly from .NET publisher
                        file_bytes = base64.b64decode(data["bytes"])
                        
                        await processor.process_pdf(
                            file_bytes, 
                            data["filename"],
                            data["sourceDocumentId"]
                        )
                    except Exception as e:
                        print(f"Error processing rabbit message: {e}")
    except Exception as e:
        print(f"RabbitMQ connection failed: {e}")

# Lifespan handler
@asynccontextmanager
async def lifespan(_: FastAPI):
    # Startup: Start RabbitMQ consumer task
    task = None
    if os.getenv("ENABLE_RABBITMQ", "false").lower() == "true":
        task = asyncio.create_task(consume_rabbitmq())
        
    yield 
    
    # Shutdown: Clean up task
    if task:
        task.cancel()

app = FastAPI(title="FinVault PDF Parser Hybrid Service", lifespan=lifespan)

# HTTP Entry Point
@app.post("/parse")
async def parse_http(file: UploadFile = File(...), sourceDocumentId: str = "manual-upload"):
    try:
        content = await file.read()
        result = await processor.process_pdf(content, file.filename, sourceDocumentId)
        return result
    except Exception as e:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail=str(e))

if __name__ == "__main__":
    uvicorn.run("main:app", host="0.0.0.0", port=8080, reload=True)