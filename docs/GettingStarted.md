# Getting Started with Echo

**Written by:** @clintonbampoe
**Last updated:** 2026-08-01 by @clintonbampoe

---

## Requirements

- Docker
- Git

---

## 1. Clone the repo

```bash
git clone https://github.com/clintonbampoe/echo.git
```

---

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

---

## 3. Building container images

Echo has a stack that consists of multiple containers (database, nginx, api, etc. See [Infrastructure](./Infrastructure.md) for more details)). Some of these containers run on custom images.

For example, the **api** image is a custom image that runs a multistage build process.
Hence, in order to run them you would need to build the image first, so that Docker can cache and reuse it for later sessions.

We have two environments in which our containers can run in: 
- **Development** 
- **Production**

**Development** is the default environment and it is the image docker builds when you run the command:
```bash
docker compose build
```
This builds our custom docker images for the dev environment.

To build the images for production, you'll need to explicitly pass the production file override `docker-compose.prod.yml` (*located at the root of the repository*) as a flag during the `compose build`

**example**
```bash
docker compose -f docker-compose.yml -f docker-compose.prod.yml build
```

This would pull all the dependencies it needs to build our images and prepare your containers for execution.

> One first run, the build might take a while (*about 5 minutes or so depending on your internet connectivity speed*). This is because, docker is pulling all the sdks, and dependencies it needs to run the containers. Rest assured this process only happens once. After that, the dependencies and images are cached and reused for later builds.
> 
> **THIS IMPLIES THAT YOU'LL NEED INTERNET CONNECTION FOR THE FIRST BUILD**

---

## 4. Starting up containers

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

> **Note**: The `-d` flag is to run it detached mode.

- By default, migrations run automatically when the API container starts.
  (`RUN_DATABASE_MIGRATIONS_ON_STARTUP=true` in `.env`).

- To disable automatic migrations and run them manually instead, set that
  value to `false` in `.env`, then run:

  ```bash
  docker-compose run --rm migrator
  ```

---

## 5. Verify

Once the containers are up, the API is reachable at:

- **Dev** `http://localhost:5025` (directly) or `http://localhost:8080` (through Nginx)
- **Prod** `http://localhost:80` (through Nginx only -- the API isn't exposed directly)

To confirm it's working, you can test the health endpoint by using:

- Curl

  ```bash
  curl http://localhost:8080/api/health/ready
  ```

- Or you can type this in the browser url bar `http://localhost:8080/api/health/ready`

---

## Related Documentation

- [Infrastructure](./Infrastructure.md)
- [Echo API](./api/ApiUsage.md)
- [Front-end](./frontend/README.md)
