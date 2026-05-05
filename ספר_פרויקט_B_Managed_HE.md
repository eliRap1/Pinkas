# ספר פרויקט בהנדסת תוכנה
## B-Managed — מערכת All-in-One לניהול עסק קטן

| שדה | ערך |
|-----|-----|
| חלופה | שירותי אינטרנט, תכנות אסינכרוני ומסדי נתונים |
| שם בית הספר | __________ |
| שם התלמיד | __________ |
| שם המנחה | __________ |
| תאריך הגשה | 05/05/2026 |

> מסמך זה נכתב לפי המסמכים *פירוט דרישות חלק 1* ו-*בדיקת פרויקט וספר פרויקט עפ"י דרישות*. כל אלמנט בקוד מתועד עם הפנייה לקובץ ושורה (`קובץ:שורה`) וקטע קוד מהפרויקט עצמו.

---

## תוכן עניינים
1. מבוא ורקע
2. מענה על דרישות המחוון
3. ניתוח מערכת
4. ארכיטקטורה
5. בסיס הנתונים Access
6. שכבת ה-Model + DataContract
7. שכבת השרת WCF (IService1, Service1, App.config)
8. שכבת ViewDB + SQL מתקדם (INSERT, UPDATE, INNER JOIN, GROUP BY)
9. אבטחה ובדיקות קלט (PBKDF2, Validation, IValueConverter, SQL Injection)
10. רב-מטבעיות (ILS + USD) + CurrencyConverter
11. לקוח WPF (BManagedClient)
12. לקוח Web (BManagedWeb) + עיצוב High-end
13. תהליכים עסקיים מקצה-לקצה
14. דו"חות
15. Polling אסינכרוני (DispatcherTimer + setInterval)
16. ירושה (Inheritance)
17. סיכום ורפלקציה

---

## 1. מבוא ורקע

**B-Managed** היא מערכת מידע All-in-One לניהול עסק קטן עצמאי או חברה זעירה. הרעיון: עסק קטן מאבד שליטה על שלושה תהליכים בו-זמנית — לקוחות, פרויקטים, וכסף. הפרויקט מאחד את שלושתם בארכיטקטורה מקצועית של שרת WCF + שני לקוחות.

**מטרת על:** לתת לבעל-עסק (Owner) שליטה מלאה על הפעילות העסקית, לעובד (Employee) גישה לפרויקטים שהוקצו לו, וללקוח (Client) פורטל לצפייה בחשבוניות.

**Stack:** WCF (BasicHttpBinding) ↔ Access (.accdb) דרך OleDb ; לקוחות: WPF (.NET Framework 4.7.2) + ASP.NET Razor Pages (.NET 8).

**הרחבה ייחודית:** רב-מטבעיות (ILS / USD) — כל סכום נשמר במטבע המקורי + מומר על-פי דרישת תצוגה דרך טבלת `ExchangeRates`.

---

## 2. מענה על דרישות המחוון

### 2.1 דרישות חובה (סעיפים 1–8)

| # | דרישה | מענה | מקום בקוד |
|---|--------|-------|------------|
| 1 | תוכנית = ממשק מלא למערכת מידע + שליפה ועדכון מטבלאות | Service1 חושף ~64 פעולות שירות הפועלות על 10 טבלאות | `WcfServiceLibrary1/IService1.cs` (שורות 1-130) |
| 2 | מסד נתונים בשרת + ממשקי משתמש עם פקדים שונים | Access .accdb + WPF (TextBox, ListView, DataGrid, ComboBox, DatePicker) + Razor (forms, select, table, cards) | `WcfServiceLibrary1/ViewDB/Database/BManaged.accdb` |
| 3 | בסיס מנורמל עם 2-4 טבלאות + טבלת קישור | 10 טבלאות. טבלאות קישור: `Projects` (Customer↔User), `Invoices` (Customer↔Project), `InvoiceLines` (Invoice↔פריט), `Expenses` (User↔Category↔Project), `Notifications` | פרק 5 |
| 4 | 2–3 לקוחות (חלונאי, אינטרנטי, סלולר) | **שניים** — WPF (חלונאי) + Razor Pages (אינטרנטי) | פרקים 11, 12 |
| 5 | שאילתות מורכבות, חיתוך ועדכון | 4 שאילתות INNER JOIN + 2 עם GROUP BY + SUM (Lessons↔Customers↔Invoices, Invoices↔Customers, Expenses↔Categories, Invoices↔Projects↔Users↔Customers) | פרק 8 |
| 6 | OOP + ירושה ממחלקות שכתב התלמיד | כל המודלים יורשים מ-`Base`. כל ה-DBים יורשים מ-`BaseDB`. `AllUsers : List<User>`. דפי WPF יורשים מ-`Page`. | פרק 16 |
| 7 | מספר רמות הרשאה | שלוש: **Owner, Employee, Client** | פרק 11 + 12 |
| 8 | UI מציג רק אפשרויות לפי הרשאה | _Layout.cshtml מסתיר לפי `Session["Role"]`. `RequireRoleAsync` ב-WPF. `OwnerHome` בודק `ClientSession.IsOwner`. | פרק 12 |

### 2.2 הרחבות (סעיף 9)
- **A. הצפנה חד-סיטרית** — סיסמאות PBKDF2 + Salt 16-byte + 10000 iterations + SlowEquals (constant-time). `Model/Helpers/SecurityHelper.cs:21-106`.
- **B. העברת קבצים שרת↔לקוח דרך ValueConverter** — `ImgConventer` ממיר `Yes/No` לתמונה. PDF של חשבונית עובר כ-`byte[]` מהשרת ללקוח. `BManagedClient/ImgConventer.cs`.
- **C. ריבוי משתמשים בו-זמנית** — Owner+Employee+Client.

### 2.3 הרחבות (סעיף 10)
1. **IValueConverter** — `BManagedClient/ImgConventer.cs`.
2. **ValidationRule** — 7 כללים: AgeRange, EmailRule, PhoneRule, isAdminRule, MinLenth, LessonPriceRule, TeacherIdRule. `BManagedClient/ValidationRules.cs`.
3. **Service חיצוני** — WCF נצרך ע"י WPF + Razor.
4. **Async** — Razor `async` Pages + WPF `DispatcherTimer` polling (15s) — פרק 15.
5. **Files (XML)** — App.config של WCF.
6. **הגנה מ-SQL Injection** — `OleDbParameter` בכל שאילתה, `IsSafeString` למניעה. `Model/Helpers/SecurityHelper.cs:130-153`.
7. **Async / Timers** — `DispatcherTimer` (WPF) + `setInterval` JSON (Web).

---

## 3. ניתוח מערכת

### 3.1 Use Cases מרכזיים
1. **התחברות** — מזהה תפקיד ומנתב לדשבורד נכון.
2. **ניהול לקוח** — Owner מוסיף, מחפש, עורך לקוח.
3. **יצירת פרויקט** — Owner משייך פרויקט ללקוח + עובד.
4. **הפקת חשבונית** — Owner יוצר חשבונית עם פריטים → חישוב VAT → סטטוס Draft/Sent/Paid.
5. **רישום הוצאה** — Owner/Employee רושם הוצאה עם קטגוריה.
6. **דו"ח VAT** — חישוב חודשי של VAT לתשלום.

### 3.2 DFD-0
```
[משתמש WPF / Web]  →  [WCF Proxy]  →  [Service1]  →  [ViewDB.*DB]  →  [OleDb]  →  [BManaged.accdb]
                                          ↓
                                   [BusinessLogic]
                                   (VAT, PDF, Numbering)
```

### 3.3 עץ תהליכים
- **CRM** — לקוח → פרויקט → חשבונית → תשלום
- **כספים** — חשבונית/הוצאה → סיכום חודשי → VAT due → דו"ח
- **תקשורת** — Notification → polling → badge

---

## 4. ארכיטקטורה

ארכיטקטורת **3 שכבות**:
1. **נתונים** — Access + ViewDB
2. **עסק** — Service1 + BusinessLogic
3. **תצוגה** — WPF + Web

מבנה הריפו `D:/yudb/`:
```
WcfServiceLibrary1/
├── WcfServiceLibrary1/   ← App.config + IService1 + Service1
├── Model/                ← DataContract
├── ViewDB/               ← BaseDB + per-table DBs + BManaged.accdb
└── BusinessLogic/        ← VAT, PDF, Numbering
BManagedClient/           ← WPF
BManagedWeb/              ← Razor Pages
```

### App.config (WCF)
`WcfServiceLibrary1/WcfServiceLibrary1/App.config`:
```xml
<basicHttpBinding>
  <binding name="BasicHttpBinding_IService1"
           maxReceivedMessageSize="20000000"
           maxBufferSize="20000000" />
</basicHttpBinding>
<service name="WcfServiceLibrary1.Service1">
  <endpoint address="" binding="basicHttpBinding"
            bindingConfiguration="BasicHttpBinding_IService1"
            contract="WcfServiceLibrary1.IService1"/>
  <endpoint address="mex" binding="mexHttpBinding" contract="IMetadataExchange"/>
</service>
<serviceDebug includeExceptionDetailInFaults="True" />
```
**הסבר השדות:** `basicHttpBinding` בחירה — תומך גם ב-Razor Core שלא מכיר WSHttpBinding. `maxReceivedMessageSize=20MB` — מאפשר העברת PDF של חשבונית בכמות של מספר עמודים. `includeExceptionDetailInFaults=True` בפיתוח — מאפשר ללקוח לראות שגיאות אמיתיות (לא ב-production).

---

## 5. בסיס הנתונים Access

10 טבלאות (כולן עם PK = COUNTER):
1. **Users** — id, username, passwordHash (PBKDF2), email, phone, role, isActive, createdAt, preferredCurrency
2. **Customers** — id, businessName, contactName, email, phone, taxId, address, ownerId(FK), preferredCurrency, notes
3. **Projects** — id, customerId(FK), title, description, status, startDate, dueDate, assignedEmployeeId(FK), totalBudget, currency
4. **Invoices** — id, invoiceNumber, projectId(FK), customerId(FK), issueDate, dueDate, subtotal, vatRate, vatAmount, total, currency, status, paidDate, notes
5. **InvoiceLines** — id, invoiceId(FK), description, quantity, unitPrice, lineTotal, currency
6. **Expenses** — id, ownerId(FK), categoryId(FK), date, amount, vatPaid, vendor, description, projectId(FK), receiptPath, currency
7. **ExpenseCategories** — id, name, isVatDeductible
8. **TaxPeriods** — id, year, month, vatCollected, vatPaid, vatDue, incomeTotal, expensesTotal, profitEstimate, taxSetAside, currency
9. **Notifications** — id, userId(FK), title, message, notificationType, isRead, createdAt, readAt
10. **ExchangeRates** — id, fromCurrency, toCurrency, rate, effectiveDate

**Seed:** משתמש admin (סיסמה `admin1234`, role=`Owner`), 8 קטגוריות הוצאה, שני שערי חליפין ILS↔USD. הקובץ `_init_db.ps1` בונה את ה-DB באופן אוטומטי.

```powershell
# _init_db.ps1 — קטע מייצג
$cat = New-Object -ComObject ADOX.Catalog
$cat.Create("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=$dbPath;")
Exec @"CREATE TABLE [Users] (
  [id] COUNTER PRIMARY KEY, [username] TEXT(50) NOT NULL,
  [passwordHash] TEXT(255) NOT NULL, ...)
"@
```

---

## 6. שכבת ה-Model + DataContract

`Base.cs` — מחלקת בסיס לכל הישויות:
```csharp
[DataContract]
public class Base
{
    [DataMember] public int Id { get; set; }
}
```

`User.cs` (קטע) — `WcfServiceLibrary1/Model/User.cs:11-22`:
```csharp
[DataContract]
public class User : Base
{
    [DataMember] public string Username { get; set; }
    [DataMember] public string PasswordHash { get; set; }
    [DataMember] public string Role { get; set; } = "Client";
    [DataMember] public string PreferredCurrency { get; set; } = "ILS";
    ...
}
```

סך הכל: 11 מחלקות מודל + 5 DTOs לדו"חות (Reports.cs) + `AllUsers : List<User>` (CollectionDataContract).

---

## 7. שכבת השרת WCF

### 7.1 IService1 — מקובץ ל-8 קטגוריות
`WcfServiceLibrary1/IService1.cs:12-130` (קטעים מייצגים):
```csharp
[ServiceContract]
public interface IService1
{
    // ===== AUTH & USERS =====
    [OperationContract] bool CheckUserPassword(string username, string password);
    [OperationContract] User GetUserById(int id);
    [OperationContract] bool AddUser(string username, string password, string email,
                                     string phone, string role, string preferredCurrency);

    // ===== CUSTOMERS / CRM =====
    [OperationContract] int  AddCustomer(Customer c);
    [OperationContract] List<Customer> GetCustomersForOwner(int ownerId);
    [OperationContract] List<Customer> SearchCustomers(string keyword, int ownerId);

    // ===== INVOICES =====
    [OperationContract] int  CreateInvoice(Invoice inv);
    [OperationContract] int  AddInvoiceLine(InvoiceLine line);
    [OperationContract] void MarkInvoicePaid(int invoiceId, DateTime paidDate);
    [OperationContract] byte[] GenerateInvoicePdf(int invoiceId);

    // ===== REPORTS / VAT =====
    [OperationContract] VatSummary GetVatSummary(int ownerId, int year, int month, string displayCurrency);
    [OperationContract] List<CustomerRevenueRow> GetTopCustomersByRevenue(int ownerId, string displayCurrency);
    ...
}
```

### 7.2 Service1 — מימוש עם FaultException
`WcfServiceLibrary1/Service1.cs:120-130`:
```csharp
public void MarkInvoicePaid(int id, DateTime paidDate)
{
    try { invDB.MarkPaid(id, paidDate); }
    catch (Exception ex)
    { throw new FaultException("MarkInvoicePaid failed: " + ex.Message); }
}
```
שיפור שלמדנו מ-Driver-moodle: עוטפים שגיאות DB ב-`FaultException` כדי שהן יגיעו ללקוח עם הודעה אמיתית.

---

## 8. שכבת ViewDB + SQL מתקדם

### 8.1 BaseDB — Select פרמטרי בטוח מ-SQL Injection
`WcfServiceLibrary1/ViewDB/BaseDB.cs:54-90`:
```csharp
protected virtual List<Base> Select(string sql, params OleDbParameter[] parameters)
{
    connection.Open();
    command.CommandText = sql;
    command.Parameters.Clear();
    if (parameters != null) command.Parameters.AddRange(parameters);
    reader = command.ExecuteReader();
    while (reader.Read()) { var e = NewEntity(); CreateModel(e); list.Add(e); }
    return list;
}
```

### 8.2 INSERT עם OleDbType מפורש (UserDB)
`WcfServiceLibrary1/ViewDB/UserDB.cs:60-80`:
```csharp
public int Insert(User u)
{
    string sql = @"INSERT INTO [Users]
                  ([username],[passwordHash],[email],[phone],[role],[isActive],[createdAt],[preferredCurrency])
                   VALUES (?,?,?,?,?,?,?,?)";
    using (var conn = GetConnection())
    using (var cmd = new OleDbCommand(sql, conn))
    {
        cmd.Parameters.Add(new OleDbParameter("@u",   OleDbType.VarWChar, 50)  { Value = u.Username });
        cmd.Parameters.Add(new OleDbParameter("@p",   OleDbType.VarWChar, 255) { Value = u.PasswordHash });
        cmd.Parameters.Add(new OleDbParameter("@a",   OleDbType.Boolean) { Value = u.IsActive });
        cmd.Parameters.Add(new OleDbParameter("@c",   OleDbType.Date)    { Value = u.CreatedAt });
        ...
        conn.Open();
        return cmd.ExecuteNonQuery();
    }
}
```

### 8.3 UPDATE
`UserDB.SetPassword` — `WcfServiceLibrary1/ViewDB/UserDB.cs:85-95`:
```csharp
public void SetPassword(int userId, string hash)
{
    using (var conn = GetConnection())
    using (var cmd = new OleDbCommand("UPDATE [Users] SET [passwordHash]=? WHERE [id]=?", conn))
    { ... if (cmd.ExecuteNonQuery() <= 0)
            throw new InvalidOperationException("SetPassword: user not found"); }
}
```

### 8.4 INNER JOIN — שאילתות דו"ח מורכבות

**א. Projects לפי Status בתוך Owner מסוים** — JOIN של 2 טבלאות:
```sql
SELECT P.* FROM [Projects] AS P
INNER JOIN [Customers] AS C ON P.[customerId] = C.[id]
WHERE P.[status] = ? AND C.[ownerId] = ?
```
מקור: `ProjectDB.GetByStatus`.

**ב. Top Customers by Revenue** — JOIN + GROUP BY + SUM:
```sql
SELECT C.[id], C.[businessName],
       SUM(I.[total])     AS invoiced,
       SUM(IIF(I.[status]='Paid', I.[total], 0)) AS paid,
       I.[currency]
FROM [Customers] AS C
INNER JOIN [Invoices] AS I ON I.[customerId] = C.[id]
WHERE C.[ownerId] = ?
GROUP BY C.[id], C.[businessName], I.[currency]
ORDER BY SUM(I.[total]) DESC
```
מקור: `ReportsDB.TopCustomersByRevenue`.

**ג. Expense Breakdown** — JOIN + GROUP BY + isVatDeductible:
```sql
SELECT EC.[name], EC.[isVatDeductible], SUM(E.[amount]) AS total, E.[currency]
FROM [Expenses] AS E
INNER JOIN [ExpenseCategories] AS EC ON E.[categoryId] = EC.[id]
WHERE E.[ownerId] = ? AND E.[date] >= ? AND E.[date] <= ?
GROUP BY EC.[name], EC.[isVatDeductible], E.[currency]
ORDER BY SUM(E.[amount]) DESC
```
מקור: `ReportsDB.ExpenseBreakdown`.

**ד. Employee Revenue** — JOIN של 4 טבלאות:
```sql
SELECT U.[id], U.[username], COUNT(DISTINCT P.[id]) AS projects,
       SUM(IIF(I.[status]='Paid', I.[total], 0)) AS revenue, I.[currency]
FROM [Invoices] AS I
INNER JOIN [Projects]  AS P ON I.[projectId]      = P.[id]
INNER JOIN [Users]     AS U ON P.[assignedEmployeeId] = U.[id]
INNER JOIN [Customers] AS C ON I.[customerId]     = C.[id]
WHERE C.[ownerId] = ?
GROUP BY U.[id], U.[username], I.[currency]
```
מקור: `ReportsDB.EmployeeRevenueReport` — 4-table JOIN בודק רק מה ש"רץ" דרך עובד.

---

## 9. אבטחה ובדיקות קלט

### 9.1 PBKDF2 (חד-סיטרי)
`Model/Helpers/SecurityHelper.cs:21-106`:
```csharp
public static string HashPassword(string password)
{
    byte[] salt = new byte[16];
    using (var rng = new RNGCryptoServiceProvider()) rng.GetBytes(salt);
    using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000))
    {
        byte[] hash = pbkdf2.GetBytes(20);
        byte[] hashBytes = new byte[36];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 20);
        return Convert.ToBase64String(hashBytes);
    }
}

private static bool SlowEquals(byte[] a, byte[] b)
{
    uint diff = (uint)a.Length ^ (uint)b.Length;
    for (int i = 0; i < a.Length && i < b.Length; i++)
        diff |= (uint)(a[i] ^ b[i]);
    return diff == 0; // constant-time = manaeg למנוע timing attacks
}
```

### 9.2 IsSafeString — סניטציית קלט
`SecurityHelper.cs:130-153`:
```csharp
public static bool IsSafeString(string input, int maxLength = 100)
{
    if (string.IsNullOrEmpty(input) || input.Length > maxLength) return false;
    foreach (char c in input)
        if (!char.IsLetterOrDigit(c) && c != ' ' && c != '@' && c != '.' && c != '-' && c != '_')
            return false;
    return true;
}
```

### 9.3 ValidationRules ב-WPF
`BManagedClient/ValidationRules.cs` — 7 כללים. דוגמה — `EmailRule`:
```csharp
public class EmailRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo culture)
    {
        Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
        return regex.IsMatch((string)value)
            ? ValidationResult.ValidResult
            : new ValidationResult(false, "Please enter a legal Email.");
    }
}
```
שימוש ב-XAML:
```xml
<TextBox.Text>
    <Binding Path="Email" UpdateSourceTrigger="PropertyChanged">
        <Binding.ValidationRules><local:EmailRule/></Binding.ValidationRules>
    </Binding>
</TextBox.Text>
```

### 9.4 IValueConverter
`BManagedClient/ImgConventer.cs`:
```csharp
public class ImgConventer : IValueConverter
{
    public object Convert(object v, Type t, object p, CultureInfo c)
    {
        if (v == null) return DependencyProperty.UnsetValue;
        return v.ToString() == "Yes" ? "picture/check.jpg" : "picture/cross.png";
    }
}
```

---

## 10. רב-מטבעיות — ILS + USD

### 10.1 CurrencyConverter
`WcfServiceLibrary1/ViewDB/CurrencyConverter.cs`:
```csharp
public decimal Convert(decimal amount, string from, string to, DateTime asOfDate)
{
    if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase)) return amount;
    double rate = _db.GetLatestRate(from, to, asOfDate);
    return Math.Round(amount * (decimal)rate, 2);
}
```

### 10.2 ExchangeRateDB.GetLatestRate — Latest-by-date
`WcfServiceLibrary1/ViewDB/ExchangeRateDB.cs:28-42`:
```csharp
public double GetLatestRate(string from, string to, DateTime asOfDate)
{
    if (string.Equals(from, to, StringComparison.OrdinalIgnoreCase)) return 1.0;
    object r = SelectScalar(
        @"SELECT TOP 1 [rate] FROM [ExchangeRates]
          WHERE [fromCurrency]=? AND [toCurrency]=? AND [effectiveDate] <= ?
          ORDER BY [effectiveDate] DESC",
        new OleDbParameter("@f", from),
        new OleDbParameter("@t", to),
        new OleDbParameter("@d", asOfDate));
    return (r != null) ? Convert.ToDouble(r) : 1.0;
}
```

### 10.3 דו"חות שמנרמלים ל-displayCurrency
`ReportsDB.VatSummary` קורא רשומות במטבע מקור, ממיר לכל אחת ל-`displayCurrency` לפי תאריכה, ואז מסכם — כל זאת על השרת.

---

## 11. לקוח WPF (BManagedClient)

### 11.1 ניווט בין דפים
`MainWindow.xaml.cs`:
```csharp
public MainWindow()
{
    InitializeComponent();
    page.Navigate(new LogIn());
}
```

### 11.2 LogIn — בדיקת תפקיד וניתוב
`BManagedClient/LogIn.xaml.cs:21-50`:
```csharp
private void signIn_Click(object sender, RoutedEventArgs e)
{
    bool ok = ServiceGateway.Use(c => c.CheckUserPassword(u, p));
    if (!ok) { ShowError("Wrong username or password."); return; }

    var user = ServiceGateway.Use(c => c.GetUserById(c.GetUserId(u)));
    sign.Role = user.Role;
    if (sign.IsOwner)         page.Navigate(new OwnerHome());
    else if (sign.IsEmployee) page.Navigate(new EmployeeHome());
    else                      page.Navigate(new ClientHome());
}
```

### 11.3 OwnerHome — DispatcherTimer
`BManagedClient/OwnerHome.xaml.cs:23-42`:
```csharp
public OwnerHome()
{
    InitializeComponent();
    if (!ClientSession.IsOwner) { /* redirect */ return; }
    RefreshStats();
    pollTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(15) };
    pollTimer.Tick += (s, e) => RefreshStats();
    pollTimer.Start();
    Unloaded += (s, e) => pollTimer.Stop();
}
```

---

## 12. לקוח Web (BManagedWeb) — עיצוב High-end

ה-Web נבנה לפי שפת עיצוב **Soft Structuralism** — סקאלת "אגנסיית-עיצוב $150k": Plus Jakarta Sans + Clash Display, רקע ניירי (`#FAFAF7`), `Double-Bezel` של כרטיסים, כפתורים עם "button-in-button", `Asymmetrical Bento` ל-dashboard, `Editorial Split` לדפי-פרטים.

### 12.1 Layout עם Glass Pill
`BManagedWeb/Pages/Shared/_Layout.cshtml`:
```html
<header class="sticky top-0 z-40 pt-6 px-4">
    <nav class="mx-auto w-max bg-white/70 backdrop-blur-xl border border-black/5
                rounded-full shadow-soft px-2 py-2 flex items-center gap-1">
        <a class="display text-base font-bold px-4">B-Managed</a>
        @if (isOwner) {
            <a class="px-3 py-1.5 rounded-full hover:bg-black/5 transition">Dashboard</a>
            ...
        }
    </nav>
</header>
```

### 12.2 Owner Dashboard — Asymmetrical Bento
`Pages/Owner/Home.cshtml`:
```html
<section class="grid grid-cols-1 md:grid-cols-12 gap-4 md:gap-6">
    <div class="md:col-span-8 md:row-span-2 ...">
        <span class="pill text-ink/40">Outstanding</span>
        <div class="display text-7xl md:text-[8rem] font-bold">@Model.UnpaidTotalDisplay</div>
    </div>
    <div class="md:col-span-4 ..."> VAT due </div>
    <div class="md:col-span-4 ..."> Quick action </div>
</section>
```

### 12.3 Polling — setInterval + JSON handler
```javascript
setInterval(async () => {
    const r = await fetch('/Owner/Home?handler=Stats');
    const d = await r.json();
    // refresh counters
}, 15000);
```
Server endpoint: `OnGetStats` ב-Home.cshtml.cs.

### 12.4 Login (Web)
מעוצב כ-Double-Bezel card:
```html
<div class="bg-black/[0.04] ring-1 ring-black/5 rounded-shell p-1.5 shadow-float">
    <div class="bg-paper-50 rounded-core shadow-inset p-8">
        <form method="post" class="space-y-5">
            <input asp-for="Username" class="..."/>
            <input asp-for="Password" type="password" class="..."/>
            <button class="group w-full ... bg-ink text-paper-50 rounded-full px-6 py-3
                           ease-spring active:scale-[0.98]">
                Sign in
                <span class="w-7 h-7 rounded-full bg-white/15 flex items-center
                             justify-center group-hover:translate-x-0.5">→</span>
            </button>
        </form>
    </div>
</div>
```

---

## 13. תהליכים עסקיים מקצה-לקצה

### 13.1 הפקת חשבונית
1. Owner ב-`Pages/Owner/Invoices` יוצר חשבונית → `Service1.CreateInvoice` → `InvoiceDB.Insert` → autonum + invoiceNumber `INV-YYYY-NNNNN` → Draft.
2. Owner מוסיף שורות → `AddInvoiceLine` → `InvoiceDB.RecalcTotals` (subtotal = SUM(lineTotal), vat = subtotal × rate, total).
3. Owner מסמן Sent → סטטוס Sent.
4. לקוח משלם → Owner מסמן Paid → `InvoiceDB.MarkPaid` → UPDATE [status]='Paid', paidDate=today.

### 13.2 רישום הוצאה + VAT
1. Owner ב-`Pages/Owner/Expenses` רושם הוצאה עם קטגוריה.
2. בסוף החודש: `ReportsDB.VatSummary(ownerId, year, month, displayCurrency)`:
   - INNER JOIN Invoices ↔ Customers → סוכם vatCollected
   - INNER JOIN Expenses ↔ ExpenseCategories → סוכם vatPaid (רק אם isVatDeductible)
   - vatDue = collected - paid

### 13.3 צפיית לקוח בחשבוניות
ה-Client login → `Pages/Client/Portal` → טוענת חשבוניות שהוקצו ללקוח → מציגה סטטוס + סכום.

---

## 14. דו"חות

| דו"ח | טכניקת SQL | מקור |
|------|-------------|-------|
| VAT Summary | INNER JOIN × 2 + filter שנה/חודש | `ReportsDB.VatSummary` |
| Profit & Loss | INNER JOIN + period filter | `ReportsDB.ProfitLoss` |
| Top Customers | INNER JOIN + GROUP BY + SUM + ORDER BY | `ReportsDB.TopCustomersByRevenue` |
| Expense Breakdown | INNER JOIN + GROUP BY + SUM | `ReportsDB.ExpenseBreakdown` |
| Employee Revenue | 4-table INNER JOIN + GROUP BY + IIF | `ReportsDB.EmployeeRevenueReport` |
| Monthly Tax Set-Aside | Aggregation + 30% rule | `ReportsDB.MonthlyTaxSetAside` |

---

## 15. Polling אסינכרוני

| מקום | טכנולוגיה | מרווח | מטרה |
|-------|------------|--------|-------|
| WPF OwnerHome | `DispatcherTimer` | 15s | רענון מונים + badge התראות |
| Web OwnerHome | JS `setInterval` + fetch JSON | 15s | רענון KPI cards |
| WPF EmployeeHome | `DispatcherTimer` | 15s | סטטוס פרויקטים |

---

## 16. ירושה (Inheritance)

- **Models** — כולם יורשים מ-`Base`. דוגמה: `User : Base`, `Customer : Base`, `Invoice : Base`, `Notification : Base`.
- **ViewDB** — כל מחלקה יורשת מ-`BaseDB` ומחויבת לממש `NewEntity()` ו-`CreateModel()`.
  ```csharp
  public abstract class BaseDB
  {
      protected abstract Base NewEntity();
      protected virtual void CreateModel(Base entity) { /* read [id] */ }
  }
  public class UserDB : BaseDB
  {
      protected override Base NewEntity() => new User();
      protected override void CreateModel(Base entity) { /* fill UserInfo fields */ }
  }
  ```
- **Collection** — `AllUsers : List<User>` עם `[CollectionDataContract]` כדי שיעבור נכון דרך WCF.
- **WPF Pages** — כל מסך יורש מ-`Page`.

---

## 17. סיכום ורפלקציה

הפרויקט עומד בכל דרישות החובה (1–8) + הרחבות (סעיף 9 שני נושאים, סעיף 10 שבעה נושאים). הקוד מבוסס על תבניות שנבדקו והוכחו ב-Driver-moodle (BaseDB עם פרמטרים, PBKDF2, ValidationRules, IValueConverter, FaultException) — מה שאיפשר זמן פיתוח קצר עם איכות גבוהה.

**שיפור מרכזי על Driver-moodle:** רב-מטבעיות מובנית. כל סכום נשמר עם המטבע שלו, וכל דו"ח מקבל `displayCurrency` כפרמטר, ממיר רשומות לפי תאריכן דרך `CurrencyConverter`, ואז מסכם.

**הרחבה ייחודית של ה-UI:** הלקוח האינטרנטי נבנה בשפת עיצוב High-end (Soft Structuralism) — Plus Jakarta Sans + Clash Display, Double-Bezel cards, Asymmetrical Bento, button-in-button, scroll-reveal עם IntersectionObserver, glass pill nav. הסגנון ניכר מיידית כ"agency-grade" ולא "Bootstrap template".

**מה למדנו:**
- אסור ל-DB layer לבלוע שגיאות OleDb — חייבים `FaultException` עם פרטים.
- כל שאילתה מקבילה מקבלת פרמטרים מפורשים `OleDbType` למניעת data type mismatch.
- ירושה אמיתית (BaseDB) חוסכת מאות שורות קוד חוזר.

**לעתיד:** הוספת לקוח MAUI (סלולרי), השתלבות עם API חיצוני של מטבעות חוץ (Bank of Israel), QuestPDF מלא לחשבוניות RTL בעברית.
