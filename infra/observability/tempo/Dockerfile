FROM grafana/tempo:3.1.0

USER 10001
COPY --chown=10001:10001 infra/observability/tempo/tempo.yaml \
  /etc/tempo/config.yaml

CMD ["-config.file=/etc/tempo/config.yaml", "-config.expand-env=true"]
