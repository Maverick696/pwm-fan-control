#!/bin/zsh
# Script per buildare e pushare l'immagine FanCommander su Docker Hub
# Usage: ./push-to-dockerhub.sh <dockerhub-username>

if [ -z "$1" ]; then
  echo "Usage: $0 <dockerhub-username>"
  exit 1
fi

USERNAME=$1
IMAGE_NAME=fan-commander
TAG=latest

cd ../FanCommander/FanCommander || exit 1

# Build immagine con Podman (o Docker)
podman build -t $IMAGE_NAME:$TAG . || exit 1

# Login a Docker Hub
podman login docker.io || exit 1

# Tag immagine per Docker Hub
podman tag $IMAGE_NAME:$TAG docker.io/$USERNAME/$IMAGE_NAME:$TAG

# Push immagine
podman push docker.io/$USERNAME/$IMAGE_NAME:$TAG

echo "Immagine pushata su Docker Hub come $USERNAME/$IMAGE_NAME:$TAG"
