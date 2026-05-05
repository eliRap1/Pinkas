# Bootstraps WcfServiceLibrary1/ViewDB/Database/BManaged.accdb with all tables + seed rows.

$ErrorActionPreference = 'Stop'

$dbPath  = "D:\yudb\WcfServiceLibrary1\ViewDB\Database\BManaged.accdb"
$adminHash = "Wh07bZCwhjvwj4IsSR2nOWYpk6fWPUt6PZFFTLC6S8jg3qMC"  # password = admin1234
$today   = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")

if (Test-Path $dbPath) { Remove-Item $dbPath -Force }

# Create empty .accdb via ADOX (uses ACE 12 provider)
$cat = New-Object -ComObject ADOX.Catalog
$null = $cat.Create("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=$dbPath;")
[Runtime.InteropServices.Marshal]::ReleaseComObject($cat) | Out-Null
[GC]::Collect()

Add-Type -AssemblyName 'System.Data'
$conn = New-Object System.Data.OleDb.OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=$dbPath;Persist Security Info=True")
$conn.Open()

function Exec($sql) {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $sql
    [void]$cmd.ExecuteNonQuery()
}

# ===== DDL =====
Exec @"
CREATE TABLE [Users] (
  [id]            COUNTER PRIMARY KEY,
  [username]      TEXT(50) NOT NULL,
  [passwordHash]  TEXT(255) NOT NULL,
  [email]         TEXT(100),
  [phone]         TEXT(20),
  [role]          TEXT(20) NOT NULL,
  [isActive]      BIT NOT NULL,
  [createdAt]     DATETIME,
  [preferredCurrency] TEXT(3)
)
"@

Exec @"
CREATE TABLE [Customers] (
  [id]              COUNTER PRIMARY KEY,
  [businessName]    TEXT(100) NOT NULL,
  [contactName]     TEXT(80),
  [email]           TEXT(100),
  [phone]           TEXT(20),
  [taxId]           TEXT(20),
  [address]         TEXT(200),
  [ownerId]         LONG,
  [preferredCurrency] TEXT(3),
  [notes]           MEMO
)
"@

Exec @"
CREATE TABLE [Projects] (
  [id]                  COUNTER PRIMARY KEY,
  [customerId]          LONG NOT NULL,
  [title]               TEXT(120) NOT NULL,
  [description]         MEMO,
  [status]              TEXT(20),
  [startDate]           DATETIME,
  [dueDate]             DATETIME,
  [assignedEmployeeId]  LONG,
  [totalBudget]         CURRENCY,
  [currency]            TEXT(3)
)
"@

Exec @"
CREATE TABLE [Invoices] (
  [id]            COUNTER PRIMARY KEY,
  [invoiceNumber] TEXT(20) NOT NULL,
  [projectId]     LONG,
  [customerId]    LONG NOT NULL,
  [issueDate]     DATETIME,
  [dueDate]       DATETIME,
  [subtotal]      CURRENCY,
  [vatRate]       DOUBLE,
  [vatAmount]     CURRENCY,
  [total]         CURRENCY,
  [currency]      TEXT(3),
  [status]        TEXT(20),
  [paidDate]      DATETIME,
  [notes]         MEMO
)
"@

Exec @"
CREATE TABLE [InvoiceLines] (
  [id]          COUNTER PRIMARY KEY,
  [invoiceId]   LONG NOT NULL,
  [description] TEXT(255),
  [quantity]    DOUBLE,
  [unitPrice]   CURRENCY,
  [lineTotal]   CURRENCY,
  [currency]    TEXT(3)
)
"@

Exec @"
CREATE TABLE [ExpenseCategories] (
  [id]                COUNTER PRIMARY KEY,
  [name]              TEXT(50) NOT NULL,
  [isVatDeductible]   BIT
)
"@

Exec @"
CREATE TABLE [Expenses] (
  [id]            COUNTER PRIMARY KEY,
  [ownerId]       LONG NOT NULL,
  [categoryId]    LONG,
  [date]          DATETIME,
  [amount]        CURRENCY,
  [vatPaid]       CURRENCY,
  [vendor]        TEXT(100),
  [description]   MEMO,
  [projectId]     LONG,
  [receiptPath]   TEXT(255),
  [currency]      TEXT(3)
)
"@

Exec @"
CREATE TABLE [TaxPeriods] (
  [id]              COUNTER PRIMARY KEY,
  [year]            LONG,
  [month]           LONG,
  [vatCollected]    CURRENCY,
  [vatPaid]         CURRENCY,
  [vatDue]          CURRENCY,
  [incomeTotal]     CURRENCY,
  [expensesTotal]   CURRENCY,
  [profitEstimate]  CURRENCY,
  [taxSetAside]     CURRENCY,
  [currency]        TEXT(3)
)
"@

Exec @"
CREATE TABLE [Notifications] (
  [id]                COUNTER PRIMARY KEY,
  [userId]            LONG NOT NULL,
  [title]             TEXT(150),
  [message]           MEMO,
  [notificationType]  TEXT(30),
  [isRead]            BIT,
  [createdAt]         DATETIME,
  [readAt]            DATETIME
)
"@

Exec @"
CREATE TABLE [ExchangeRates] (
  [id]              COUNTER PRIMARY KEY,
  [fromCurrency]    TEXT(3) NOT NULL,
  [toCurrency]      TEXT(3) NOT NULL,
  [rate]            DOUBLE NOT NULL,
  [effectiveDate]   DATETIME NOT NULL
)
"@

# ===== seed rows =====
function MakeParam($name, $value) {
    $p = New-Object System.Data.OleDb.OleDbParameter
    $p.ParameterName = $name
    if ($null -eq $value) {
        $p.Value = [DBNull]::Value
    } elseif ($value -is [bool]) {
        $p.OleDbType = [System.Data.OleDb.OleDbType]::Boolean
        $p.Value = $value
    } elseif ($value -is [datetime]) {
        $p.OleDbType = [System.Data.OleDb.OleDbType]::Date
        $p.Value = $value
    } elseif ($value -is [int] -or $value -is [long]) {
        $p.OleDbType = [System.Data.OleDb.OleDbType]::Integer
        $p.Value = $value
    } elseif ($value -is [double] -or $value -is [decimal]) {
        $p.OleDbType = [System.Data.OleDb.OleDbType]::Double
        $p.Value = [double]$value
    } else {
        $p.OleDbType = [System.Data.OleDb.OleDbType]::VarWChar
        $p.Value = [string]$value
    }
    return $p
}

function ExecP($sql, $params) {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $sql
    foreach ($p in $params) {
        [void]$cmd.Parameters.Add((MakeParam $p[0] $p[1]))
    }
    [void]$cmd.ExecuteNonQuery()
}

# admin owner
ExecP "INSERT INTO [Users] ([username],[passwordHash],[email],[phone],[role],[isActive],[createdAt],[preferredCurrency]) VALUES (?,?,?,?,?,?,?,?)" @(
  @('@u','admin'),
  @('@p',$adminHash),
  @('@e','admin@b-managed.local'),
  @('@ph','0500000000'),
  @('@r','Owner'),
  @('@a',$true),
  @('@c',(Get-Date)),
  @('@cur','ILS')
)

# expense categories
$cats = @(
  @{ name='Equipment';  vat=$true },
  @{ name='Fuel';       vat=$true },
  @{ name='Rent';       vat=$true },
  @{ name='Utilities';  vat=$true },
  @{ name='Marketing';  vat=$true },
  @{ name='Travel';     vat=$true },
  @{ name='Meals';      vat=$false },
  @{ name='Other';      vat=$false }
)
foreach ($c in $cats) {
  ExecP "INSERT INTO [ExpenseCategories] ([name],[isVatDeductible]) VALUES (?,?)" @(
    @('@n', $c.name),
    @('@v', $c.vat)
  )
}

# exchange rates seed (manual, latest USD ≈ 3.7 ILS)
$today2 = Get-Date
ExecP "INSERT INTO [ExchangeRates] ([fromCurrency],[toCurrency],[rate],[effectiveDate]) VALUES (?,?,?,?)" @(
  @('@f','USD'), @('@t','ILS'), @('@r',3.7), @('@d',$today2)
)
ExecP "INSERT INTO [ExchangeRates] ([fromCurrency],[toCurrency],[rate],[effectiveDate]) VALUES (?,?,?,?)" @(
  @('@f','ILS'), @('@t','USD'), @('@r',0.27), @('@d',$today2)
)

$conn.Close()
Write-Host "OK BManaged.accdb created at $dbPath"
