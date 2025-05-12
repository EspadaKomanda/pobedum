FROM python:3.10-slim

RUN apt-get update && apt-get install -y curl ffmpeg

WORKDIR /dir

COPY requirements.txt .

RUN pip install --no-cache-dir -r requirements.txt

COPY . .

WORKDIR /dir

CMD ["python", "-m", "app.main"]
