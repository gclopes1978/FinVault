# Use the official Python image as base
FROM python:3.12-slim

# Set the working directory inside the container
WORKDIR /app

# Install system dependencies required for PDF processing (e.g., poppler)
# Remove lists to keep the image size small
RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential \
    && rm -rf /var/lib/apt/lists/*

# Copy only the requirements file first to leverage Docker layer caching
COPY requirements.txt .

# Install Python dependencies
RUN pip install --no-cache-dir -r requirements.txt

# Copy the source code of the Parser to the container
COPY . .

# Define the command to start the parser service
# Example: If it runs as a continuous service listening for files
CMD ["python", "main.py"]