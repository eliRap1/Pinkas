# B-Managed — Project Book (English summary)

> Companion to the full Hebrew project book (`ספר_פרויקט_B_Managed_HE.md`). This file covers the same chapters, condensed, in English.

## 1. Introduction

**B-Managed** is an All-in-One management system for freelancers and small businesses, built on the same architecture as the sister project *Driver-moodle*: WCF service backed by an MS Access (.accdb) database, consumed by two clients — a Windows WPF desktop app and an ASP.NET Razor Pages web app.

**Why two projects?** The Hebrew syllabus *"Web Services, Async Programming and Databases"* requires demonstrating mastery of WCF + parameterized SQL + multi-client architecture. Driver-moodle covered driving-school management; B-Managed reuses every reusable pattern but applies it to a higher-stakes domain — money flow.

**Roles:** Owner, Employee, Client (3-tier).
**Stack:** WCF (BasicHttpBinding) ↔ Access via OleDb / WPF .NET 4.7.2 / ASP.NET Razor Pages .NET 8 / Tailwind CDN with Plus Jakarta Sans + Clash Display.

## 2. Architecture (3 layers)

```
[WPF / Web client]  →  [WCF Service1]  →  [ViewDB.* per-table]  →  [Access BManaged.accdb]
                            ↑
                        [BusinessLogic: VAT calc, PDF, Currency converter]
```

App.config wires `basicHttpBinding`, `maxReceivedMessageSize=20MB` (PDF over wire), `includeExceptionDetailInFaults=True` for development. Endpoint `localhost:8744`.

## 3. Database — 10 tables

`Users`, `Customers`, `Projects`, `Invoices`, `InvoiceLines`, `Expenses`, `ExpenseCategories`, `TaxPeriods`, `Notifications`, `ExchangeRates`. PowerShell bootstrap script `_init_db.ps1` creates the .accdb via ADOX, runs DDL, seeds an admin owner (`admin` / `admin1234`), 8 default categories and ILS↔USD rates.

## 4. Service contract

`IService1.cs` exposes ~64 `OperationContract` methods grouped by 8 banners: Auth, Customers, Projects, Invoices, Expenses, Reports/VAT, Currency, Notifications. Each mutating call in `Service1.cs` wraps DB exceptions in `FaultException` so error text reaches the client (lesson from Driver-moodle bug-fix work).

## 5. ViewDB — parameterized everywhere

All SQL goes through `OleDbParameter`. INSERT/UPDATE/DELETE either through `BaseDB.SaveChanges` or — for ops that must surface errors — through a direct `OleDbCommand` block. Reports use INNER JOIN + GROUP BY + SUM:

- 2-table JOIN: `Projects ⨝ Customers` (filter by owner).
- 4-table JOIN: `Invoices ⨝ Projects ⨝ Users ⨝ Customers` (employee revenue report).
- GROUP BY + SUM: top customers by revenue, expense breakdown by category.

## 6. Multi-currency (ILS + USD)

Every monetary table has a `currency` column. `CurrencyConverter` reads the latest matching row from `ExchangeRates` (effectiveDate ≤ asOfDate). Reports accept a `displayCurrency` parameter and normalize each row via the converter before aggregating.

## 7. Security

- **PBKDF2 hashing** — 16-byte salt + 10000 iterations + constant-time comparison (`SecurityHelper.HashPassword` / `VerifyPassword` / `SlowEquals`).
- **Sanitization** — `IsSafeString(input, maxLength)` rejects injection chars before any string is even passed to a parameterized query.
- **WPF ValidationRule** — 7 client-side rules (email regex, phone regex, min length, age range, lesson-price range, role enum, teacher-id existence check).
- **WPF IValueConverter** — `ImgConventer` maps `Yes/No` → image path.
- **WCF FaultException** — all mutating service methods wrap DB errors so the client sees real messages.

## 8. Polling (async UI)

- **WPF**: `DispatcherTimer` (15s) on `OwnerHome` — refresh KPI cards + notification badge.
- **Web**: `setInterval` (15s) calls `/Owner/Home?handler=Stats`, an Razor JSON endpoint that returns lightweight counters; client patches the DOM in place. No full-page reload.

## 9. Inheritance

- All models inherit from `Base` ({ Id }) → 11 entity classes.
- All ViewDB classes inherit from `BaseDB` (abstract `NewEntity()`, virtual `CreateModel(Base)`).
- `AllUsers : List<User>` with `[CollectionDataContract]` so it round-trips through WCF.
- Every WPF page inherits from `System.Windows.Controls.Page`.

## 10. WPF client (BManagedClient)

Pages: `LogIn` → role-aware navigation → `OwnerHome` / `EmployeeHome` / `ClientHome`. From `OwnerHome` a frame-based `Customers` page demonstrates CRUD with search.

Reusable building blocks (copied from Driver-moodle, re-namespaced):
- `ServiceGateway` — disposes the WCF client cleanly.
- `ClientSession` — typed accessors `IsOwner / IsEmployee / IsClient`.
- `Sign` — session DTO populated at login.
- `ValidationRules` + `ImgConventer`.

App.xaml exposes a small design-token resource set (Cyan / Green / Orange / Red brushes + `PrimaryButton` and `CardBorder` styles) used across pages.

## 11. Web client (BManagedWeb) — high-end visual design

Designed as a "$150k agency-grade" experience. Vibe = **Soft Structuralism**, layout = **Asymmetrical Bento** for the dashboard.

Highlights:
- **Floating glass nav** — pill-shaped, detached from the top, `backdrop-blur-xl`, role-aware links.
- **Double-Bezel cards** — outer shell + inner core with concentric radii (`rounded-shell` / `rounded-core`).
- **Button-in-button** CTA — main pill button with a nested circular arrow icon that translates on hover.
- **Magnetic hover physics** — `active:scale-[0.98]` + spring cubic-bezier `(0.32,0.72,0,1)`.
- **Scroll-reveal** — `IntersectionObserver` adds `.in` class for blur-fade-up entry.
- **Film grain** overlay via fixed pseudo-element pointer-events-none.
- **Typography** — Plus Jakarta Sans body + Clash Display headlines from Fontshare.

Pages: Login, Index (redirect by role), Logout, Owner/Home (bento), Owner/Customers (search + add), Owner/Reports (VAT + top customers + expense breakdown with currency selector), Employee/Home (assigned projects), Client/Portal (read-only invoice cards).

## 12. End-to-end flow — invoice creation

1. Owner adds a Customer → `Service1.AddCustomer` → `CustomerDB.Insert` (parameterized INSERT).
2. Owner creates a Project for that customer → `AddProject` → `ProjectDB.Insert`.
3. Owner creates an Invoice (Draft) → `CreateInvoice` → `InvoiceDB.Insert` + auto-numbering `INV-YYYY-NNNNN`.
4. Owner adds line items → `AddInvoiceLine` → `InvoiceLineDB.Insert` + `InvoiceDB.RecalcTotals` (sums `lineTotal`, applies `vatRate`, writes back subtotal/vatAmount/total).
5. Owner marks invoice Sent → `UpdateInvoiceStatus`.
6. Customer pays → Owner marks Paid → `MarkInvoicePaid` → UPDATE status='Paid', paidDate=today.
7. Reports refresh: `VatSummary` re-queries Invoices+Customers JOIN, sums by currency through `CurrencyConverter`.

## 13. Reports

Six dashboards: VAT Summary, Profit & Loss, Top Customers by Revenue, Expense Breakdown, Employee Revenue, Monthly Tax Set-Aside (30 % of profit + VAT due — Israel default).

## 14. Build, run, deploy

```
WcfServiceLibrary1/BManaged.sln       → MSBuild → 4 .NET 4.7.2 libraries
BManagedClient/BManagedClient.csproj  → MSBuild → BManagedClient.exe (WinExe)
BManagedWeb/BManagedWeb.csproj        → dotnet build / dotnet run → port 5050
```

Connected services (`bsrv` namespace) are generated locally with `dotnet-svcutil` once the WCF host is running; not committed to keep the repo clean.

## 15. Lessons from Driver-moodle baked in from day 1

| Issue caught in Driver-moodle | Built in correctly here |
|-------------------------------|--------------------------|
| `BaseDB.SaveChanges` swallowed `OleDbException` | New mutating ops use direct `OleDbCommand` and rethrow as `InvalidOperationException` |
| WCF generic SOAP fault hid real cause | `Service1` wraps every mutating op in `FaultException("op failed: " + base message)` |
| Hardcoded admin usernames in nav | Role + `IsOwner` flag stored in session at login; nav reads session |
| Date format mismatch between clients | All money has `currency`; reports take `displayCurrency`; FX picked by date |
| White-on-white in DataGrid on selection | WPF tokens explicit |
| Mark-as-read bool param ambiguous | All OleDb params declared with explicit `OleDbType` |
| Stats stale until full reload | `DispatcherTimer` (WPF) + `setInterval` JSON (Web) polling |

## 16. Files reference

- Server: `WcfServiceLibrary1/{IService1, Service1, App.config}` + `Model/*.cs` + `ViewDB/*.cs` + `BusinessLogic/*.cs`
- WPF: `BManagedClient/BManagedClient/{LogIn, OwnerHome, EmployeeHome, ClientHome, Customers, ClientSession, Sign, ValidationRules, ImgConventer}.{xaml,cs}`
- Web: `BManagedWeb/BManagedWeb/Pages/{Shared/_Layout, Login, Owner/{Home,Customers,Reports}, Employee/Home, Client/Portal}.cshtml`
- DB bootstrap: `_init_db.ps1`
- Plan: `../.claude/plans/lets-start-with-brain-drifting-fern.md`
- Hebrew book: `ספר_פרויקט_B_Managed_HE.md`

---

*End of English summary.*
