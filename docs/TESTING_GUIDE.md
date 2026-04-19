# EduPortal — Feature Testing Guide

**Live URLs**
- Frontend: `https://education-portal-rpvr.vercel.app`
- Backend API: `https://eduportal-backend-ssg4.onrender.com`
- Health check: `https://eduportal-backend-ssg4.onrender.com/api/health`

> **Note:** Render free tier sleeps after 15 min of inactivity. First request may take 30–60 seconds to wake up.

---

## Roles

| Role | Access |
|------|--------|
| **Guest** | Homepage, public resource list, CMS pages |
| **User** | Dashboard, enroll in resources, take exams, download certificates |
| **SuperAdmin** | Everything + full admin panel |

---

## Part 1 — Guest (No Login Required)

### 1.1 Homepage
**URL:** `/`

What to check:
- [ ] Page loads with hero section, features, how-it-works, CTA
- [ ] "Browse Resources" button navigates to `/resources`
- [ ] "Create Free Account" button navigates to `/auth/register`

---

### 1.2 Browse Resources
**URL:** `/resources`

What to check:
- [ ] Resource list loads (empty if no resources added yet)
- [ ] Filter/search works once resources exist
- [ ] Clicking a resource opens its detail page at `/resources/[slug]`

---

### 1.3 Resource Detail Page
**URL:** `/resources/[slug]`

What to check:
- [ ] Title, description, price displayed
- [ ] "Enroll" or "Buy" button visible
- [ ] Clicking enroll redirects to login if not authenticated

---

### 1.4 CMS Static Pages
**URLs:** `/about-us`, `/contact`, `/privacy`, `/terms`, `/faq`, `/help`

What to check:
- [ ] Pages load (content will be empty until admin fills them in)

---

## Part 2 — User Registration & Login

### 2.1 Register
**URL:** `/auth/register`

Steps:
1. Fill in First Name, Last Name, Email, Password, Confirm Password
2. Click **Register**

Expected:
- [ ] Redirected to `/dashboard`
- [ ] Welcome state shown

---

### 2.2 Login
**URL:** `/auth/login`

Steps:
1. Enter registered email and password
2. Click **Sign In**

Expected:
- [ ] Redirected to `/dashboard`
- [ ] User name shown in navbar

---

### 2.3 Logout
Steps:
1. Click profile avatar in navbar
2. Click **Logout**

Expected:
- [ ] Redirected to homepage
- [ ] Login button visible again

---

## Part 3 — User Dashboard

**URL:** `/dashboard`
*(Must be logged in)*

### 3.1 Dashboard Overview
What to check:
- [ ] Stats cards visible (enrolled resources, completed, certificates)
- [ ] Recent activity section

---

### 3.2 My Content
**URL:** `/dashboard/my-content`

What to check:
- [ ] Lists resources the user has enrolled in
- [ ] Empty state shown if no enrollments yet

---

### 3.3 My Certificates
**URL:** `/dashboard/certificates`

What to check:
- [ ] Lists earned certificates
- [ ] Download button works (after passing an exam)

---

## Part 4 — Resources & Enrollment

### 4.1 Enroll in a Free Resource
*(Requires admin to create a free resource first — see Part 6)*

Steps:
1. Go to `/resources`
2. Click a free resource
3. Click **Enroll**

Expected:
- [ ] Enrolled successfully (toast message)
- [ ] Resource appears in `/dashboard/my-content`

---

### 4.2 View Resource Content
**URL:** `/resources/[slug]/view`

Steps:
1. Go to `/dashboard/my-content`
2. Click an enrolled resource → **View Content**

Expected:
- [ ] Video player, PDF viewer, or article content loads

---

### 4.3 Purchase a Paid Resource (Razorpay)
*(Requires admin to create a paid resource first)*

Steps:
1. Click a paid resource → **Buy Now**
2. Redirected to `/checkout/[resourceId]`
3. Click **Pay with Razorpay**
4. Use test card: `4111 1111 1111 1111`, any future expiry, any CVV
5. Complete payment

Expected:
- [ ] Redirected to `/checkout/success`
- [ ] Resource accessible in dashboard

---

## Part 5 — Exams & Certificates

### 5.1 Take an Exam
**URL:** `/exams/[slug]/attempt`
*(Requires admin to create and publish an exam first — see Part 6.5)*

Steps:
1. Go to a resource detail page that has an exam linked
2. Click **Take Exam**
3. Answer all questions
4. Click **Submit**

Expected:
- [ ] Score displayed on results page `/exams/results/[attemptId]`
- [ ] Pass/fail status shown

---

### 5.2 Download Certificate
*(Only available after passing an exam)*

Steps:
1. Go to `/dashboard/certificates`
2. Click **Download** on a certificate

Expected:
- [ ] PDF certificate downloaded with your name, exam title, score, date

---

## Part 6 — Admin Panel

**URL:** `/admin`

First, promote your account to SuperAdmin via Supabase SQL:
```sql
-- Step 1: Get your user ID
SELECT id, email FROM "Users";

-- Step 2: Assign SuperAdmin role (RoleId = 1)
INSERT INTO "UserRoles" ("UserId", "RoleId") VALUES ('YOUR_USER_ID', 1);
```

Then log out and log back in.

---

### 6.1 Admin Dashboard
**URL:** `/admin`

What to check:
- [ ] Analytics overview loads (total users, resources, revenue, exam attempts)

---

### 6.2 Analytics
**URL:** `/admin/analytics`

What to check:
- [ ] Charts load (user growth, revenue, popular resources)
- [ ] Date range filter works

---

### 6.3 Manage Categories
**URL:** `/admin/categories`

Steps to test:
1. Click **Add Category**
2. Enter name (e.g. "Programming") → Save
3. Edit the category → rename it → Save
4. Delete the category

Expected:
- [ ] Category appears in resource creation form
- [ ] CRUD operations work

---

### 6.4 Manage Resources
**URL:** `/admin/resources`

**Create a free resource:**
1. Click **New Resource**
2. Fill in:
   - Title: `Introduction to Python`
   - Slug: `intro-python`
   - Type: `Video` / `PDF` / `Blog`
   - Category: select one
   - Price: `0` (free)
   - Description: any text
3. Upload content file (for Video/PDF) or write article (for Blog)
4. Click **Save**
5. Click **Publish**

Expected:
- [ ] Resource appears on `/resources` public page
- [ ] Users can enroll

**Create a paid resource:**
- Same steps but set Price > 0 (e.g. `₹499`)

**Edit resource:**
- `/admin/resources/[id]/edit` — modify any field → Save

**Unpublish:**
- Click **Unpublish** → resource hidden from public

---

### 6.5 Manage Exams
**URL:** `/admin/exams`

**Create an exam:**
1. Click **New Exam**
2. Fill in:
   - Title: `Python Basics Exam`
   - Duration: `30` (minutes)
   - Passing Score: `70` (%)
   - Link to resource (optional)
3. Save

**Add questions:**
1. Go to `/admin/questions`
2. Click **Add Question**
3. Fill in:
   - Question text
   - 4 answer options
   - Mark the correct answer
   - Link to exam
4. Save

**Publish exam:**
- Back in `/admin/exams` → click **Publish**

Expected:
- [ ] Exam appears on the linked resource page
- [ ] Users can attempt it

---

### 6.6 View Exam Attempts
**URL:** `/admin/exam-attempts`

What to check:
- [ ] All user exam attempts listed
- [ ] Score, pass/fail, date visible
- [ ] Filter by exam or user works

---

### 6.7 Manage Users
**URL:** `/admin/users`

What to check:
- [ ] All registered users listed with email, name, role, status
- [ ] **Deactivate** a user — they can no longer log in
- [ ] **Activate** a user — restores access

---

### 6.8 CMS — Banners
**URL:** `/admin/cms/banners`

Steps:
1. Edit the **Hero** banner
2. Change title and subtitle
3. Save → click **Revalidate** (refreshes cached homepage)

Expected:
- [ ] Homepage hero text updated

---

### 6.9 CMS — Pages
**URL:** `/admin/cms/pages`

Steps:
1. Click **About Us**
2. Add some content using the rich text editor
3. Publish

Expected:
- [ ] `/about-us` page shows the content

---

### 6.10 CMS — FAQs
**URL:** `/admin/cms/faqs`

Steps:
1. Click **Add FAQ**
2. Enter question and answer
3. Save

Expected:
- [ ] FAQ appears on `/faq` public page

---

### 6.11 CMS — Feature Flags
**URL:** `/admin/cms/feature-flags`

Available flags:

| Flag | Effect |
|------|--------|
| `enable_payments` | Enables/disables payment gateway |
| `enable_exams` | Enables/disables exam module |
| `enable_certificates` | Enables/disables certificate generation |
| `enable_google_auth` | Shows/hides Google login button |
| `maintenance_mode` | Shows maintenance page to all users |

Steps:
1. Toggle **maintenance_mode** ON → visit homepage → maintenance page shown
2. Toggle it back OFF

---

### 6.12 CMS — Footer
**URL:** `/admin/cms/footer`

Steps:
1. Edit company text and copyright
2. Save

Expected:
- [ ] Footer updated on all public pages

---

### 6.13 CMS — Sections
**URL:** `/admin/cms/sections`

Controls which homepage sections are visible:
- Hero Banner
- Featured Resources
- Testimonials
- Promo Banner
- Stats Bar

Steps:
1. Toggle **Promo Banner** ON
2. Go to Banners → edit the Promo banner message
3. Visit homepage → promo bar visible at top

---

### 6.14 CMS — Settings
**URL:** `/admin/cms/settings`

Editable settings:
- `site_name` — displayed as platform name
- `support_email` — shown in footer/contact
- `currency` — used in pricing display

---

## Part 7 — Google OAuth
*(Requires Google OAuth credentials configured)*

**URL:** `/auth/login` → **Continue with Google**

Steps:
1. Click **Continue with Google**
2. Select your Google account
3. Authorize

Expected:
- [ ] Redirected back to `/auth/google-callback`
- [ ] Logged in and redirected to `/dashboard`

---

## Quick Smoke Test Checklist

Run these after every deployment to confirm nothing is broken:

| Test | URL | Expected |
|------|-----|----------|
| Health check | `/api/health` | `{"status":"healthy"}` |
| Homepage loads | `/` | Hero section visible |
| Register works | `/auth/register` | Redirects to dashboard |
| Login works | `/auth/login` | Redirects to dashboard |
| Resources page | `/resources` | Loads without error |
| Admin accessible | `/admin` | Admin dashboard (SuperAdmin only) |
| Dashboard loads | `/dashboard` | Stats and content visible |

---

## Common Issues

| Issue | Likely Cause | Fix |
|-------|-------------|-----|
| Stuck on login after submit | Cookie/session issue | Clear cookies, try again |
| Resources page empty | No resources published | Create and publish via admin |
| Exam not showing | Exam not published | Go to `/admin/exams` → Publish |
| Certificate not generated | Exam passing score not met | Check score threshold in admin |
| Payment fails | Razorpay test mode | Use test card `4111 1111 1111 1111` |
| Admin panel shows 403 | Role not assigned | Run Supabase SQL to assign SuperAdmin |
| Backend slow first load | Render free tier sleeping | Wait 30–60 sec, retry |
