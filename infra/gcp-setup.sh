#!/usr/bin/env bash
# EduPortal — One-time GCP infrastructure provisioning (free tier)
# Run: bash infra/gcp-setup.sh
# Prerequisites: gcloud CLI installed + authenticated (gcloud auth login)
#
# Database: Supabase free tier (NOT Cloud SQL — Cloud SQL has no free tier)
# Redis:    Upstash free tier (NOT Memorystore)
# Hosting:  Cloud Run (2M requests/month free permanently)
set -euo pipefail

# ── Helpers ──────────────────────────────────────────────────────────────────
RED='\033[0;31m'; GREEN='\033[0;32m'; YELLOW='\033[1;33m'; BLUE='\033[0;34m'; NC='\033[0m'
log()    { echo -e "${GREEN}✓${NC} $*"; }
warn()   { echo -e "${YELLOW}⚠${NC}  $*"; }
err()    { echo -e "${RED}✗${NC}  $*"; exit 1; }
header() { echo -e "\n${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"; echo -e "  $*"; echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"; }
ask_secret() { read -rsp "  $1: " "$2"; echo; }

command -v gcloud &>/dev/null || err "gcloud CLI not found — install from https://cloud.google.com/sdk"

header "EduPortal — GCP Free Tier Setup"
echo ""
echo "  Services used (all free):"
echo "    Cloud Run        — backend + frontend hosting"
echo "    Artifact Registry — container images (0.5 GB free)"
echo "    Secret Manager   — runtime secrets (free tier)"
echo "    Supabase         — PostgreSQL database (free, set up separately)"
echo "    Upstash          — Redis cache (free, set up separately)"
echo ""

# ── Project & Region ─────────────────────────────────────────────────────────
CURRENT_PROJECT=$(gcloud config get-value project 2>/dev/null || true)
if [[ -n "$CURRENT_PROJECT" ]]; then
  read -rp "  GCP Project ID [$CURRENT_PROJECT]: " input
  PROJECT_ID="${input:-$CURRENT_PROJECT}"
else
  read -rp "  GCP Project ID: " PROJECT_ID
fi

REGION="us-central1"
read -rp "  Region [$REGION]: " input; REGION="${input:-$REGION}"

AR_REPO="eduportal"
DEPLOY_SA="github-actions-deploy"
DEPLOY_SA_EMAIL="$DEPLOY_SA@$PROJECT_ID.iam.gserviceaccount.com"
RUNTIME_SA="eduportal-runtime"
RUNTIME_SA_EMAIL="$RUNTIME_SA@$PROJECT_ID.iam.gserviceaccount.com"

gcloud config set project "$PROJECT_ID" --quiet
echo ""
log "Project: $PROJECT_ID  |  Region: $REGION"

# ── Enable APIs ──────────────────────────────────────────────────────────────
header "1 / 5 — Enabling GCP APIs"
gcloud services enable \
  run.googleapis.com \
  artifactregistry.googleapis.com \
  secretmanager.googleapis.com \
  cloudresourcemanager.googleapis.com \
  --project="$PROJECT_ID" --quiet
log "APIs enabled (run, artifactregistry, secretmanager)"

# ── Artifact Registry ────────────────────────────────────────────────────────
header "2 / 5 — Artifact Registry"
if gcloud artifacts repositories describe "$AR_REPO" --location="$REGION" --project="$PROJECT_ID" &>/dev/null 2>&1; then
  warn "Repository '$AR_REPO' already exists — skipping"
else
  gcloud artifacts repositories create "$AR_REPO" \
    --repository-format=docker \
    --location="$REGION" \
    --project="$PROJECT_ID" \
    --quiet
fi
AR_HOST="$REGION-docker.pkg.dev"
AR_BASE="$AR_HOST/$PROJECT_ID/$AR_REPO"
log "Registry: $AR_BASE"

# ── Service Accounts ─────────────────────────────────────────────────────────
header "3 / 5 — Service Accounts"

create_sa() {
  local name="$1" display="$2" email="$3"
  if gcloud iam service-accounts describe "$email" --project="$PROJECT_ID" &>/dev/null 2>&1; then
    warn "SA '$name' already exists"
  else
    gcloud iam service-accounts create "$name" \
      --display-name="$display" --project="$PROJECT_ID" --quiet
    log "Created: $email"
  fi
}

grant_role() {
  gcloud projects add-iam-policy-binding "$PROJECT_ID" \
    --member="serviceAccount:$1" --role="$2" --quiet 2>/dev/null || true
  log "Granted $2 → $1"
}

# Deploy SA — used by GitHub Actions to push images and deploy services
create_sa "$DEPLOY_SA" "GitHub Actions Deploy" "$DEPLOY_SA_EMAIL"
grant_role "$DEPLOY_SA_EMAIL" "roles/run.admin"
grant_role "$DEPLOY_SA_EMAIL" "roles/artifactregistry.writer"
grant_role "$DEPLOY_SA_EMAIL" "roles/iam.serviceAccountUser"

# Runtime SA — Cloud Run services run as this identity
create_sa "$RUNTIME_SA" "EduPortal Runtime" "$RUNTIME_SA_EMAIL"
grant_role "$RUNTIME_SA_EMAIL" "roles/secretmanager.secretAccessor"

# Download deploy SA key for GitHub Actions
KEY_FILE="infra/github-actions-key.json"
gcloud iam service-accounts keys create "$KEY_FILE" \
  --iam-account="$DEPLOY_SA_EMAIL" --project="$PROJECT_ID" --quiet
grep -qxF "/$KEY_FILE" .gitignore 2>/dev/null || echo "/$KEY_FILE" >> .gitignore
log "Deploy key → $KEY_FILE (added to .gitignore)"

# ── Secret Manager ───────────────────────────────────────────────────────────
header "4 / 5 — Secret Manager"
echo ""
echo "  You'll be prompted for each backend secret."
echo "  For Supabase DB: go to supabase.com → Project → Settings → Database → Connection string (URI mode)"
echo "  For Upstash Redis: go to upstash.com → Redis → your database → .NET connection string"
echo ""

upsert_secret() {
  local name="$1" value="$2"
  if gcloud secrets describe "$name" --project="$PROJECT_ID" &>/dev/null 2>&1; then
    printf '%s' "$value" | gcloud secrets versions add "$name" --data-file=- --project="$PROJECT_ID" --quiet
    warn "Updated: $name"
  else
    printf '%s' "$value" | gcloud secrets create "$name" \
      --data-file=- --replication-policy=automatic --project="$PROJECT_ID" --quiet
    log "Created: $name"
  fi
}

prompt_upsert() {
  local name="$1" prompt="$2"
  read -rsp "  $prompt: " value; echo
  [[ -n "$value" ]] && upsert_secret "$name" "$value" || warn "Skipped: $name (add later)"
}

echo "  ── Database (Supabase) ──"
prompt_upsert "db-connection-string" "Supabase connection string (Host=db.XXX.supabase.co;...)"

echo ""
echo "  ── Auth ──"
prompt_upsert "jwt-secret"   "JWT Secret (run: openssl rand -base64 64)"
prompt_upsert "jwt-issuer"   "JWT Issuer (e.g. https://api.yourdomain.com or Cloud Run URL)"
prompt_upsert "jwt-audience" "JWT Audience (e.g. https://yourdomain.com or Cloud Run URL)"

echo ""
echo "  ── Cache (Upstash) ──"
prompt_upsert "redis-connection-string" "Upstash Redis URL (rediss://default:TOKEN@HOST:PORT)"

echo ""
echo "  ── Storage (Cloudflare R2) ──"
prompt_upsert "r2-account-id"       "R2 Account ID"
prompt_upsert "r2-bucket-name"      "R2 Bucket Name"
prompt_upsert "r2-access-key-id"    "R2 Access Key ID"
prompt_upsert "r2-secret-access-key" "R2 Secret Access Key"

echo ""
echo "  ── Email (Resend) ──"
prompt_upsert "resend-api-key"    "Resend API Key"
prompt_upsert "email-from-address" "From address (e.g. noreply@yourdomain.com)"

echo ""
echo "  ── Google OAuth ──"
prompt_upsert "google-client-id"     "Google Client ID"
prompt_upsert "google-client-secret" "Google Client Secret"

echo ""
echo "  ── Payments ──"
prompt_upsert "stripe-secret-key"      "Stripe Secret Key"
prompt_upsert "stripe-webhook-secret"  "Stripe Webhook Secret"
prompt_upsert "razorpay-key-id"        "Razorpay Key ID"
prompt_upsert "razorpay-key-secret"    "Razorpay Key Secret"

echo ""
echo "  ── URLs (set after first deploy — press Enter to skip for now) ──"
prompt_upsert "frontend-url"          "Frontend URL (e.g. https://eduportal-frontend-xxx.run.app)"
prompt_upsert "nextjs-revalidate-url" "Revalidate URL (FRONTEND_URL/api/revalidate)"
prompt_upsert "revalidation-secret"   "Revalidation Secret (run: openssl rand -hex 32)"

# ── Summary ──────────────────────────────────────────────────────────────────
header "5 / 5 — Setup Complete"
echo ""
echo "  ┌─────────────────────────────────────────────────────────────────┐"
echo "  │  Add these to BOTH GitHub repos:                                │"
echo "  │  Settings → Secrets and variables → Actions                     │"
echo "  └─────────────────────────────────────────────────────────────────┘"
echo ""
echo "  Secrets:"
echo -e "    ${GREEN}GCP_SA_KEY${NC}  →  contents of ./$KEY_FILE"
echo "              Windows: type infra\\github-actions-key.json"
echo "              Mac/Linux: cat infra/github-actions-key.json | pbcopy"
echo ""
echo "  Repository Variables (not secrets):"
echo -e "    ${GREEN}GCP_PROJECT_ID${NC}  →  $PROJECT_ID"
echo -e "    ${GREEN}GCP_REGION${NC}      →  $REGION"
echo -e "    ${GREEN}AR_BASE${NC}         →  $AR_BASE"
echo -e "    ${GREEN}RUNTIME_SA${NC}      →  $RUNTIME_SA_EMAIL"
echo ""
echo "  Frontend repo also needs these variables:"
echo -e "    ${GREEN}APP_NAME${NC}  →  EduPortal"
echo -e "    ${GREEN}API_URL${NC}   →  (set after backend first deploy)"
echo -e "    ${GREEN}APP_URL${NC}   →  (set after frontend first deploy)"
echo ""
echo "  Frontend repo also needs these secrets:"
echo -e "    ${GREEN}GOOGLE_CLIENT_ID${NC}          →  Google OAuth Client ID"
echo -e "    ${GREEN}STRIPE_PUBLISHABLE_KEY${NC}    →  Stripe Publishable Key"
echo -e "    ${GREEN}RAZORPAY_KEY_ID${NC}           →  Razorpay Key ID"
echo -e "    ${GREEN}REVALIDATION_SECRET${NC}       →  same value as above"
echo ""
echo -e "  ${GREEN}Next: push to main to trigger your first deployment.${NC}"
echo ""
