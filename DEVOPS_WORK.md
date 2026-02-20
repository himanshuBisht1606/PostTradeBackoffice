# Post-Trade Backoffice — DevOps Reference

> **Architecture:** Docker Compose on OCI Always Free VMs + Supabase PostgreSQL + ghcr.io
>
> **Rule:** Read this file at the start of every DevOps session.

---

## Infrastructure Overview

| Component | Dev | QA |
|-----------|-----|----|
| VM shape | `VM.Standard.E2.1.Micro` (1 OCPU / 1 GB RAM) | `VM.Standard.E2.1.Micro` (1 OCPU / 1 GB RAM) |
| VM OS | Oracle Linux 8 | Oracle Linux 8 |
| VM user | `opc` | `opc` |
| Public IP | `80.225.204.132` | _(set after QA VM created)_ |
| Container runtime | Docker CE 26.x | Docker CE 26.x |
| Compose file | `docker-compose.dev.yml` | `docker-compose.qa.yml` |
| Image tag | `ghcr.io/himanshubisht1606/posttrade-api:dev-latest` | `ghcr.io/himanshubisht1606/posttrade-api:qa-latest` |
| Database | Supabase free project (Dev) | Supabase free project (QA) — separate project |
| ASPNETCORE_ENVIRONMENT | `Development` | `Staging` |
| Scalar UI | `http://80.225.204.132/scalar/v1` | `http://<QA_IP>/scalar/v1` |
| Health check | `http://80.225.204.132/health` | `http://<QA_IP>/health` |

---

## Pipeline Triggers

| Workflow | Trigger | File |
|----------|---------|------|
| Deploy — Dev | Push to any branch **except** `main` | `.github/workflows/deploy-dev.yml` |
| Deploy — QA | Push/merge to `main` | `.github/workflows/deploy-qa.yml` |

Both workflows: run unit tests → build & push Docker image to ghcr.io → SCP compose file → SSH deploy → health check verify.

---

## GitHub Secrets

> Settings → Secrets and variables → Actions → Repository secrets

| Secret | Dev | QA | Notes |
|--------|-----|----|-------|
| `DEV_VM_HOST` | `80.225.204.132` | — | Dev VM public IP |
| `QA_VM_HOST` | — | _(set after QA VM created)_ | QA VM public IP |
| `VM_SSH_KEY` | shared | shared | Contents of `ssh-key-2026-02-20 (1).key` — same key for both VMs |
| `DEV_DB_HOST` | `aws-1-ap-south-1.pooler.supabase.com` | — | Supabase Dev project host |
| `DEV_DB_PORT` | `5432` | — | Direct connection port (not 6543 pooler) |
| `DEV_DB_NAME` | `postgres` | — | |
| `DEV_DB_USER` | `postgres.jplnnxygfguucfrfyurk` | — | |
| `DEV_DB_PASS` | _(Supabase Dev password)_ | — | |
| `QA_DB_HOST` | — | _(Supabase QA project host)_ | |
| `QA_DB_PORT` | — | `5432` | |
| `QA_DB_NAME` | — | `postgres` | |
| `QA_DB_USER` | — | _(Supabase QA project user)_ | |
| `QA_DB_PASS` | — | _(Supabase QA password)_ | |
| `JWT_SECRET_KEY` | shared | shared | Same key for both environments |
| `GITHUB_TOKEN` | auto | auto | Provided by GitHub Actions — no setup needed |

---

## Status

| Environment | VM | Supabase DB | Docker | Pipeline | Scalar UI |
|-------------|----|----|--------|----------|-----------|
| **Dev** | ✅ Done | ✅ Done | ✅ Done | ✅ Done | ✅ `http://80.225.204.132/scalar/v1` |
| **QA** | ⏳ Pending | ⏳ Pending | ⏳ Pending | ⏳ Pending | ⏳ Pending |

---

## QA Environment Setup (Step-by-Step)

> Follow these steps when you are ready to set up the QA environment.
> All steps mirror what was done for Dev.

### Step 1 — Create QA VM on OCI

1. OCI Console → Compute → Instances → **Create Instance**
2. Name: `posttrade-qa-vm`
3. Image: **Oracle Linux 8**
4. Shape: **VM.Standard.E2.1.Micro** (Always Free eligible — under Specialty and Previous Generation)
5. Networking:
   - Create or select a VCN
   - Assign a **public IP**
6. SSH keys: Upload the **same public key** (from `ssh-key-2026-02-20 (1).key`) — reuse it
7. Boot volume: default (46.6 GB is fine)
8. Click **Create**
9. Note the **Public IP address** once the instance is Running

### Step 2 — Open Ports in OCI Security List

OCI Console → Networking → Virtual Cloud Networks → QA VM's VCN → Security Lists → Default Security List → Add Ingress Rules:

| Protocol | Source CIDR | Port | Purpose |
|----------|-------------|------|---------|
| TCP | `0.0.0.0/0` | `22` | SSH |
| TCP | `0.0.0.0/0` | `80` | HTTP (API + Scalar) |

### Step 3 — SSH into QA VM and Set Up Docker

```bash
ssh -i "ssh-key-2026-02-20 (1).key" opc@<QA_VM_PUBLIC_IP>
```

Once connected, run each block in order:

**3a — Add swap (required — OOM killer kills Docker install on 1 GB RAM):**
```bash
sudo fallocate -l 2G /swapfile
sudo chmod 600 /swapfile
sudo mkswap /swapfile
sudo swapon /swapfile
echo '/swapfile none swap sw 0 0' | sudo tee -a /etc/fstab
```

**3b — Disable slow OCI repo (prevents dnf timeouts):**
```bash
sudo dnf config-manager --disable ol8_ksplice 2>/dev/null || true
```

**3c — Install Docker CE:**
```bash
sudo dnf config-manager --add-repo https://download.docker.com/linux/centos/docker-ce.repo
sudo dnf install -y docker-ce docker-ce-cli containerd.io --allowerasing
sudo systemctl enable --now docker
sudo usermod -aG docker opc
```

Log out and back in for the group change to take effect:
```bash
exit
ssh -i "ssh-key-2026-02-20 (1).key" opc@<QA_VM_PUBLIC_IP>
docker --version   # should show Docker version 26.x
```

**3d — Open port 80 in VM firewall:**
```bash
sudo firewall-cmd --permanent --add-port=80/tcp && sudo firewall-cmd --reload
```

**3e — Create working directory:**
```bash
mkdir -p ~/posttrade
```

### Step 4 — Create Supabase QA Project

1. Go to [supabase.com](https://supabase.com) → New project
2. Name: `posttrade-qa` (or similar)
3. Set a **database password** (save it — you'll need it)
4. Region: closest to OCI Mumbai (e.g. `ap-southeast-1` Singapore or `ap-south-1`)
5. After project is created → **Project Settings → Database**
6. Copy the **Direct connection** values (NOT the pooler on port 6543):
   - Host: `aws-0-<region>.pooler.supabase.com` (or direct host)
   - Port: `5432`
   - Database: `postgres`
   - User: `postgres.<project-ref>`
   - Password: _(what you set in step 3)_

### Step 5 — Add QA GitHub Secrets

Go to GitHub repo → Settings → Secrets and variables → Actions → add these:

| Secret name | Value |
|-------------|-------|
| `QA_VM_HOST` | QA VM public IP from Step 1 |
| `QA_DB_HOST` | Supabase QA host from Step 4 |
| `QA_DB_PORT` | `5432` |
| `QA_DB_NAME` | `postgres` |
| `QA_DB_USER` | Supabase QA user from Step 4 |
| `QA_DB_PASS` | Supabase QA password from Step 4 |

> `VM_SSH_KEY` and `JWT_SECRET_KEY` are already set from Dev — no need to re-add.

### Step 6 — Verify Manually on QA VM (Before Triggering Pipeline)

SSH into the QA VM and write a test `.env`:

```bash
cd ~/posttrade

printf 'DB_HOST=%s\nDB_PORT=%s\nDB_NAME=%s\nDB_USER=%s\nDB_PASS=%s\nJWT_SECRET_KEY=%s\n' \
  "<QA_DB_HOST>" "5432" "postgres" "<QA_DB_USER>" "<QA_DB_PASS>" "<JWT_SECRET_KEY>" > .env

cat .env   # verify values are correct
```

Pull and run the image manually as a smoke test:
```bash
echo "<GITHUB_TOKEN>" | docker login ghcr.io --username himanshubisht1606 --password-stdin
docker pull ghcr.io/himanshubisht1606/posttrade-api:qa-latest
```

> Note: `qa-latest` only exists after the first QA pipeline run. If it doesn't exist yet, trigger the pipeline in Step 7 first, then come back to verify.

### Step 7 — Trigger QA Pipeline

Merge a PR into `main` (or push directly to `main`):
```bash
git checkout main
git push origin main
```

The pipeline will:
1. Run unit tests
2. Build & push `posttrade-api:qa-latest` to ghcr.io
3. SCP `docker-compose.qa.yml` to the QA VM
4. SSH into VM → write `.env` from secrets → `docker compose up`
5. Health check: `curl http://localhost/health`

### Step 8 — Verify QA Deployment

```bash
# On the QA VM:
docker ps                                  # posttrade-api-qa should be Up
docker logs posttrade-api-qa --tail 20     # check for DB connection errors
curl http://localhost/health               # should return Healthy
```

From your browser or local machine:
- `http://<QA_VM_PUBLIC_IP>/health`
- `http://<QA_VM_PUBLIC_IP>/scalar/v1`

> Note: Scalar UI is NOT available in QA — `ASPNETCORE_ENVIRONMENT: Staging` disables it.
> Only the health endpoint and API endpoints are accessible.

---

## Key Lessons & Gotchas

| Issue | Fix |
|-------|-----|
| Oracle Linux 8 ships Podman, not Docker | Install Docker CE from `download.docker.com/linux/centos/docker-ce.repo` with `--allowerasing` |
| Docker install killed on 1 GB VM | Add 2 GB swap **before** installing Docker |
| `dnf` timeouts | Disable `ol8_ksplice` repo: `sudo dnf config-manager --disable ol8_ksplice` |
| ghcr.io image name must be lowercase | Use `tr '[:upper:]' '[:lower:]'` in workflow — `himanshubisht1606` not `himanshuBisht1606` |
| `permissions: packages: write` required | Add to workflow YAML or ghcr.io push is denied |
| Docker Compose doesn't auto-load `.env` with non-default filename | Always use `--env-file .env` flag with `-f docker-compose.dev.yml` |
| `@` or `=` in password breaks ADO.NET connection string | Use separate component vars (`DB_HOST`, `DB_PORT`, etc.) not a full connection string secret |
| `appleboy/ssh-action` env vars need `envs:` list | List every var in the `envs:` parameter, otherwise they're not passed to remote script |
| Port 80 blocked after VM creation | Open in OCI Security List AND run `sudo firewall-cmd --permanent --add-port=80/tcp` |
| Supabase port 6543 (pooler) may cause Npgsql issues | Use Direct connection port `5432` |
| `ALREADY_ENABLED` warning from firewalld | Not an error — port was already open |

---

## Useful Commands (On VM)

```bash
# Check container status
docker ps

# View live logs
docker logs posttrade-api-dev -f

# Restart container
docker compose -f docker-compose.dev.yml --env-file .env up -d --force-recreate

# Verify env file is loading correctly
docker compose -f docker-compose.dev.yml --env-file .env config | grep ConnectionStrings

# Test health endpoint
curl http://localhost/health

# Clean up old images
docker image prune -f
```

---

*Last updated: 2026-02-20 | Dev: ✅ Complete | QA: ⏳ Pending*
