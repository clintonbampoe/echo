# Extends the official Postgres image with cron support.
# Uses the Debian-based image to match the db service exactly —

FROM postgres:18

RUN apt-get update && apt-get install -y --no-install-recommends cron nano \
    && rm -rf /var/lib/apt/lists/*

RUN export EDITOR=nano

# Script is mounted at runtime via compose volume, not baked in —
# so changes to backup.sh don't require rebuilding this image.
CMD [ "cron", "-f" ]
