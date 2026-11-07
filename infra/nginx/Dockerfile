
FROM node:24-alpine AS web-deps
WORKDIR /app
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

FROM web-deps AS web-build
ARG VITE_API_URL=/api
ENV VITE_API_URL=$VITE_API_URL
COPY frontend/ .
RUN npm run lint
RUN npm run build

FROM nginx:alpine AS base
RUN rm -rf /usr/share/nginx/html/*
COPY infra/nginx/nginx.conf /etc/nginx/nginx.conf
COPY infra/nginx/api-locations.conf /etc/nginx/api-locations.conf
COPY infra/nginx/spa-headers.conf /etc/nginx/spa-headers.conf
COPY infra/nginx/tls.conf /etc/nginx/tls.conf

FROM base AS dev
COPY infra/nginx/site.dev.conf /etc/nginx/site.conf

FROM base AS web
ENV NGINX_ENVSUBST_OUTPUT_DIR=/etc/nginx
COPY --chmod=755 infra/nginx/reload-on-renewal.sh /docker-entrypoint.d/90-reload-on-renewal.sh
COPY infra/nginx/grafana-locations.conf /etc/nginx/grafana-locations.conf
COPY infra/nginx/site.conf.template /etc/nginx/templates/site.conf.template
COPY --from=web-build /app/dist /usr/share/nginx/html
