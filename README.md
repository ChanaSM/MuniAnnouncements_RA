# MuniAnnouncements_RA – "הבקשות שלי"

מיקרו-שירות שמרכז עבור תושב, באזור האישי, את כל הבקשות שהגיש לרשויות השונות (הנחה בארנונה, תו חניה, דיווח על מפגע ועוד) בתצוגה אחודה, עם הסטטוס העדכני של כל בקשה וסימון כשנדרשת פעולה מצד התושב.

הפרויקט כולל שירות .NET 8 ורכיב Angular 21. מערכת הטפסים ו-MongoDB **מדומות** בנתונים קבועים, כנדרש.

## מבנה

```
MuniAnnouncements_RA/
├── src/MuniAnnouncements.Api/        שירות ה-API (ASP.NET Core, .NET 8)
│   ├── Controllers/                  MyRequestsController: GET /api/my-requests
│   ├── Services/                     MyRequestsService (תזמור), RequestNormalizer (הלוגיקה)
│   ├── Clients/                      IFormsSystemClient + FakeFormsSystemClient (מערכת הטפסים)
│   ├── Repositories/                 IServiceCatalogRepository + InMemoryServiceCatalogRepository (Mongo)
│   ├── Models/External|Catalog|Api/  מה שמגיע מבחוץ, מסמך הקטלוג, מה שחוזר ללקוח
│   └── Auth/                         JwtOptions + נקודת הנפקת טוקן לפיתוח בלבד
├── tests/MuniAnnouncements.Api.Tests/  בדיקות יחידה ללוגיקת הנרמול (xUnit)
└── client/my-requests-app/           הרכיב "הבקשות שלי" (Angular)
```

## איך זה עובד

```
Angular ──Bearer token──▶ Controller ──▶ Service ──▶ Client (בקשות)
                              │                 ├──▶ Repository (קטלוג)
                              │                 └──▶ RequestNormalizer
                              ◀── JSON מנורמל, בלי ת"ז ──┘
```

1. התושב מזוהה לפי ה-claim בשם `idNumber` בטוקן. הת"ז אף פעם לא מגיעה ב-URL.
2. ה-Service מביא ממערכת הטפסים את כל הבקשות של הת"ז, ואת הגדרות הקטלוג לפי קודי הטפסים.
3. ה-`RequestNormalizer` מחיל את הכללים ומחזיר רשימה מנורמלת.

כל שכבה מכירה רק את זו שמתחתיה, ושתי נקודות המגע עם העולם החיצוני הן interfaces. כדי לעבור לשירותים אמיתיים מחליפים שני רישומים ב-`Program.cs`.

### כללי הנרמול

| כלל | התנהגות |
|---|---|
| שמות | קוד הטופס מוחלף בשם השירות, וקוד הרשות בשם הרשות |
| סטטוס לא מוכר | מוצג "בטיפול" ונכתבת אזהרה ללוג |
| `actionRequired` | הסטטוס ברשימת הסטטוסים שדורשים פעולה |
| `isOverdue` | הבקשה פתוחה, ועברו **יותר** מ-`slaDays` ימים מההגשה |
| טופס שלא בקטלוג | הבקשה לא מוחזרת ונכתבת אזהרה ללוג |
| מיון | פעולה נדרשת קודם, ואז עדכון אחרון מהחדש לישן |
| פרטיות | מבנה התשובה לא כולל שדה ת"ז |

### החלטות שלא הוגדרו בדרישות

- **רשות שלא מוגדרת לטופס:** הבקשה מדולגת ונכתבת ללוג, כמו טופס שלא בקטלוג. בלי הגדרות הרשות אין שם, סטטוסים או SLA להציג.
- **גבול ה-SLA:** בקשה שעברו עליה בדיוק `slaDays` ימים עדיין לא חריגה.
- **סטטוס לא מוכר נחשב פתוח,** ולכן יכול להיות חריג.
- **סימון "נקרא" בחלונית:** נשמר בדפדפן בלבד, לפי מזהה בקשה ותאריך עדכון. כשהבקשה מתעדכנת הסימון מתאפס. הוא לא נשמר בשרת.

## הרצה

דרישות: .NET SDK 8, Node 24.0 ומעלה.

### שרת

```bash
dotnet run --project src/MuniAnnouncements.Api --launch-profile https
```

השרת עולה ב-`https://localhost:7210`, ו-Swagger ב-`/swagger`. תעודת הפיתוח של .NET עלולה לא להיות מהימנה בדפדפן. אפשר לאשר אותה, או להריץ פעם אחת `dotnet dev-certs https --trust`.

### לקוח

```bash
cd client/my-requests-app
npm install
npm start
```

האפליקציה עולה ב-`http://localhost:4200`, וקריאות `/api` מועברות לשרת דרך `proxy.conf.json`, ולכן אין צורך ב-CORS בפיתוח.

### כניסה לבדיקה

באזור האישי האמיתי הטוקן כבר קיים. כדי לדמות אותו בפיתוח:

- **בלקוח:** בפס "ת"ז לבדיקה" מקלידים ת"ז ולוחצים "התחברות". הפס מוצג רק בפיתוח.
- **ב-Swagger:** קוראים ל-`POST /api/dev/token?idNumber=...`, מעתיקים את הטוקן ולוחצים Authorize.

| ת"ז | מה רואים |
|---|---|
| `012345678` | שש בקשות, כולל פעולה נדרשת וחריגות |
| `987654321` | בקשה אחת |
| כל ת"ז אחרת | "עדיין לא הגשת בקשות" |

כדי לראות את מצב השגיאה, לוחצים "ניתוק" (השרת עונה 401 והרכיב מציג הודעה עם "נסה שוב"), או עוצרים את השרת.

## בדיקות

```bash
dotnet test
```

15 בדיקות יחידה ללוגיקת הנרמול, כולל סטטוס לא מוכר, חריגה מ-SLA (וגבולה), טופס ורשות חסרים, מיון ופרטיות הת"ז.

```bash
cd client/my-requests-app
npm test -- --watch=false
```

## מה היה משתנה בפרודקשן

| היום | בפרודקשן |
|---|---|
| `FakeFormsSystemClient` | `HttpFormsSystemClient` מבוסס `HttpClient`, אותו interface |
| `InMemoryServiceCatalogRepository` | מימוש מבוסס `IMongoCollection<ServiceCatalogDocument>`. המודל כבר כולל attributes של Mongo |
| טוקן מנקודת הנפקה בפיתוח | טוקן של ה-Identity Provider של האזור האישי, עם Issuer, Audience ומפתח שלו |
| מפתח חתימה ב-`appsettings.json` | מפתח מניהול סודות, לא מקובץ |
| הכתובות המותרות ל-CORS: `localhost:4200` | הכתובת האמיתית של האזור האישי |
| `localStorage` לטוקן ולסימון "נקרא" | הטוקן מהאזור האישי, והסימון בשרת אם נדרש |

## הערות על הסביבה

- **גרסת Angular:** Angular 21, כי גרסה 22 דורשת Node 24.15 ומעלה.
- **NuGet ו-npm:** `nuget.config` ו-`client/my-requests-app/.npmrc` מכוונים לרשומים הציבוריים, כדי שהפרויקט לא יהיה תלוי בהגדרות גלובליות של מחשב מסוים, וכדי שכתובות פנימיות לא ייכנסו ל-`package-lock.json`.
- **FluentAssertions:** גרסה 8 ומעלה היא ברישיון מסחרי של Xceed, וחינמית לשימוש לא מסחרי בלבד.
