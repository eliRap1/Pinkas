# B-Managed

All-in-One management system for freelancers and small businesses.

WCF service backend, MS Access (.accdb) database, three clients (Owner / Employee / Client roles), two front-ends (WPF desktop + ASP.NET Razor Pages web).

---

## Quick start

### 0. Clone

```cmd
git clone https://github.com/eliRap1/yudb.git
cd yudb
```

### 1. Bootstrap the database

```cmd
powershell -ExecutionPolicy Bypass -File _init_db.ps1
```

Creates `WcfServiceLibrary1/ViewDB/Database/BManaged.accdb` with 10 tables and seeds:

| User | Password | Role |
|------|----------|------|
| `admin` | `admin1234` | Owner |
| `dana` | `admin1234` | Employee |
| `acme` | `admin1234` | Client |

…plus 2 customers, 2 projects, 1 invoice (with line items), 2 expenses, 8 expense categories, ILS↔USD exchange rates.

### 2. Restore the PdfSharp NuGet package

```cmd
nuget restore WcfServiceLibrary1\BusinessLogic\packages.config -PackagesDirectory packages
```

(or open `BManaged.sln` in Visual Studio — auto-restores on build).

### 3. Run the WCF host

Open `WcfServiceLibrary1\BManaged.sln` in Visual Studio.

**Set startup project = `WcfServiceLibrary1`** → press **F5**. WcfSvcHost + WcfTestClient open. Service is at:

```
http://localhost:8744/Design_Time_Addresses/WcfServiceLibrary1/Service1/
```

If "Class Library cannot be started" — your VS install lacks WCF tooling. Use the included fallback:

- Set startup project = `ConsoleHost` → F5 → console-hosted service runs at the same URL.

### 4. Run the WPF client

Open `BManagedClient\BManagedClient.csproj` in Visual Studio (separate window).

Build → F5. Sign in with `admin` / `admin1234` to land on the Owner dashboard.

**No "Add Service Reference" step needed** — `Connected Services\bsrv\Reference.cs` is hand-written and committed.

### 5. Run the web

```cmd
cd BManagedWeb\BManagedWeb
dotnet restore
dotnet run
```

→ http://localhost:5050. Same logins.

---

## Layout

```
yudb/
├── _init_db.ps1                ← bootstrap script for BManaged.accdb
├── _md_to_docx.py              ← rebuild project-book .docx from .md
├── _make_flows.py              ← regenerate flow PNGs
├── arch-flow.png  wpf-flow.png  web-flow.png
├── README.md
├── ספר_פרויקט_B_Managed_HE.md  ←  project book (Hebrew)
├── ספר_פרויקט_B_Managed_HE.docx
├── BManaged_ProjectBook_EN.md  ←  project book (English)
├── BManaged_ProjectBook_EN.docx
├── WcfServiceLibrary1/         server
│   ├── WcfServiceLibrary1/     IService1, Service1, App.config
│   ├── Model/                  DataContract entities + SecurityHelper
│   ├── ViewDB/                 BaseDB + 9 per-table DBs + BManaged.accdb
│   ├── BusinessLogic/          VAT, CurrencyConverter, InvoicePdfBuilder (PdfSharp)
│   ├── ConsoleHost/            standalone WCF host fallback
│   └── BManaged.sln            4 projects + ConsoleHost
├── BManagedClient/             WPF client (.NET 4.7.2)
├── BManagedWeb/                ASP.NET Razor Pages (.NET 8)
└── nav-map/                    Mermaid maps + page screenshots
```

---

## Roles & features

| Role | Capabilities |
|------|--------------|
| **Owner** | Customers · Projects · Invoices (with PDF) · Expenses · Reports (VAT/P&L/charts/CSV) · ManageUsers (approve, promote, reset, deactivate) · Settings |
| **Employee** | Assigned projects · Log own expenses · Notifications |
| **Client** | Read-only invoice portal · Self-signup |

### Pending approval workflow

New users that sign up via SignUp default to `IsActive = false`. Owner sees them in **ManageUsers → Pending approvals** and clicks **Approve**. `CheckUserPassword` rejects inactive accounts so unapproved users cannot log in.

### Multi-currency

Every monetary table has a `currency` column (ILS / USD). `CurrencyConverter` reads the latest matching row from `ExchangeRates` (effectiveDate ≤ asOfDate). Reports take a `displayCurrency` parameter.

### PDF invoices

`InvoicePdfBuilder.cs` uses **PdfSharp 1.50.5147** (the .NET-Framework-compatible build). Dotted-line totals box, A4 page, English font (Helvetica). NuGet package restores automatically; if not, run `nuget restore` from the repo root.

### Charts & CSV

Owner Reports page renders a Chart.js bar (income vs expenses vs VAT due) plus a doughnut (expenses by category) and exports a CSV via the **Export CSV** button.

### RTL / Hebrew

Click the **EN ↔ עב** toggle in the floating nav — sets `Session["Lang"]` and switches `<html dir="rtl" lang="he">`.

---

## Architecture rubric mapping

| Rubric requirement | Where it lives |
|--------------------|----------------|
| WCF service + multi-client | `WcfServiceLibrary1` + `BManagedClient` + `BManagedWeb` |
| Multiple normalized tables + link table | 10 tables — `Invoices` is link `Customer ↔ Project`, `InvoiceLines` is link `Invoice ↔ items` |
| Complex SQL (JOINs / aggregation) | `ViewDB/ReportsDB.cs` — 4-table INNER JOIN + GROUP BY + SUM in `EmployeeRevenueReport` |
| OOP + inheritance | All entities inherit `Base`; all DBs inherit `BaseDB`; `AllUsers : List<User>` |
| Multiple roles + role-aware UI | `Session["Role"]` (Web) / `ClientSession.IsOwner` (WPF) gates every page |
| Encryption | `Model/Helpers/SecurityHelper.cs` — PBKDF2 + 16-byte salt + 10000 iter + constant-time compare |
| Validation | `BManagedClient/ValidationRules.cs` (7 rules) + Razor `Regex` validation in `SignUp.cshtml.cs` |
| `IValueConverter` | `BManagedClient/ImgConventer.cs` |
| External service consumption | Both clients consume the WCF service over `BasicHttpBinding` |
| Async / threading | WPF `DispatcherTimer` polling + Web `setInterval` JSON polling |
| File transfer | Invoice PDF returned as `byte[]` over WCF |
| SQL-injection protection | `OleDbParameter` everywhere + `IsSafeString(input, max)` sanitizer |

Full rubric → `ספר_פרויקט_B_Managed_HE.md` chapter 2.

---

## Project book

- **Hebrew master**: `ספר_פרויקט_B_Managed_HE.md` + `.docx`
- **English appendix**: `BManaged_ProjectBook_EN.md` + `.docx`

To rebuild docx:

```cmd
python _md_to_docx.py
```

---

## Running tests / smoke test

WCF: `curl http://localhost:8744/Design_Time_Addresses/WcfServiceLibrary1/Service1/?wsdl`

DB: PowerShell ACE.OLEDB query against `BManaged.accdb`.

Web: `agent-browser open http://localhost:5050/Login` (see `nav-map/`).
