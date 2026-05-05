# B-Managed

All-in-One management system for freelancers and small businesses.

Same architecture as the **Driver-moodle** sister project:
WCF service ↔ MS Access (.accdb) backend, with two clients (WPF + Web Razor Pages).

## Layout

```
yudb/
├── WcfServiceLibrary1/         server (WCF, BasicHttpBinding)
│   ├── WcfServiceLibrary1/     IService1, Service1, App.config
│   ├── Model/                  DataContract classes + Helpers/SecurityHelper.cs
│   ├── ViewDB/                 BaseDB + per-table DBs
│   │   └── Database/BManaged.accdb
│   └── BusinessLogic/          VAT, currency converter, invoice numberer, PDF
├── BManagedClient/             WPF desktop client (Owner + Employee)
├── BManagedWeb/                ASP.NET Razor Pages (all 3 roles + client portal)
├── nav-map/                    flow PNGs + screenshots
├── ספר_פרויקט_B_Managed_HE.md  project book (Hebrew)
└── BManaged_ProjectBook_EN.md  project book (English)
```

## Roles

| Role | WPF | Web |
|------|-----|-----|
| **Owner** | full access (CRM, projects, invoices, expenses, reports, employees) | full access |
| **Employee** | assigned projects, log own expenses | same (read-mostly) |
| **Client** | – | own invoices + project status portal |

## MVP features

1. **CRM** — customers + projects with status (active / done / awaiting payment)
2. **Invoicing** — auto-numbered, line items, PDF export (QuestPDF), mark paid
3. **Expense tracking** — categories, VAT-deductible flag, optional receipt upload
4. **VAT / tax reports** — auto-calc VAT due, monthly tax-set-aside, P&L

## Currencies

- **ILS** (₪) — default
- **USD** ($) — alternate
- Per-customer preferred currency, conversion via `ExchangeRates` table.

## Polling

WPF uses `DispatcherTimer` (10–60 s); Web uses `setInterval` JSON fetch — same
pattern as Driver-moodle.
