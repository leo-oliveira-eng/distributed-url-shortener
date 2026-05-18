from os import getenv

from fastapi import FastAPI

app = FastAPI(docs_url=None, redoc_url=None, openapi_url=None)


@app.get("/health")
def health() -> dict[str, str]:
    return {
        "status": "Healthy",
        "service": "Statistics.Api",
        "environment": getenv("APP_ENVIRONMENT", "Development"),
    }


def main() -> None:
    import uvicorn

    uvicorn.run(
        "statistics_api.main:app",
        host=getenv("STATISTICS_API_HOST", "127.0.0.1"),
        port=int(getenv("STATISTICS_API_HTTP_PORT", "5103")),
        log_level=getenv("STATISTICS_API_LOG_LEVEL", "info"),
    )


if __name__ == "__main__":
    main()

