FROM grafana/alloy:v1.21.0

COPY config.alloy /etc/alloy/config.alloy

CMD ["run", "--server.http.listen-addr=0.0.0.0:12345", "--storage.path=/var/lib/alloy", "/etc/alloy/config.alloy"]
