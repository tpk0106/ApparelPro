# ApparelPro Docker Dev Environment — Setup Guide (your machine)

Wired specifically to your actual folders:

- Backend solution: `C:\ApparelPro` (`ApparelPro.sln` — BusinessLogic, Data, Shared, WebApi)
- Frontend project: `C:\ap-fe\apparelpro-front-end-react`
- SQL Server: containerized, restored from a `.bak` backup of your live SQLEXPRESS database

All commands below run **on your PC** (PowerShell or a WSL terminal) — I placed the files, but Docker Desktop, Visual Studio, and VS Code all need to actually execute the commands locally; that part I can't do from here.

---

## Step 1 — Prerequisites (skip anything already installed)

1. **WSL2**: PowerShell (Administrator) → `wsl --install`, reboot if prompted.
2. **Docker Desktop**, with "Use WSL 2 based engine" checked during setup.
3. **VS Code** + the **"Dev Containers"** extension (`ms-vscode-remote.remote-containers`).
4. **Visual Studio 2022** (17.8+) — already installed, nothing to change there.

---

## Step 2 — Back up your live database

Your data currently sits as `.mdf`/`.ldf` files under `C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\` — those can't be copied directly (protected folder, and locked while SQLEXPRESS is running). Instead, take one proper backup:

Open SQL Server Management Studio (or `sqlcmd`) connected to your local SQLEXPRESS instance and run:

```sql
BACKUP DATABASE [YourLocalDatabaseName]
TO DISK = 'C:\ApparelPro\db\backup\ApparelPro.bak'
WITH FORMAT, INIT, NAME = 'ApparelPro-Full', STATS = 10;
```

Replace `YourLocalDatabaseName` with your actual database name (check SSMS's Object Explorer if unsure). This drops the backup straight into `db\backup\`, which I've already wired the container to look at automatically (see `db/scripts/entrypoint.sh`).

---

## Step 3 — Start SQL Server + the frontend

From `C:\ApparelPro`:

```powershell
docker compose up -d sqlserver frontend
```

*What happens:* Docker pulls the SQL Server 2022 image and builds the frontend's dev image. The SQL container's entrypoint script waits for SQL Server to come up, checks whether it's already restored (via a marker file, so this is safe to re-run), and if your `.bak` is present, restores it as `ApparelProDb` (or whatever `DB_NAME` you set in `.env`) — resolving the original Windows file paths to container-native ones automatically.

Watch it happen:

```powershell
docker compose logs -f sqlserver
```

You should see `[entrypoint] Restoring ApparelProDb from ...` then SQL Server's own restore progress, ending with `[entrypoint] Restore complete.`

Verify with any SQL client pointed at `localhost,1433`, user `sa`, password from `.env` (`ApparelPro!Dev2026#` unless you changed it):

```sql
SELECT name FROM sys.databases;
```

---

## Step 4 — Backend: Visual Studio on your host (default path)

1. Open `C:\ApparelPro\ApparelPro.sln` in Visual Studio as usual.
2. Update your connection string (wherever `ApparelPro.WebApi` reads it — typically `appsettings.Development.json`) to:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=ApparelProDb;User Id=sa;Password=ApparelPro!Dev2026#;TrustServerCertificate=True"
}
```

3. F5 as normal. Visual Studio debugs on your host's own .NET runtime exactly as before — the only change is which SQL Server it's talking to.

### Advanced path — run the backend itself in a container too

Only if you want the backend's SDK/tool versions pinned in a container as well:

```powershell
docker compose --profile with-backend-container up -d --build backend
```

Then in Visual Studio: right-click the WebApi project → **Add → Container Orchestrator Support → Docker Compose**, pointing at this same `docker-compose.yml`. F5 will then build/run/debug inside the container via `vsdbg` (already installed in `.devcontainer/Dockerfile`), while breakpoints and the UI stay in Visual Studio on your host.

---

## Step 5 — Frontend: attach VS Code via Dev Containers

1. In VS Code: **File → Open Folder** → `C:\ap-fe\apparelpro-front-end-react`.
2. Click **"Reopen in Container"** when prompted (or Command Palette → **Dev Containers: Reopen in Container**).
3. VS Code reopens attached inside the `frontend` container — its terminal, ESLint/Prettier/Tailwind extensions (pre-listed in `.devcontainer/devcontainer.json`) all run using the container's Node, not your host's. `postCreateCommand` runs `npm install` automatically on first attach — this repopulates `node_modules` fresh inside the container (your existing Windows-built `node_modules` isn't reused, since native modules compiled for Windows generally don't work inside a Linux container; your host copy is untouched).
4. Once attached, in the integrated terminal:

```bash
npm run dev -- --host 0.0.0.0
```

`--host 0.0.0.0` is required — otherwise Vite only listens inside the container and the port-forward to `http://localhost:5173` on your desktop browser won't reach it.

The API base URL is already wired to `http://localhost:5000` via `VITE_API_BASE_URL` in `docker-compose.yml`, matching whichever backend path you used in Step 4.

---

## Step 6 — EF Core migrations

- **Host Visual Studio path:** Package Manager Console → `Add-Migration InitialCreate` → `Update-Database` (targets `localhost,1433` via your connection string).
- **Containerized backend path:** `docker compose exec backend bash`, then `dotnet ef migrations add InitialCreate` / `dotnet ef database update` (the `dotnet-ef` CLI tool is pre-installed in the image).

Per the `clipper-migration` skill's `DATAMODELS.md` registry, your `DbContext` should map to the documented entities (`OrderwiseStockMaster`, `PurchaseOrder`, `Style`, etc.) — since your `.bak` already contains real data, review the first migration's diff carefully before applying it, in case column/table names don't match exactly yet.

---

## Cheat sheet

| Action | Command |
|---|---|
| Start SQL + frontend | `docker compose up -d sqlserver frontend` |
| Also start the optional backend container | `docker compose --profile with-backend-container up -d backend` |
| Stop everything, keep data | `docker compose stop` |
| Remove containers, keep data volumes | `docker compose down` |
| **Wipe the database** (re-triggers restore next start) | `docker compose down -v` |
| Tail SQL Server logs | `docker compose logs -f sqlserver` |
| Shell into frontend container | `docker compose exec frontend bash` |
| Shell into backend container (if enabled) | `docker compose exec backend bash` |

---

## Troubleshooting

- **SQL container keeps restarting** — check `docker compose logs sqlserver`; usually `SA_PASSWORD` doesn't meet complexity rules.
- **Restore skipped** — the `.bak` must exist in `db\backup\` *before* the first `docker compose up`; also check `docker compose down -v` wasn't run since (that clears the marker along with all data, which is fine — it just means it'll try to restore again next start).
- **Frontend unreachable at `localhost:5173`** — confirm you ran `npm run dev -- --host 0.0.0.0`, not the plain default.
- **Visual Studio can't reach `localhost,1433`** — check `docker compose ps` shows `sqlserver` healthy, and that your existing local SQLEXPRESS service (still running on port 1433 on the host) isn't conflicting — if it is, either stop that Windows service while using the container, or change the container's host port mapping in `docker-compose.yml` (e.g. `"14330:1433"`) and adjust connection strings accordingly.
