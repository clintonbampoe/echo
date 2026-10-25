# Echo Web Client

**Written by:** @clintonbampoe
**Last updated:** 2026-10-14 by @clintonbampoe

React 19 + TypeScript + Vite. How it is organised and the rules it follows: [Architecture](./architecture.md).

## Run it

Needs Node 24 (matches CI).

```bash
cd frontend
npm install
cp .env.example .env.local   # points at the API on localhost:5025
npm run dev                  # http://localhost:5173
```

## In Docker

One nginx serves the whole app (see [Infrastructure](../infrastructure/web-app.md)).

| | Dev | Prod |
|---|---|---|
| Command | `docker compose up` | `docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build` |
| Web app | `http://localhost:8080` through nginx, or `:5173` straight from Vite | `http://localhost` |
| Served by | Vite (hot reload), passed through by nginx | the built files, inside the nginx image |

In dev, the app calls the API at the `VITE_API_URL` in `.env.local`. If you use `:8080`, add
`http://localhost:8080` to `Cors__AllowedOrigins`, or set `VITE_API_URL=/api` to go through nginx.

Opening `dist/index.html` from disk shows a white page: its files are loaded from `/assets/` and the router
needs a server. To look at a production build without Docker, run `npm run build && npm run preview`.

## Scripts

| Command | What it does |
|---|---|
| `npm run dev` | dev server |
| `npm run check` | lint + typecheck + tests + build (run before pushing) |
| `npm run lint` | ESLint, including the layer and size rules |
| `npm run typecheck` | `tsc -b` |
| `npm test` | Vitest, once |
| `npm run gen:icons` | regenerate `src/ui/layout/Icon.tsx` from `scripts/icons.json` |
