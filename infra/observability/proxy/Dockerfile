FROM nginx:alpine

COPY infra/observability/proxy/default.conf.template /etc/nginx/templates/default.conf.template
COPY infra/nginx/tls.conf /etc/nginx/tls.conf
COPY --chmod=755 infra/nginx/reload-on-renewal.sh /docker-entrypoint.d/90-reload-on-renewal.sh
