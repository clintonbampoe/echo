FROM grafana/loki:3.5.0

USER 10001
COPY --chown=10001:10001 loki-config.yml /etc/loki/config.yml

CMD ["-config.file=/etc/loki/config.yml"]
