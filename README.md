# Document Verification Pipeline

A KYC-like document verification pipeline built with **C#/.NET, Python, Docker, PostgreSQL, PaddleOCR and AWS Rekognition**.

The system provides:

* Document OCR using PaddleOCR
* MRZ extraction, parsing and validation
* AES encryption and HMAC lookup hashes
* Face comparison between selfie and document photo
* Dockerized microservices architecture

The .NET API acts as the main orchestrator, while OCR and face recognition run as separate services.

> Face matching alone is not sufficient to verify document authenticity. In production, this is typically handled by specialized third-party **document verification / identity verification providers** that offer **document authenticity checks, tampering detection, NFC verification, and liveness detection**. Due to limited resources, I decided not to implement the full document verification process from scratch.


```mermaid
flowchart TD
    Client[Client / Frontend] -->|POST /api/ocr/mrz-code| API[.NET API]
    
    API --> OCR_Service[OcrService.cs]
    OCR_Service --> OCR_Python_Microservice[
    Python OCR Microservice
    PaddleOCR
    ]
    OCR_Python_Microservice --> OCR_Service
    OCR_Service --> MRZ_Parsing[MRZ Parsing and Validation]
    MRZ_Parsing --> AES_Ecryption[AES Encryption + HMAC Lookup Hash]
    AES_Ecryption --> Database[(PostgreSQL)]

    Client[Client / Frontend] -->|POST /api/face/compare| API[.NET API]
    API --> Face_Compare[Face Compare]
    Face_Compare --> Face_Recognition[
    Face Recognition Microservice
    ]
```