import os
import tempfile
import traceback
import logging
from contextlib import asynccontextmanager

from fastapi import FastAPI, UploadFile, File, HTTPException
from paddleocr import PaddleOCR
from PIL import Image

ocr: PaddleOCR | None = None

logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('/home/appuser/logs/app.log'),
        logging.StreamHandler(),
    ]
)


@asynccontextmanager
async def lifespan(app: FastAPI):
    global ocr
    ocr = PaddleOCR(
        lang=os.getenv("OCR_LANG", "en"),
        use_textline_orientation=True
    )
    yield


app = FastAPI(lifespan=lifespan)


@app.get("/health")
async def health():
    return {"status": "ok"}


@app.post("/ocr")
async def run_ocr(file: UploadFile = File(...)):
    logging.info("=" * 80)
    logging.info("OCR REQUEST START")
    logging.info(f"filename={file.filename}")
    logging.info(f"content_type={file.content_type}")
    logging.info(f"file.file={file.file}")
    logging.info(f"file.size={file.size}")

    data = await file.read()

    logging.info(f"received bytes={len(data)}")

    if not data:
        logging.error("ERROR: empty file")
        raise HTTPException(400, "empty file")

    suffix = os.path.splitext(file.filename or "")[1] or ".jpg"

    with tempfile.NamedTemporaryFile(delete=False, suffix=suffix) as tmp:
        tmp.write(data)
        path = tmp.name

    logging.info(f"temp file={path}")

    try:
        # ---------------------------------------------------------
        # IMAGE VALIDATION
        # ---------------------------------------------------------
        try:
            logging.info("Opening image with PIL...")

            with Image.open(path) as img:
                logging.info(
                    f"Image info: "
                    f"format={img.format}, "
                    f"mode={img.mode}, "
                    f"size={img.size}, "
                    f"frames={getattr(img, 'n_frames', 1)}"
                )

                img.verify()

            logging.info("PIL verify: OK")

        except Exception as e:
            logging.error("IMAGE VALIDATION FAILED")
            logging.info(f"{type(e).__name__}: {e}")
            logging.info(traceback.format_exc())

            raise HTTPException(
                status_code=400,
                detail=f"Invalid or corrupted image: {type(e).__name__}: {e}",
            )

        # ---------------------------------------------------------
        # OCR
        # ---------------------------------------------------------
        try:
            logging.info("Starting PaddleOCR.predict()")

            prediction = ocr.predict(path)

            logging.info("PaddleOCR.predict(): OK")
            logging.info(f"prediction type={type(prediction)}")

        except Exception as e:
            logging.error("OCR PREDICT FAILED")
            logging.info(f"{type(e).__name__}: {e}")
            logging.info(traceback.format_exc())

            raise HTTPException(
                status_code=500,
                detail=f"OCR processing failed: {type(e).__name__}: {e}",
            )

    finally:
        try:
            os.remove(path)
            logging.info(f"Deleted temp file: {path}")
        except Exception as e:
            logging.error(f"Failed to delete temp file: {e}")

    # -------------------------------------------------------------
    # PARSE OCR RESULT
    # -------------------------------------------------------------
    try:
        items = []

        for index, res in enumerate(prediction):
            logging.info(f"Processing OCR result #{index}")
            logging.info(f"res type={type(res)}")
            logging.info(f"res.json type={type(res.json)}")
            logging.info(f"res.json={res.json}")

            p = res.json.get("res", res.json)

            logging.info(f"OCR RAW KEYS: {list(p.keys())}")

            texts = p.get("rec_texts", [])
            scores = p.get("rec_scores", [])

            logging.info(f"rec_texts={texts}")
            logging.info(f"rec_scores={scores}")

            for text, score in zip(texts, scores):
                items.append({
                    "text": text,
                    "score": float(score),
                })

        logging.info(f"OCR ITEMS: {items}")

    except Exception as e:
        logging.error("OCR RESULT PARSING FAILED")
        logging.info(f"{type(e).__name__}: {e}")
        logging.info(traceback.format_exc())

        raise HTTPException(
            status_code=500,
            detail=f"OCR result parsing failed: {type(e).__name__}: {e}",
        )

    result = {
        "text": "\n".join(i["text"] for i in items),
        "items": items,
        "summary_score": (
            sum(i["score"] for i in items) / len(items)
            if items
            else 0.0
        ),
    }

    logging.info(f"OCR RESPONSE: {result}")
    logging.info("OCR REQUEST END")
    logging.info("=" * 80)

    return result