# One nginx for the whole app: it proxies /api to the API and serves the web client.
# Build context is the repo root (see docker-compose.yml).

FROM node:24-alpine AS web-deps
WORKDIR /app
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

FROM web-deps AS web-build
# Baked into the bundle at build time. nginx serves the API on the same origin.
ARG VITE_API_URL=/api
ENV VITE_API_URL=$VITE_API_URL
COPY frontend/ .
RUN npm run lint
RUN npm run build

# What both environments share. The image ships a "Welcome to nginx!" page; remove it.
FROM nginx:alpine AS base
RUN rm -rf /usr/share/nginx/html/*
COPY infra/nginx/nginx.conf /etc/nginx/nginx.conf
COPY infra/nginx/api-locations.conf /etc/nginx/api-locations.conf
COPY infra/nginx/spa-headers.conf /etc/nginx/spa-headers.conf
COPY infra/nginx/tls.conf /etc/nginx/tls.conf

# Development: pass the web app through to Vite. The Node stages above are skipped.
FROM base AS dev
COPY infra/nginx/site.dev.conf /etc/nginx/site.conf

# Production: serve the built web app over HTTPS. Last stage, so it is the default.
# The site file is a template: at start-up the image fills in APP_DOMAIN and writes it to /etc/nginx/site.conf.
FROM base AS web
ENV NGINX_ENVSUBST_OUTPUT_DIR=/etc/nginx
COPY infra/nginx/site.conf.template /etc/nginx/templates/site.conf.template
COPY --from=web-build /app/dist /usr/share/nginx/html
