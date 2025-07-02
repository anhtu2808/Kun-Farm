#!/bin/bash

echo "🚀 KunFarm Deploy"

# Vào thư mục deploy
cd /var/docker/kunfarm-app || exit 1

# Load env
source .env

# Pull image mới
echo "📦 Pull latest image..."
docker-compose pull kunfarm-api

# Restart
echo "🔄 Restart service..."
docker-compose up -d --force-recreate kunfarm-api

# Check
sleep 10
if curl -f http://localhost:5270 &>/dev/null; then
    echo "✅ Deploy thành công!"
else
    echo "❌ Deploy thất bại!"
fi

docker-compose ps 