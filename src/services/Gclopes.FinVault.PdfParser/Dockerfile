# Use the official Python image as base
FROM python:3.12-slim

# Set the working directory inside the container
WORKDIR /app

# Install system dependencies
# pdfplumber relies on poppler-utils for robust PDF handling
RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential \
    poppler-utils \
    && rm -rf /var/lib/apt/lists/*

# Copy requirements first to leverage Docker layer caching
COPY requirements.txt .

# Install Python dependencies
RUN pip install --no-cache-dir -r requirements.txt

# Copy the source code of the Parser to the container
COPY . .

# Expose the port the service listens on
EXPOSE 8080

# Define the command to start the service using Uvicorn
# This keeps the process running and ensures the app is served correctly
CMD ["uvicorn", "main:app", "--host", "0.0.0.0", "--port", "8080"]