# EduPortal — GCP Deployment Guide (Free Tier)

## Cost: ₹0/month permanently

All services used are on permanent free tiers — no credit card charges after trial.

| Service | What it does | Free tier |
|---------|-------------|-----------|
| **Cloud Run** (GCP) | Hosts backend + frontend | 2M requests/month free |
| **Artifact Registry** (GCP) | Stores Docker images | 0.5 GB/month free |
| **Secret Manager** (GCP) | Runtime secrets | 6 versions free |
| **Supabase** | PostgreSQL database | 500 MB, unlimited projects |
| **Upstash** | Redis cache | 10,000 commands/day |
| **Cloudflare R2** | File storage | 10 GB free |

**Not used** (would cost money): Cloud SQL, Memorystore, App Engine, GKE.

---

## Architecture

```
GitHub Push → GitHub Actions → Artifact Registry → Cloud Run
                                                    ├── eduportal-backend  (.NET 8)
                                                    └── eduportal-frontend (Next.js 14)

Supabase PostgreSQL  ← backend connects via standard connection string
Upstash Redis        ← backend cache (optional, graceful fallthrough)
Secret Manager       ← all backend runtime secrets (mounted as env vars)
```

Push to `main` = auto-deploy. No VMs, no SSH, no docker-compose.

---

## One-Time Setup (~20 min)

### Before you start — create free accounts

1. **Supabase** — [supabase.com](https://supabase.com) → New project
   - After creating: Settings → Database → **Connection string** (URI mode)
   - Copy the string, replace `[YOUR-PASSWORD]` with your DB password
   - Format: `Host=db.XXXX.supabase.co;Database=postgres;Username=postgres;Password=PASS;SSL Mode=Require`

2. **Upstash Redis** — [upstash.com](https://upstash.com) → Create database → Free tier
   - After creating: copy the **Redis URL** (starts with `rediss://`)

3. **Google Cloud project** — [console.cloud.google.com](https://console.cloud.google.com)
   - Create a new project (or use existing)
   - Billing must be linked (required to activate APIs, but free tier means no charges)

4. **gcloud CLI** — [install guide](https://cloud.google.com/sdk/docs/install)
   ```bash
   gcloud auth login
   gcloud config set project YOUR_PROJECT_ID
   ```

---

### Step 1 — Run the setup script

From the backend repo root:

```bash
bash infra/gcp-setup.sh
```

What it does:
- Enables Cloud Run, Artifact Registry, Secret Manager APIs
- Creates the `eduportal` Artifact Registry repository
- Creates two service accounts (`github-actions-deploy`, `eduportal-runtime`)
- Creates all secrets in Secret Manager (you're prompted for each value)
- Downloads a deploy key to `infra/github-actions-key.json`

**Takes ~5 minutes.** No Cloud SQL — database is Supabase.

---

### Step 2 — Add GitHub Secrets & Variables

The script prints the exact values at the end. Go to each repo:
**Settings → Secrets and variables → Actions**

#### Both repos — Secret:

| Name | Value |
|------|-------|
| `GCP_SA_KEY` | Full contents of `infra/github-actions-key.json` |

```bash
# Windows (PowerShell): copy to clipboard
Get-Content infra\github-actions-key.json | Set-Clipboard

# Mac/Linux
cat infra/github-actions-key.json | pbcopy
```

#### Both repos — Variables (not secrets):

| Name | Value |
|------|-------|
| `GCP_REGION` | `us-central1` |
| `AR_BASE` | `us-central1-docker.pkg.dev/YOUR_PROJECT_ID/eduportal` |
| `RUNTIME_SA` | `eduportal-runtime@YOUR_PROJECT_ID.iam.gserviceaccount.com` |

#### Frontend repo only — Variables:

| Name | Value |
|------|-------|
| `APP_NAME` | `EduPortal` |
| `API_URL` | *(leave empty — set after backend first deploy)* |
| `APP_URL` | *(leave empty — set after frontend first deploy)* |

#### Frontend repo only — Secrets:

| Name | Value |
|------|-------|
| `GOOGLE_CLIENT_ID` | Google OAuth Client ID |
| `STRIPE_PUBLISHABLE_KEY` | Stripe publishable key |
| `RAZORPAY_KEY_ID` | Razorpay key ID |
| `REVALIDATION_SECRET` | Same value entered during setup script |

---

### Step 3 — First Deploy

Push to `main` in the **backend repo**:

```bash
git commit --allow-empty -m "chore: trigger first Cloud Run deploy"
git push origin main
```

Watch the Actions tab. Deploy takes ~3–4 min.
After it succeeds, click the deployment summary to get the backend URL.

---

### Step 4 — Wire Up URLs

```bash
# Get backend URL
gcloud run services describe eduportal-backend \
  --region=us-central1 --format='value(status.url)'
```

1. Set `API_URL` variable in the **frontend** repo → paste the backend URL
2. Push to `main` in the **frontend repo** to trigger its first deploy
3. Get the frontend URL (from Actions summary or `gcloud run services describe`)
4. Set `APP_URL` variable in the frontend repo → paste the frontend URL
5. Update two secrets in Secret Manager with the real frontend URL:
   ```bash
   echo -n "https://FRONTEND_CLOUD_RUN_URL" | \
     gcloud secrets versions add frontend-url --data-file=-

   echo -n "https://FRONTEND_CLOUD_RUN_URL/api/revalidate" | \
     gcloud secrets versions add nextjs-revalidate-url --data-file=-
   ```
6. Re-deploy backend once more so it picks up the CORS origin:
   ```bash
   git commit --allow-empty -m "chore: update cors origin"
   git push origin main
   ```

---

## Day-to-Day

```
Push to main  →  GitHub Actions  →  ~3 min  →  live
```

That's it. No SSH, no docker-compose, no server management.

---

## Custom Domain (Free on Cloud Run)

Cloud Run provides a `*.run.app` URL by default. To use your own domain:

```bash
# Backend
gcloud run domain-mappings create \
  --service=eduportal-backend \
  --domain=api.yourdomain.com \
  --region=us-central1

# Frontend
gcloud run domain-mappings create \
  --service=eduportal-frontend \
  --domain=yourdomain.com \
  --region=us-central1
```

GCP provisions a free TLS certificate. Add the DNS records shown to your provider.

---

## Update a Secret

```bash
echo -n "NEW_VALUE" | gcloud secrets versions add SECRET_NAME --data-file=-
# Then redeploy the service to pick it up
gcloud run services update eduportal-backend --region=us-central1 \
  --update-secrets=SECRET_NAME=SECRET_NAME:latest
```

---

## Rollback

```bash
# List revisions
gcloud run revisions list --service=eduportal-backend --region=us-central1

# Send all traffic to a previous revision
gcloud run services update-traffic eduportal-backend \
  --to-revisions=REVISION_NAME=100 --region=us-central1
```

---

## View Logs

```bash
gcloud run services logs read eduportal-backend --region=us-central1 --tail=50
gcloud run services logs read eduportal-frontend --region=us-central1 --tail=50
```

---

## Free Tier Limits (for reference)

If the project ever scales up:

| Service | Free limit | Overage cost |
|---------|-----------|-------------|
| Cloud Run requests | 2M/month | ~₹0.004/1K |
| Cloud Run compute | 360K GB-sec | ~₹0.006/GB-sec |
| Supabase DB | 500 MB | Upgrade plan |
| Upstash Redis | 10K commands/day | ~$0.2/100K commands |

For a personal/portfolio project, you will not hit these limits.
