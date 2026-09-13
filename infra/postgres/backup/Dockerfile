# Extends the official Postgres image with cron support.
# Uses the Debian-based image to match the db service exactly —

FROM postgres:18

COPY infra/postgres/backup/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# backup.sh is mounted at runtime — changes don't require a rebuild
CMD [ "/entrypoint.sh" ]
