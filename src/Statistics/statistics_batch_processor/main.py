from os import getenv


def main() -> None:
    environment = getenv("APP_ENVIRONMENT", "Development")
    print(f"Statistics.BatchProcessor.Worker shell started in {environment}.")


if __name__ == "__main__":
    main()

