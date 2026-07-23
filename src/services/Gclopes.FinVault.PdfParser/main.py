import os
import io
import json
import asyncio
import base64
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

# Base URL of the DocumentIngestion service's completion callback, e.g.
# "http://document-ingestion:8080/api/document". Called directly over the internal Docker
# network (not through the Gateway) once a document has finished processing, successfully or not,
# so the PDF Upload Centre's import history reflects the real outcome.
DOCUMENT_INGESTION_URL = os.getenv("DOCUMENT_INGESTION_SERVICE_URL")

# Sentinel used by the /parse HTTP endpoint when no queue-originated document is being tracked;
# there is nothing to report back to DocumentIngestion in that case.
_MANUAL_UPLOAD_SENTINEL = "manual-upload"


async def notify_document_ingestion(source_document_id: str, success: bool, document_type: str | None, error_message: str | None) -> None:
    if not DOCUMENT_INGESTION_URL:
        print("[DocumentIngestion] DOCUMENT_INGESTION_SERVICE_URL not configured, skipping completion callback.")
        return

    if source_document_id == _MANUAL_UPLOAD_SENTINEL:
        return

    try:
        async with httpx.AsyncClient() as client:
            response = await client.post(
                f"{DOCUMENT_INGESTION_URL}/{source_document_id}/completed",
                json={"success": success, "documentType": document_type, "errorMessage": error_message},
                timeout=10.0
            )
            response.raise_for_status()
    except Exception as e:
        # Reporting the outcome is best-effort: a failure to notify DocumentIngestion should
        # never mask the original processing result, so it's only logged here.
        print(f"[DocumentIngestion] Failed to report completion for {source_document_id}: {e}")


class PDFProcessorService:
    def __init__(self, service_urls: dict):
        self.service_urls = service_urls

    async def process_pdf(self, file_bytes: bytes, filename: str, source_document_id: str) -> dict:
        doc_type = None
        try:
            if not filename.endswith('.pdf'):
                raise ValueError("Only PDF files are supported.")

            with pdfplumber.open(io.BytesIO(file_bytes)) as pdf:
                first_page_text = pdf.pages[0].extract_text() or ""
                print(f"[PDFProcessor] Extracted text from first page of {filename}: {first_page_text[:100]}...")  # Log first 100 chars
                doc_type = DocumentClassifier.classify(first_page_text)

                if doc_type == "unknown":
                    raise ValueError("Document type could not be classified.")

                payload = self._delegate_to_parser(doc_type, pdf)
                envelope = {"sourceDocumentId": source_document_id, "data": payload}
                print(f"envelope--> {envelope}")
                # The downstream .NET services expect the full envelope
                # ({ sourceDocumentId, data }), not the bare parser payload.
                await self._forward_to_dotnet_service(doc_type, envelope)

            await notify_document_ingestion(source_document_id, True, doc_type, None)
            return {"status": "success", "documentType": doc_type, "data": envelope}
        except Exception as e:
            await notify_document_ingestion(source_document_id, False, doc_type, str(e))
            raise

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
processor = PDFProcessorService({
    "bank_statement": os.getenv("CURRENTACCOUNT_SERVICE_URL"),
    "payroll": os.getenv("PAYROLL_SERVICE_URL"),
    "pension_insurance": os.getenv("PENSION_INSURANCE_SERVICE_URL"),
    "pension_fund": os.getenv("PENSION_FUND_SERVICE_URL"),
})

async def consume_rabbitmq():
    rabbit_host = os.getenv("RABBITMQ_HOST", "finvault-rabbitmq")
    rabbit_user = os.getenv("RABBITMQ_USER", "guest")
    rabbit_pass = os.getenv("RABBITMQ_PASS", "guest")
    rabbit_port = int(os.getenv("RABBITMQ_PORT", 5672))

    print(f"[RabbitMQ] Attempting to connect to {rabbit_host}:{rabbit_port}...")
    
    for attempt in range(1, 11):
        try:
            # Using parameter-based connection for maximum stability
            connection = await aio_pika.connect_robust(
                host=rabbit_host,
                port=rabbit_port,
                login=rabbit_user,
                password=rabbit_pass,
                virtualhost="/"
            )
            print("[RabbitMQ] Connection established successfully!")
            
            channel = await connection.channel()
            queue = await channel.declare_queue("pdf_processing_queue", durable=True)
            
            async with queue.iterator() as queue_iter:
                async for message in queue_iter:
                    async with message.process():
                        try:
                            data = json.loads(message.body)
                            await processor.process_pdf(
                                base64.b64decode(data["bytes"]),
                                data["filename"],
                                data["sourceDocumentId"]
                            )
                        except Exception as e:
                            print(f"Error processing message: {e}")
            break
        except Exception as e:
            print(f"[RabbitMQ] Attempt {attempt} failed: {e}")
            await asyncio.sleep(10)

@asynccontextmanager
async def lifespan(_: FastAPI):
    print("--- LIFESPAN STARTUP ---")
    enabled = os.getenv("RABBITMQ_ENABLED", "false").lower()
    task = None
    
    if enabled == "true":
        task = asyncio.create_task(consume_rabbitmq())
    else:
        print("--- RABBITMQ IS DISABLED ---")
        
    yield 
    
    if task:
        task.cancel()

app = FastAPI(title="FinVault PDF Parser", lifespan=lifespan)

@app.post("/parse")
async def parse_http(file: UploadFile = File(...), sourceDocumentId: str = "manual-upload"):
    try:
        content = await file.read()
        return await processor.process_pdf(content, file.filename, sourceDocumentId)
    except Exception as e:
        raise HTTPException(status_code=400, detail=str(e))