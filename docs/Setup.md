# Setup

**Written by:** @clintonbampoe
**Last updated:** 2026-07-31 by @clintonbampoe

---

## Requirements

- Docker + Docker compose

## 1. Clone the repo

```bash
git clone https://github.com/clintonbampoe/echo.git
```

## 2. Configure secrets

- Copy the structure in the `.env.example` file into a new `.env` file. Speak to any of the maintainers of the repository for secrets and API keys.

- Fill in the secrets into the `.env` file and add it to the repository root.

- JWT keys are Base64 encoded. This is because normal PEM keys can have escape characters and other sequences that interfere with the `.env` file format. Hence, the reason for the encoding. Rest assured, every module that reads this key knows to decode it before use. The reader should also keep this in mind for any module they might add that would read the JWT keys.
  **NB:** The encoding is only for the **JWT Keys** due to the quirks of PEM keys. The rest of the secrets are just in their normal form.

- To set up a **JWT Key** pair on your device, Navigate to the the `/backend/tools/jwt-key-setup` directory in the repository.
  And run this command:

  ```bash
  sh setup-jwt-keys.sh env
  ```

  The script would then check the `.env` file at the repo root to see if the public and private keys for JWT have been configured. It would then ask user permission to override if yes, or it would just continue if the key pair have not been generated yet.

**NB:** Please note that if you're in a windows environment, unless you've configured an interpreter for bash or using a shell like **Git Bash** or **WSL**, I can't guarantee that this script would run. In that case, contact the maintainer for a **JWT Key Pair**

## 3. Start up containers

To start up the app containers and services, you must have docker installed on your system.
Navigate to the repository root to run the following commands.

- **For Development**:

  ```bash
  docker compose up
  ```

  with no flags

- **For Production**:

  ```bash
  docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d
  ```

- By default, migrations run automatically when the API container starts
  (`RUN_DATABASE_MIGRATIONS_ON_STARTUP=true` in `.env`).

- To disable automatic migrations and run them manually instead, set that
  value to `false` in `.env`, then run:

  ```bash
  docker-compose run --rm migrator
  ```

## 4. Verify

Once the containers are up, the API is reachable at:

- **Dev** `http://localhost:5025` (directly) or `http://localhost:8080` (through Nginx)
- **Prod** `http://localhost:80` (through Nginx only -- the API isn't exposed directly)

To confirm it's working, you can test the health endpoint by using:

- Curl

  ```bash
  curl http://localhost:8080/health/ready
  ```

- Or you can type this in the browser url bar `http://localhost:8080/health/ready`
