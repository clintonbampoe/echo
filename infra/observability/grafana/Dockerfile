FROM grafana/grafana:12.0.0

COPY --chown=472:472 infra/observability/grafana/grafana.ini \
  /etc/grafana/grafana.ini
COPY --chown=472:472 infra/observability/grafana/provisioning/ \
  /etc/grafana/provisioning/
COPY --chown=472:472 infra/observability/grafana/dashboards/ \
  /etc/grafana/dashboards/
