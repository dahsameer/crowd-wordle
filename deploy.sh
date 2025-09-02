#!/usr/bin/env bash
set -e

FRONTEND_DIR="/var/www/crowdwordle/frontend"
BACKEND_DIR="/var/www/crowdwordle/backend"
SERVICE_NAME="crowdwordle"

echo "Select which project(s) to deploy:"
echo "1) Frontend"
echo "2) Backend"
echo "3) Both"
read -rp "Enter your choice (1/2/3): " choice

deploy_frontend() {
  echo "🚀 Deploying frontend..."
  pushd frontend > /dev/null
  
  echo "🔹 Installing dependencies..."
  pnpm install --frozen-lockfile

  echo "🔹 Building frontend..."
  pnpm build

  echo "🔹 Deploying to $FRONTEND_DIR..."
  sudo mkdir -p "$FRONTEND_DIR"
  sudo rm -rf "$FRONTEND_DIR/*"
  sudo cp -r dist/* "$FRONTEND_DIR/"
  
  popd > /dev/null
  echo "✅ Frontend deployed!"
}

deploy_backend() {
  echo "🚀 Deploying backend..."
  pushd backend > /dev/null

  echo "🔹 Stopping service: $SERVICE_NAME"
  sudo systemctl stop "$SERVICE_NAME" || true

  echo "🔹 Building backend (Native AOT)..."
  dotnet publish CrowdWordle/CrowdWordle.csproj -c Release -p:PublishAot=true -o publish

  echo "🔹 Deploying to $BACKEND_DIR..."
  sudo mkdir -p "$BACKEND_DIR"
  sudo rm -rf "$BACKEND_DIR/*"
  sudo cp -r publish/* "$BACKEND_DIR/"

  echo "🔹 Restarting service: $SERVICE_NAME"
  sudo systemctl start "$SERVICE_NAME"
  
  popd > /dev/null
  echo "✅ Backend deployed!"
}

case $choice in
  1) deploy_frontend ;;
  2) deploy_backend ;;
  3) deploy_frontend; deploy_backend ;;
  *) echo "Invalid choice!" && exit 1 ;;
esac

echo "🎉 Deployment complete!"
