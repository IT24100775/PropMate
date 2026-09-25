from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from api.maintenance_ai_api import router as maintenance_ai_router


app = FastAPI(
    title="PropMate Agentic AI",
    description="Agentic AI service for PropMate maintenance management",
    version="1.0.0"
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=[
        "http://localhost:5173",
        "http://127.0.0.1:5173",
    ],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


app.include_router(maintenance_ai_router)


@app.get("/")
def root():
    return {
        "message": "PropMate Agentic AI is running"
    }


@app.get("/health")
def health():
    return {
        "status": "healthy"
    }