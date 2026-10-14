# Development only: installs dependencies for the Vite dev server container
# (see docker-compose.override.yml). The production build is in infra/nginx/Dockerfile.
FROM node:24-alpine
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
