import os
import io
import httpx
import pdfplumber
from fastapi import FastAPI, UploadFile, File, HTTPException, status
from classifier import DocumentClassifier
from parsers.bank_statement import BankStatementParser
from parsers.payroll import PayrollParser
from parsers.pension_insurance import PensionInsuranceParser
from parsers.pension_fund import PensionFundParser

app = FastAPI(title="FinVault PDF Parser Microservice")

# Service routing configurations via Environment Variables
SERVICE_URLS = {
    "bank_statement": os.getenv("CURRENTACCOUNT_SERVICE_URL"),
    "payroll": os.getenv("FOLHASALARIAL_SERVICE_URL"),
    "pension_insurance": os.getenv("PPR_SERVICE_URL"),
    "pension_fund": os.getenv("PPR_SERVICE_URL"),
}

@app.post("/parse")
async def parse_pdf(file: UploadFile = File(...)):
    if not file.filename.endswith('.pdf'):
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail="Only PDF files are supported.")
        
    try:
        file_bytes = await file.read()
        
        with pdfplumber.open(io.BytesIO(file_bytes)) as pdf:
            first_page_text = pdf.pages[0].extract_text() or ""
            doc_type = DocumentClassifier.classify(first_page_text)
            
            if doc_type == "unknown":
                raise HTTPException(status_code=status.HTTP_422_UNPROCESSABLE_ENTITY, detail="Document type could not be classified.")
                
            # Process parser based on discovered document type
            if doc_type == "bank_statement":
                payload = BankStatementParser.parse(pdf)
            elif doc_type == "payroll":
                payload = PayrollParser.parse(pdf)
            elif doc_type == "pension_insurance":
                payload = PensionInsuranceParser.parse(pdf)
            elif doc_type == "pension_fund":
                try:
                    payload = PensionFundParser.parse(pdf)
                except ValueError as val_err:
                    raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail=str(val_err))
            
            # Forward data to target destination microservice
            target_url = SERVICE_URLS.get(doc_type)
            if not target_url:
                raise HTTPException(
                    status_code=status.HTTP_501_NOT_IMPLEMENTED, 
                    detail=f"Target service URL for '{doc_type}' is not configured."
                )
                
            async with httpx.AsyncClient() as client:
                try:
                    response = await client.post(target_url, json=payload, timeout=10.0)
                    response.raise_for_status()
                except httpx.HTTPStatusError as exc:
                    raise HTTPException(
                        status_code=status.HTTP_502_BAD_GATEWAY,
                        detail=f"Failed to forward data to .NET service. Remote responded with status: {exc.response.status_code}"
                    )
                except httpx.RequestError:
                    raise HTTPException(
                        status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
                        detail="Target .NET service is unreachable."
                    )
                    
            return {
                "status": "success",
                "documentType": doc_type,
                "forwardedTo": target_url,
                "data": payload
            }
            
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=status.HTTP_500_INTERNAL_SERVER_ERROR, detail=f"An error occurred while processing the PDF: {str(e)}")