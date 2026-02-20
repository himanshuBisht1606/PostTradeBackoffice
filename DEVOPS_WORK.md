# Post-Trade Backoffice — DevOps Work Plan

> This file tracks the phase-wise DevOps setup for deploying the .NET 8 Web API
> and PostgreSQL on Oracle Cloud Free VMs using Docker Compose.
>
> **Approach:** Docker Compose on 2 OCI Always Free A1.Flex VMs (Dev + QA).
> GitHub Container Registry (ghcr.io) used for Docker images — free, no OCIR needed.
>
> **Rule:** Read this file at the start of every DevOps session.
> Update status as phases are completed.

---

## Quick Reference

| Item | Value |
|------|-------|
| GitHub repo | `himanshuBisht1606/PostTradeBackoffice` |
| Container registry | `ghcr.io/himanshubisht1606/posttrade-backoffice/posttrade-api` |
| Dev image tag | `dev-latest` |
| QA image tag | `qa-latest` |
| Dev VM user | `opc` |
| QA VM user | `opc` |
| Dev VM host | _(fill after VM is created)_ → GitHub Secret: `DEV_VM_HOST` |
| QA VM host | _(fill after VM is created)_ → GitHub Secret: `QA_VM_HOST` |
| SSH key | _(generated during VM creation)_ → GitHub Secret: `VM_SSH_KEY` |
| Dev DB password | _(you choose)_ → GitHub Secret: `DEV_DB_PASSWORD` |
| QA DB password | _(you choose)_ → GitHub Secret: `QA_DB_PASSWORD` |
| JWT secret key | _(you choose, 32+ chars)_ → GitHub Secret: `JWT_SECRET_KEY` |

---

## GitHub Secrets Required

| Secret | Description |
|--------|-------------|
| `DEV_VM_HOST` | Public IP address of Dev VM (OCI E2.1.Micro) |
| `QA_VM_HOST` | Public IP address of QA VM (OCI E2.1.Micro) |
| `VM_SSH_KEY` | SSH private key (used for both VMs) |
| `DEV_DB_CONNECTION` | Full Npgsql connection string for Dev (Supabase) |
| `QA_DB_CONNECTION` | Full Npgsql connection string for QA (Supabase) |
| `JWT_SECRET_KEY` | JWT signing secret (32+ chars) |
| `GITHUB_TOKEN` | Auto-provided by GitHub Actions — no setup needed |

---

## Phase Overview

| Phase | Scope | Trigger | Status |
|-------|-------|---------|--------|
| Phase 1 | GitHub Actions — Dev CI/CD | Push to any `feature/**` branch | ✅ Done — `.github/workflows/deploy-dev.yml` |
| Phase 2 | Docker Compose — Dev environment | Runs on Dev VM | ✅ Done — `docker-compose.dev.yml` |
| Phase 3 | GitHub Actions — QA CI/CD | Merge / push to `main` branch | ✅ Done — `.github/workflows/deploy-qa.yml` |
| Phase 4 | Docker Compose — QA environment | Runs on QA VM | ✅ Done — `docker-compose.qa.yml` |
| Phase 5 | OCI Free VM setup | Manual one-time setup | ⏳ Pending — manual steps on OCI Console |

---

## Phase 1 — GitHub Actions Workflow: Dev Deployment

**Status:** ⏳ Pending

**File to create:** `.github/workflows/deploy-dev.yml`

**Trigger:** Push to any branch matching `feature/**`

### Tasks

- [ ] **1.1 — Define workflow trigger**
  - Trigger on `push` to `feature/**` branches
  - Optionally allow `workflow_dispatch` for manual runs

- [ ] **1.2 — Add GitHub Secrets** (must be set in repo Settings → Secrets)
  - `OCI_CLI_USER` — OCI user OCID
  - `OCI_CLI_TENANCY` — OCI tenancy OCID
  - `OCI_CLI_FINGERPRINT` — OCI API key fingerprint
  - `OCI_CLI_KEY_CONTENT` — OCI private key (PEM, base64 or raw)
  - `OCI_CLI_REGION` — OCI region code (`<OCI_REGION>`)
  - `OCIR_USERNAME` — OCIR login (`<OCIR_NAMESPACE>/<oci-username>`)
  - `OCIR_PASSWORD` — OCI auth token (generated from OCI Console)
  - `OKE_CLUSTER_ID` — OKE cluster OCID (`<OKE_CLUSTER_ID>`)
  - `DEV_DB_PASSWORD` — PostgreSQL password for Dev (`<DEV_DB_PASSWORD>`)
  - `JWT_SECRET_KEY` — JWT signing secret (`<JWT_SECRET_KEY>`)

- [ ] **1.3 — Checkout step**
  - Use `actions/checkout@v4`

- [ ] **1.4 — Run unit tests**
  - Use `actions/setup-dotnet@v4` with .NET 8 SDK
  - Run `dotnet test` against `backend/tests/PostTrade.Tests/PostTrade.Tests.csproj`
  - Fail the pipeline if any tests fail

- [ ] **1.5 — Build and push Docker image**
  - Log in to OCIR using `docker login <DOCKER_REGISTRY>` with `OCIR_USERNAME` / `OCIR_PASSWORD`
  - Build image from `<DOCKERFILE_PATH>`
  - Tag image as:
    - `<DOCKER_REGISTRY>/<DOCKER_IMAGE_NAME>:dev-<GITHUB_SHA_SHORT>`
    - `<DOCKER_REGISTRY>/<DOCKER_IMAGE_NAME>:dev-latest`
  - Push both tags to OCIR

- [ ] **1.6 — Configure kubectl for OKE**
  - Install OCI CLI in the runner
  - Authenticate using OCI secrets
  - Run `oci ce cluster create-kubeconfig` to write `~/.kube/config`
  - Verify cluster connectivity with `kubectl cluster-info`

- [ ] **1.7 — Ensure Dev namespace exists**
  - Apply or create namespace `<DEV_NAMESPACE>` if not present
  - Use `kubectl apply` so it is idempotent

- [ ] **1.8 — Create / update Kubernetes Secrets for Dev**
  - Create or update a Secret named `posttrade-dev-db-secret` in `<DEV_NAMESPACE>` with:
    - `POSTGRES_PASSWORD` = `<DEV_DB_PASSWORD>`
    - `ConnectionStrings__DefaultConnection` = full connection string (built from placeholders)
  - Create or update a Secret named `posttrade-dev-jwt-secret` in `<DEV_NAMESPACE>` with:
    - `Jwt__Key` = `<JWT_SECRET_KEY>`
  - Use `kubectl create secret generic --dry-run=client -o yaml | kubectl apply -f -` pattern for idempotency

- [ ] **1.9 — Update image tag in Dev deployment manifest**
  - Use `sed` or `envsubst` to replace the image tag placeholder in the Dev API deployment manifest
    with `<DOCKER_REGISTRY>/<DOCKER_IMAGE_NAME>:dev-<GITHUB_SHA_SHORT>`

- [ ] **1.10 — Apply all Dev Kubernetes manifests**
  - Apply all manifests under `<K8S_MANIFESTS_DIR>/dev/` in this order:
    1. Namespace
    2. ConfigMap
    3. Secrets (if created by manifest rather than step 1.8)
    4. PostgreSQL PVC
    5. PostgreSQL Deployment
    6. PostgreSQL Service (ClusterIP — internal only)
    7. Web API Deployment
    8. Web API Service (LoadBalancer or NodePort)
  - Use `kubectl apply -f <K8S_MANIFESTS_DIR>/dev/`

- [ ] **1.11 — Wait for rollout and verify**
  - Run `kubectl rollout status deployment/posttrade-api -n <DEV_NAMESPACE>`
  - Run `kubectl rollout status deployment/postgres -n <DEV_NAMESPACE>`

---

## Phase 2 — Kubernetes Manifests: Dev Environment

**Status:** ⏳ Pending

**Folder to create:** `k8s/dev/`

**Files to create:**

```
k8s/dev/
  00-namespace.yaml
  01-configmap.yaml
  02-postgres-secret.yaml          ← optional (can be created by pipeline instead)
  03-postgres-pvc.yaml
  04-postgres-deployment.yaml
  05-postgres-service.yaml
  06-api-deployment.yaml
  07-api-service.yaml
```

### Tasks

- [ ] **2.1 — Namespace manifest** (`00-namespace.yaml`)
  - Create namespace `<DEV_NAMESPACE>`
  - Add label: `environment: dev`

- [ ] **2.2 — ConfigMap manifest** (`01-configmap.yaml`)
  - Name: `posttrade-dev-config`
  - Namespace: `<DEV_NAMESPACE>`
  - Keys to include:
    - `ASPNETCORE_ENVIRONMENT` = `Development`
    - `Jwt__Issuer` = `<JWT_ISSUER>`
    - `Jwt__Audience` = `<JWT_AUDIENCE>`
    - `Jwt__ExpiryMinutes` = `60`
    - `POSTGRES_DB` = `<DEV_DB_NAME>`
    - `POSTGRES_USER` = `<DEV_DB_USER>`
  - **Do not store passwords here** — passwords go in Secrets

- [ ] **2.3 — PostgreSQL Secret manifest** (`02-postgres-secret.yaml`) _(optional — can be injected by pipeline)_
  - Name: `posttrade-dev-db-secret`
  - Namespace: `<DEV_NAMESPACE>`
  - Type: `Opaque`
  - Keys (base64-encoded):
    - `POSTGRES_PASSWORD` = `<DEV_DB_PASSWORD>`
    - `ConnectionStrings__DefaultConnection` = full Npgsql connection string
      ```
      Host=postgres-service;Port=5432;Database=<DEV_DB_NAME>;
      Username=<DEV_DB_USER>;Password=<DEV_DB_PASSWORD>
      ```
  - Note: If managed by pipeline (step 1.8), this file may be a template with placeholders only

- [ ] **2.4 — PostgreSQL PVC manifest** (`03-postgres-pvc.yaml`)
  - Name: `postgres-dev-pvc`
  - Namespace: `<DEV_NAMESPACE>`
  - StorageClass: `<PVC_STORAGE_CLASS>`
  - AccessMode: `ReadWriteOnce`
  - Storage: `<PVC_SIZE_DEV>`

- [ ] **2.5 — PostgreSQL Deployment manifest** (`04-postgres-deployment.yaml`)
  - Name: `postgres`
  - Namespace: `<DEV_NAMESPACE>`
  - Replicas: `1`
  - Image: `postgres:16-alpine`
  - Environment variables sourced from:
    - ConfigMap `posttrade-dev-config`: `POSTGRES_DB`, `POSTGRES_USER`
    - Secret `posttrade-dev-db-secret`: `POSTGRES_PASSWORD`
  - Volume mount: attach `postgres-dev-pvc` to `/var/lib/postgresql/data`
  - Resource limits (free-tier friendly):
    - CPU: `250m` request / `500m` limit
    - Memory: `256Mi` request / `512Mi` limit
  - Liveness probe: `pg_isready` command
  - Readiness probe: `pg_isready` command

- [ ] **2.6 — PostgreSQL Service manifest** (`05-postgres-service.yaml`)
  - Name: `postgres-service`
  - Namespace: `<DEV_NAMESPACE>`
  - Type: `ClusterIP` (internal only — not exposed outside cluster)
  - Port: `5432`
  - Selector: matches PostgreSQL deployment pods

- [ ] **2.7 — Web API Deployment manifest** (`06-api-deployment.yaml`)
  - Name: `posttrade-api`
  - Namespace: `<DEV_NAMESPACE>`
  - Replicas: `1`
  - Image: `<DOCKER_REGISTRY>/<DOCKER_IMAGE_NAME>:dev-latest`
    _(image tag is replaced by pipeline before apply)_
  - Environment variables sourced from:
    - ConfigMap `posttrade-dev-config`: `ASPNETCORE_ENVIRONMENT`, `Jwt__Issuer`, `Jwt__Audience`, `Jwt__ExpiryMinutes`
    - Secret `posttrade-dev-db-secret`: `ConnectionStrings__DefaultConnection`
    - Secret `posttrade-dev-jwt-secret`: `Jwt__Key`
  - Container port: `<DEV_API_PORT>`
  - Resource limits (free-tier friendly):
    - CPU: `250m` request / `500m` limit
    - Memory: `256Mi` request / `512Mi` limit
  - Liveness probe: HTTP GET `/health` on `<DEV_API_PORT>` (after delay 30s)
  - Readiness probe: HTTP GET `/health` on `<DEV_API_PORT>` (after delay 15s)
  - Image pull secret: `ocir-secret` (for OCIR authentication)

- [ ] **2.8 — Web API Service manifest** (`07-api-service.yaml`)
  - Name: `posttrade-api-service`
  - Namespace: `<DEV_NAMESPACE>`
  - Type: `LoadBalancer` (OKE will provision an OCI Load Balancer)
  - Port: `80` → TargetPort: `<DEV_API_PORT>`
  - Selector: matches API deployment pods
  - Annotation: `service.beta.kubernetes.io/oci-load-balancer-shape: flexible`
    (required for OCI free-tier LB)

- [ ] **2.9 — OCIR image pull secret**
  - A K8s Secret of type `kubernetes.io/dockerconfigjson` named `ocir-secret`
  - Must be present in `<DEV_NAMESPACE>` before first deploy
  - Created once manually or via pipeline step (not stored in manifest file for security)

- [ ] **2.10 — Add health check endpoint to API** _(code change)_
  - Confirm `/health` endpoint exists in `Program.cs` (`app.MapHealthChecks("/health")`)
  - Add `builder.Services.AddHealthChecks()` if not present
  - This is required for liveness/readiness probes

---

## Phase 3 — GitHub Actions Workflow: QA Deployment

**Status:** ⏳ Pending

**File to create:** `.github/workflows/deploy-qa.yml`

**Trigger:** Push to `main` branch (i.e., after a PR is merged)

### Tasks

- [ ] **3.1 — Define workflow trigger**
  - Trigger on `push` to `main` branch only
  - Optionally require the Dev workflow to have passed first (using `workflow_run`)

- [ ] **3.2 — Add QA-specific GitHub Secrets** (in addition to Phase 1 secrets)
  - `QA_DB_PASSWORD` — PostgreSQL password for QA (`<QA_DB_PASSWORD>`)
  - All OCI/OCIR/OKE secrets from Phase 1 apply here too (same cluster, different namespace)

- [ ] **3.3 — Checkout step**
  - Use `actions/checkout@v4`

- [ ] **3.4 — Run unit tests** _(same as Phase 1 step 1.4)_
  - Must pass before any build or deploy step

- [ ] **3.5 — Build and push Docker image for QA**
  - Log in to OCIR (same registry as Dev)
  - Build image from `<DOCKERFILE_PATH>`
  - Tag image as:
    - `<DOCKER_REGISTRY>/<DOCKER_IMAGE_NAME>:qa-<GITHUB_SHA_SHORT>`
    - `<DOCKER_REGISTRY>/<DOCKER_IMAGE_NAME>:qa-latest`
  - Push both tags to OCIR

- [ ] **3.6 — Configure kubectl for OKE** _(same as Phase 1 step 1.6)_

- [ ] **3.7 — Ensure QA namespace exists**
  - Apply or create namespace `<QA_NAMESPACE>`

- [ ] **3.8 — Create / update Kubernetes Secrets for QA**
  - Create or update Secret `posttrade-qa-db-secret` in `<QA_NAMESPACE>` with:
    - `POSTGRES_PASSWORD` = `<QA_DB_PASSWORD>`
    - `ConnectionStrings__DefaultConnection` = QA connection string
  - Create or update Secret `posttrade-qa-jwt-secret` in `<QA_NAMESPACE>` with:
    - `Jwt__Key` = `<JWT_SECRET_KEY>`

- [ ] **3.9 — Update image tag in QA deployment manifest**
  - Replace image tag placeholder with `<DOCKER_REGISTRY>/<DOCKER_IMAGE_NAME>:qa-<GITHUB_SHA_SHORT>`

- [ ] **3.10 — Apply all QA Kubernetes manifests**
  - Apply all manifests under `<K8S_MANIFESTS_DIR>/qa/` in the same order as Phase 2
  - Use `kubectl apply -f <K8S_MANIFESTS_DIR>/qa/`

- [ ] **3.11 — Wait for rollout and verify**
  - Run `kubectl rollout status deployment/posttrade-api -n <QA_NAMESPACE>`
  - Run `kubectl rollout status deployment/postgres -n <QA_NAMESPACE>`

---

## Phase 4 — Kubernetes Manifests: QA Environment

**Status:** ⏳ Pending

**Folder to create:** `k8s/qa/`

**Files to create:**

```
k8s/qa/
  00-namespace.yaml
  01-configmap.yaml
  02-postgres-secret.yaml          ← optional (can be created by pipeline instead)
  03-postgres-pvc.yaml
  04-postgres-deployment.yaml
  05-postgres-service.yaml
  06-api-deployment.yaml
  07-api-service.yaml
```

### Tasks

All tasks mirror Phase 2 exactly, but scoped to the QA environment.
Key differences from Dev are listed below.

- [ ] **4.1 — Namespace manifest** (`00-namespace.yaml`)
  - Namespace: `<QA_NAMESPACE>`
  - Label: `environment: qa`

- [ ] **4.2 — ConfigMap manifest** (`01-configmap.yaml`)
  - Name: `posttrade-qa-config`
  - Namespace: `<QA_NAMESPACE>`
  - Keys to include:
    - `ASPNETCORE_ENVIRONMENT` = `Staging` (or `QA`)
    - `Jwt__Issuer` = `<JWT_ISSUER>`
    - `Jwt__Audience` = `<JWT_AUDIENCE>`
    - `Jwt__ExpiryMinutes` = `60`
    - `POSTGRES_DB` = `<QA_DB_NAME>`
    - `POSTGRES_USER` = `<QA_DB_USER>`

- [ ] **4.3 — PostgreSQL Secret manifest** (`02-postgres-secret.yaml`)
  - Name: `posttrade-qa-db-secret`
  - Namespace: `<QA_NAMESPACE>`
  - Keys: `POSTGRES_PASSWORD`, `ConnectionStrings__DefaultConnection`
  - Connection string uses `<QA_DB_NAME>` / `<QA_DB_USER>` / `<QA_DB_PASSWORD>`

- [ ] **4.4 — PostgreSQL PVC manifest** (`03-postgres-pvc.yaml`)
  - Name: `postgres-qa-pvc`
  - Namespace: `<QA_NAMESPACE>`
  - StorageClass: `<PVC_STORAGE_CLASS>`
  - AccessMode: `ReadWriteOnce`
  - Storage: `<PVC_SIZE_QA>`

- [ ] **4.5 — PostgreSQL Deployment manifest** (`04-postgres-deployment.yaml`)
  - Identical to Dev (Phase 2.5) but:
    - Namespace: `<QA_NAMESPACE>`
    - References ConfigMap `posttrade-qa-config` and Secret `posttrade-qa-db-secret`
    - PVC: `postgres-qa-pvc`

- [ ] **4.6 — PostgreSQL Service manifest** (`05-postgres-service.yaml`)
  - Name: `postgres-service`
  - Namespace: `<QA_NAMESPACE>`
  - Type: `ClusterIP` (DB never exposed externally)

- [ ] **4.7 — Web API Deployment manifest** (`06-api-deployment.yaml`)
  - Namespace: `<QA_NAMESPACE>`
  - Image: `<DOCKER_REGISTRY>/<DOCKER_IMAGE_NAME>:qa-latest`
    _(tag replaced by pipeline before apply)_
  - References ConfigMap `posttrade-qa-config` and Secrets `posttrade-qa-db-secret`, `posttrade-qa-jwt-secret`
  - Same resource limits as Dev unless QA needs higher limits

- [ ] **4.8 — Web API Service manifest** (`07-api-service.yaml`)
  - Name: `posttrade-api-service`
  - Namespace: `<QA_NAMESPACE>`
  - Type: `LoadBalancer`
  - Same OCI LB annotation as Dev

- [ ] **4.9 — OCIR image pull secret**
  - `ocir-secret` must also exist in `<QA_NAMESPACE>`
  - Created once manually or via pipeline step

---

## Phase 5 — Oracle Cloud OKE Setup

**Status:** ⏳ Pending

> This phase is a one-time manual setup on Oracle Cloud Infrastructure.
> No automation — follow steps in order.

### Tasks

- [ ] **5.1 — Create Oracle Cloud Free Tier Account**
  - Sign up at cloud.oracle.com if not already done
  - Confirm free tier is active (Always Free resources available)

- [ ] **5.2 — Generate OCI API Key**
  - Go to: OCI Console → Profile → API Keys → Add API Key
  - Download private key (`.pem` file)
  - Note: User OCID, Tenancy OCID, Fingerprint, Region
  - These values fill `OCI_CLI_USER`, `OCI_CLI_TENANCY`, `OCI_CLI_FINGERPRINT`, `OCI_CLI_REGION` GitHub Secrets

- [ ] **5.3 — Create OKE Cluster (Free Tier)**
  - Go to: OCI Console → Developer Services → Kubernetes Clusters (OKE) → Create Cluster
  - Select: **Quick Create** (simplest option for free tier)
  - Choose:
    - Kubernetes version: `v1.29.x` (latest stable)
    - Node shape: `VM.Standard.A1.Flex` (ARM, Always Free eligible)
    - Node count: `2` (fits within free tier limits: 4 OCPU / 24 GB RAM total)
    - Node memory: `12 GB` per node
    - Node OCPUs: `2` per node
    - Boot volume: `50 GB` per node
    - API endpoint: Public (required for GitHub Actions access)
    - Node subnet: Private (recommended)
  - Note the Cluster OCID → fill `<OKE_CLUSTER_ID>`

- [ ] **5.4 — Create OCIR Repository**
  - Go to: OCI Console → Developer Services → Container Registry
  - Create repository named `posttrade-api`
  - Set visibility: **Private**
  - Note the registry URL: `<OCI_REGION>.ocir.io/<OCIR_NAMESPACE>/posttrade-api`
  - This fills `<DOCKER_REGISTRY>/<DOCKER_IMAGE_NAME>`

- [ ] **5.5 — Generate OCIR Auth Token**
  - Go to: OCI Console → Profile → Auth Tokens → Generate Token
  - Copy the token immediately (shown only once)
  - This fills `OCIR_PASSWORD` GitHub Secret
  - `OCIR_USERNAME` = `<OCIR_NAMESPACE>/<oci-username>` (e.g. `tenancyname/user@email.com`)

- [ ] **5.6 — Install and configure OCI CLI locally** _(for local testing)_
  - Install OCI CLI: `pip install oci-cli` or use the install script
  - Run `oci setup config` and enter OCID / key details
  - Confirm: `oci iam user get --user-id <OCI_CLI_USER>` returns user info

- [ ] **5.7 — Configure local kubectl for OKE**
  - Run: `oci ce cluster create-kubeconfig --cluster-id <OKE_CLUSTER_ID> --region <OCI_REGION>`
  - Confirm: `kubectl get nodes` returns the 2 worker nodes

- [ ] **5.8 — Create Kubernetes namespaces**
  - Apply or run: create `<DEV_NAMESPACE>` namespace
  - Apply or run: create `<QA_NAMESPACE>` namespace
  - Verify: `kubectl get namespaces` shows both

- [ ] **5.9 — Create OCIR image pull secrets in both namespaces**
  - For `<DEV_NAMESPACE>`: create Secret `ocir-secret` of type `kubernetes.io/dockerconfigjson`
    using OCIR credentials
  - For `<QA_NAMESPACE>`: same as above
  - Verify: `kubectl get secret ocir-secret -n <DEV_NAMESPACE>`

- [ ] **5.10 — Add Dockerfile to repository**
  - Confirm `<DOCKERFILE_PATH>` (`backend/Dockerfile`) exists with:
    - Multi-stage build: SDK image for build → Runtime image for final
    - Base image: `mcr.microsoft.com/dotnet/aspnet:8.0`
    - Build image: `mcr.microsoft.com/dotnet/sdk:8.0`
    - Published output copied to final stage
    - `EXPOSE <DEV_API_PORT>`
    - `ENTRYPOINT ["dotnet", "PostTrade.API.dll"]`

- [ ] **5.11 — Add health check to API** _(if not already done in Phase 2.10)_
  - Ensure `builder.Services.AddHealthChecks()` is in `Program.cs`
  - Ensure `app.MapHealthChecks("/health")` is in `Program.cs`
  - Test locally: `curl http://localhost:5000/health` returns `200 Healthy`

- [ ] **5.12 — Deploy Dev environment manually (first time)**
  - Apply Dev manifests in order (Phase 2 file list)
  - Verify pods are Running: `kubectl get pods -n <DEV_NAMESPACE>`
  - Verify DB is not externally accessible: `kubectl get svc -n <DEV_NAMESPACE>`
    (postgres-service must show `ClusterIP`, not `LoadBalancer`)
  - Get API external IP: `kubectl get svc posttrade-api-service -n <DEV_NAMESPACE>`
  - Test API: `curl http://<EXTERNAL_IP>/health`

- [ ] **5.13 — Deploy QA environment manually (first time)**
  - Apply QA manifests in order (Phase 4 file list)
  - Same verification steps as 5.12 but for `<QA_NAMESPACE>`

- [ ] **5.14 — Verify end-to-end pipeline**
  - Push a commit to a `feature/**` branch
  - Confirm Dev GitHub Actions workflow runs, tests pass, image is pushed, and Dev pods roll over
  - Merge a PR to `main`
  - Confirm QA GitHub Actions workflow runs, tests pass, image is pushed, and QA pods roll over

---

## Notes for Future Sessions

- Always fill in the **Quick Reference** table at the top before starting any phase
- Phase 5 must be completed before Phase 1 or Phase 3 can run end-to-end
- The DB Secret (`ConnectionStrings__DefaultConnection`) is the critical link between
  the API deployment and PostgreSQL — double-check the hostname matches the
  PostgreSQL Service name (e.g. `postgres-service`) and namespace
- PostgreSQL Services must always be `ClusterIP` — never expose DB with `LoadBalancer` or `NodePort`
- OCI free tier LB requires the annotation `service.beta.kubernetes.io/oci-load-balancer-shape: flexible`
  on the Web API Service, otherwise provisioning may fail
- Image tags in `06-api-deployment.yaml` are replaced at deploy time by the pipeline —
  do not hardcode a specific SHA in the manifest file; use a placeholder like `IMAGE_TAG`
- Run `dotnet test` gates both Dev and QA pipelines — a failing test blocks the deploy

---

*Last updated: 2026-02-19 | Phases 1–4: ✅ Done | Phase 5: ⏳ Pending (manual OCI setup)*
