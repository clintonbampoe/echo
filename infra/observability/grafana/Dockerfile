FROM grafana/grafana:12.0.0

USER root
RUN mkdir -p /var/lib/grafana /var/log/grafana && chown -R 472:472 /var/lib/grafana /var/log/grafana
    
USER 472

COPY --chown=472:472 grafana.ini /etc/grafana/grafana.ini
COPY --chown=472:472 provisioning/ /etc/grafana/provisioning/
COPY --chown=472:472 dashboards/ /etc/grafana/dashboards/
