FROM postgres:18

COPY infra/postgres/postgresql.conf /etc/postgresql/postgresql.conf
COPY infra/postgres/conf/ /etc/postgresql/conf.d/
COPY infra/postgres/init/extensions.sql /docker-entrypoint-initdb.d/extensions.sql
COPY --chmod=755 infra/postgres/init/monitor-role.sh /docker-entrypoint-initdb.d/monitor-role.sh

CMD ["postgres", "-c", "config_file=/etc/postgresql/postgresql.conf"]
