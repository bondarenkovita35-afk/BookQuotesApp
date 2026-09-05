# BookQuotesApp

Ett fullstack CRUD-projekt med en personlig bokhylla ("Böcker") och personliga citat ("Mina citat"), byggt med Angular 20 och .NET 9. Varje användare loggar in med e-post och lösenord och ser bara sina egna böcker och citat.

## Innehåll

- [Funktioner](#funktioner)
- [Teknik](#teknik)
- [Arkitektur](#arkitektur)
- [Datamodell](#datamodell)
- [Kom igång lokalt](#kom-igång-lokalt)
- [Miljövariabler och konfiguration](#miljövariabler-och-konfiguration)
- [Databas och migrationer](#databas-och-migrationer)
- [Köra tester](#köra-tester)
- [Produktionsbygge](#produktionsbygge)
- [Driftsättning](#driftsättning)
- [API och Swagger](#api-och-swagger)
- [Säkerhet](#säkerhet)
- [Fattade beslut](#fattade-beslut)
- [Kända begränsningar](#kända-begränsningar)
- [Möjliga förbättringar](#möjliga-förbättringar)

## Funktioner

**Konto**
- Registrering och inloggning med e-post och lösenord (lösenord lagras endast som hash).
- JWT-baserad autentisering; alla skyddade sidor och API-endpoints kräver giltig inloggning.
- Automatisk utloggning med tydligt meddelande när sessionen har gått ut.

**Böcker**
- Lista, lägg till, redigera och ta bort egna böcker (titel, författare, utgivningsdatum).
- Validering i både formulär och API.

**Mina citat**
- Samma CRUD-mönster som böcker, men citat är alltid personliga.
- Fem startcitat skapas automatiskt en gång per nytt konto, utan risk för dubbletter.

**Gränssnitt**
- Responsiv layout för mobil, surfplatta och desktop, med hopfällbar mobilmeny.
- Ljust och mörkt tema med sparat val och systemtemat som standard.
- Tydliga laddnings-, tom- och felstatusar samt bekräftelse innan borttagning.

## Teknik

| Del | Val |
|---|---|
| Frontend | Angular 20 (standalone-komponenter, signals, ny kontrollflödessyntax), TypeScript (strict), Bootstrap 5, Font Awesome |
| Backend | .NET 9, ASP.NET Core Web API, Entity Framework Core 9 |
| Autentisering | JWT (Bearer-token), lösenordshashning med `Microsoft.AspNetCore.Identity.PasswordHasher` |
| Databas | SQL Server (lokalt: LocalDB, produktion: Azure SQL Database) |
| Test | xUnit + Moq + SQLite in-memory (backend), Jasmine/Karma (frontend) |

## Arkitektur

```
Angular 20 SPA  --HTTPS/JSON, Authorization: Bearer-->  ASP.NET Core Web API  --EF Core-->  SQL Server
```

Frontend och backend är två separata projekt i samma repo (`/frontend` och `/backend`) som kommunicerar enbart via REST-API:et. Backend är helt stateless — hela sessionen bärs av JWT-token som skickas i `Authorization`-headern på varje anrop.

**Backend-struktur** (`backend/BookQuotesApp.Api`):

```
Controllers/    Tunna kontroller: tar emot förfrågan, anropar en service, returnerar resultat
Services/       Affärslogik (registrering/inloggning, CRUD, ägarskapskontroll)
Data/           EF Core DbContext och migrationer
Entities/       User, Book, Quote
Dtos/           Indata/utdata separat från entiteterna
Middleware/     Centraliserad felhantering
HealthChecks/   /health-endpoint för drift
```

Det finns medvetet inget separat repository-lager ovanpå EF Core (DbContext fyller redan den rollen) och ingen AutoMapper — med tre entiteter är manuell mappning tydligare än att lägga till ett till beroende.

**Frontend-struktur** (`frontend/src/app`):

```
core/       Tjänster, guards och interceptors som används i hela appen (auth, tema)
shared/     Återanvändbara komponenter (navigering, bekräftelsedialog)
features/   En mapp per funktionsområde (auth, books, quotes), lazy-loaded var för sig
```

## Datamodell

**User** — `Id, Email (unikt), PasswordHash, CreatedAt`
**Book** — `Id, Title, Author, PublishedDate, UserId, CreatedAt`
**Quote** — `Id, Text, Author (valfritt), UserId, CreatedAt`

Både `Book` och `Quote` har en obligatorisk relation till `User`. Varje endpoint för böcker och citat kontrollerar att den inloggade användarens `UserId` matchar radens `UserId` innan den lämnar ut, ändrar eller tar bort något — även om anroparen gissar rätt `id` på en annan användares rad.

## Kom igång lokalt

**Förutsättningar:** .NET 9 SDK, Node.js 22 (matchar Angular 20:s krav ^20.19 / ^22.12), SQL Server LocalDB (ingår i Visual Studio, eller installeras separat).

```bash
# Backend
cd backend/BookQuotesApp.Api
dotnet user-secrets set "Jwt:SigningKey" "<en-lång-slumpad-sträng>"
dotnet ef database update
dotnet run
```

API:et startar på `https://localhost:7156` och Swagger finns på `https://localhost:7156/swagger`.

```bash
# Frontend, i ett nytt terminalfönster
cd frontend
npm install
npm start
```

Frontend startar på `http://localhost:4200` och pekar mot `https://localhost:7156/api` i utvecklingsläge (se `src/environments/environment.development.ts`).

## Miljövariabler och konfiguration

Inga hemligheter ligger i repot. Följande måste sättas separat:

| Nyckel | Var | Beskrivning |
|---|---|---|
| `Jwt:SigningKey` | `dotnet user-secrets` lokalt, appinställning i Azure i produktion | Nyckel som signerar JWT-token. Minst 32 tecken slumpad text. |
| `ConnectionStrings:DefaultConnection` | `appsettings.Development.json` lokalt (redan ifylld med LocalDB, inget lösenord), appinställning i Azure i produktion | Databasanslutning. |
| `Cors:AllowedOrigins` | `appsettings.Development.json` lokalt, appinställning i Azure i produktion | Vilka frontend-adresser som får anropa API:et. |

`Jwt:Issuer`, `Jwt:Audience` och `Jwt:ExpiryMinutes` har icke-hemliga standardvärden i `appsettings.json`.

## Databas och migrationer

```bash
cd backend/BookQuotesApp.Api
dotnet ef migrations add <Namn>   # skapa en ny migration efter en modelländring
dotnet ef database update         # applicera migrationer mot databasen
```

## Köra tester

```bash
# Backend (23 tester: autentisering, CRUD, ägarskapskontroll, validering)
cd backend
dotnet test

# Frontend (Jasmine/Karma). Om Chrome inte är installerat, sätt CHROME_BIN
# till t.ex. Edge: "C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"
cd frontend
npm test -- --watch=false
```

## Produktionsbygge

```bash
cd backend/BookQuotesApp.Api && dotnet build -c Release
cd frontend && npm run build
```

Frontendens produktionsfiler hamnar i `frontend/dist/frontend/browser`.

## Driftsättning

**Planerad lösning:**

- **Frontend:** Vercel, automatisk driftsättning från GitHub.
- **Backend:** Azure App Service (Linux), prisnivå F1 (kostnadsfri).
- **Databas:** Azure SQL Database, kostnadsfri nivå (100 000 vCore-sekunder och 32 GB lagring per månad, pausas automatiskt vid gränsen).

Inga molnresurser är skapade ännu i det här skedet av projektet — resurser, region och kostnad gås igenom och godkänns separat innan något skapas.

## API och Swagger

I utvecklingsläge finns interaktiv API-dokumentation på `/swagger`. Alla endpoints under `/api/books` och `/api/quotes` kräver `Authorization: Bearer <token>`, hämtat från `/api/auth/login` eller `/api/auth/register`.

Hälsokontroll för drift: `GET /health`.

## Säkerhet

- **Lösenord:** hashas med `PasswordHasher<User>`, lagras aldrig i klartext.
- **JWT:** kort livslängd (2 timmar), skickas som Bearer-token i headern — inte i en cookie. Vercel och Azure ligger på olika domäner, och en cookie som fungerar mellan dem hade behövt `SameSite=None`, vilket blockeras av flera webbläsares tredjepartsskydd. Token lagras i `sessionStorage` via en egen tjänst (inte utspritt i komponenter), rensas vid utloggning och vid ett 401-svar. Detta skyddar inte mot XSS på samma sätt som en `HttpOnly`-cookie hade gjort — det är en medveten avvägning för pålitlig funktion mellan domänerna, inte ett påstående om att `sessionStorage` är helt säkert.
- **Ägarskap:** varje bok och citat kontrolleras mot den inloggade användarens id på varje läsning, ändring och borttagning. Både "finns inte" och "tillhör någon annan" ger samma 404-svar, för att inte avslöja att ett id existerar.
- **CORS:** enbart uttryckligen tillåtna adresser, ingen wildcard.
- **Loggning:** inga lösenord, tokens eller anslutningssträngar skrivs till loggar.

## Fattade beslut

- **Böcker är personliga**, precis som citat — inte ett gemensamt bibliotek för alla användare.
- **JWT i `sessionStorage`**, inte en cookie — se [Säkerhet](#säkerhet).
- **Angular 20 och .NET 9** används exakt som specificerat, trots att båda närmar sig slutet av sin officiella supportperiod under hösten 2026 — versionerna var ett uttryckligt krav.
- **Ingen refresh-token.** Token varar 2 timmar; efter det loggas användaren ut och loggar in igen. Ett refresh-flöde hade gett längre sessioner men är mer komplext än vad omfattningen av projektet motiverar.

## Kända begränsningar

- Ingen refresh-token — utgången session kräver ny inloggning.
- Manuellt testat i Microsoft Edge. Chrome och Firefox var inte installerade i utvecklingsmiljön vid byggtillfället och har därför inte kunnat testas manuellt (de automatiserade frontend-testerna körs dock via Edge, som liksom Chrome bygger på Chromium).
- `npm audit` visar två måttliga sårbarheter i Karmas testberoenden (`qs`/`body-parser`) — påverkar endast lokal testkörning, inte produktionsbygget.

## Möjliga förbättringar

- Refresh-token för längre sessioner utan att förlänga access-token-livslängden.
- Sökning, sortering och paginering för böcker och citat.
- Automatiserade end-to-end-tester (t.ex. Playwright) utöver dagens enhetstester och manuella test.
