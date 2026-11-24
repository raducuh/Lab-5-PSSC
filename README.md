# Lucrarea 5: Interacțiunea cu baza de date (EF Core 10, SQL-first)

```
Lucrarea-05/
├── SQL/
│   ├── create-db.sql            Sarcina 1: baza Student (exemplul profesorului)
│   └── create-magazin-db.sql    Sarcina 2: baza Magazin (schema din Lucrarea 4 + coloana Products.Stock)
├── Exemple/                     Sarcina 1: exemplul profesorului (Examples.sln / Examples.slnx)
├── Magazin/                     Sarcina 2: Magazin.sln / Magazin.slnx
│   ├── Magazin.Functional/      Result, Option (neschimbat)
│   ├── Magazin.Domain/          domeniul din Lucrarea 3, extins cu porturi + verificarea clientului
│   ├── Magazin.Data/            contextul din Lucrarea 4 + repository-uri (adaptoare) + model de citire
│   └── Magazin.ConsoleApp/      meniul, care apelează workflow-ul
└── global.json
```

Cerințe: .NET SDK 10 și SQL Server (instanța implicită `.`, autentificare Windows).

## Rulare

1. Creați bazele de date, dintr-un terminal în folderul `Lucrarea-05` (sau executați fișierele `.sql` în SSMS / fereastra Database din Rider):
   ```
   sqlcmd -S . -E -C -i SQL\create-db.sql
   sqlcmd -S . -E -C -i SQL\create-magazin-db.sql
   ```
   Scripturile sunt idempotente. `create-magazin-db.sql` poate fi rulat și peste baza `Magazin` din Lucrarea 4: adaugă doar coloana `Stock`.
2. Rulați:
   ```
   dotnet run --project Exemple/Examples.ConsoleApp      # Sarcina 1
   dotnet run --project Magazin/Magazin.ConsoleApp       # Sarcina 2
   ```
   Pentru o altă instanță (de exemplu `.\SQLEXPRESS`) setați connection string-ul o singură dată:
   ```
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.\SQLEXPRESS;Database=Magazin;Trusted_Connection=True;TrustServerCertificate=true" --project Magazin/Magazin.ConsoleApp
   ```
3. În Rider: deschideți `Magazin/Magazin.sln` (sau `Exemple/Examples.sln`) și rulați proiectul `...ConsoleApp`.

În meniul Sarcinii 2: `1` plasează o comandă prin workflow, `2`/`3`/`4` afișează produsele (cu stoc), clienții și comenzile.
Date de test: clienți `CL0001`, `CL0002`; produse `PR0001` (stoc 10), `PR0002` (5), `PR0003` (2), `PR0004` (0); orașe de livrare: Satu Mare, Cluj-Napoca, București, Oradea.

---

## Sarcina 1: elementele noi din exemplu (față de Lucrările 3 și 4)

1. **Proiect nou, `Examples.Data`, care depinde de `Examples.Domain` (nu invers).** Domeniul nu știe nimic despre EF Core; **`Examples.Domain` este identic cu cel din Lucrarea 3**.
2. **Porturi și adaptoare.** Domeniul își definește nevoile ca interfețe (`IStudentsRepository`, `IGradesRepository`, în `Domain/Repositories/`); `Examples.Data` le implementează (`StudentsRepository`, `GradesRepository`). Domeniul cere „ce are nevoie”, infrastructura decide „cum”.
3. **Workflow în structura „impur → pur → impur”** (`PublishExamWorkflow.ExecuteAsync`): citește din bază (`GetExistingAsync`) → logică pură (`Publish`) → salvează doar pe succes (`TapAsync(SaveAsync)`) → `Map(ToEvent)`. Nucleul pur rămâne testabil fără bază de date.
4. **Entități EF separate de stările domeniului** (`StudentEntity`, `GradeEntity`). Detaliile de persistență (`GradeId`, `IsUpdated`) au ieșit din domeniu; `Option<Grade>` se convertește în `decimal?` la graniță.
5. **`OnModelCreating` oglindește scriptul SQL** (nume de tabele, lungimi, indexuri unice, cheie străină 1-la-1, `HasPrecision(4, 2)`), fără migrații. Precizia lipsă făcea ca nota 7,88 să se salveze 8.
6. **Upsert după numărul matricol**, cu o singură interogare urmărită (`Include`) și un singur `SaveChangesAsync`.
7. **`StudentsRepository`:** `AsNoTracking` + `Contains` (tradus în `IN (...)`). Rândurile propriei baze sunt de încredere: un matricol invalid acolo este corupție (`GetValueOrThrow` → `InvalidDataException`), nu eroare de validare.
8. **Model de citire separat** (`IGradesQuery`, `StudentGradeRow`): se proiectează direct din EF, fără domeniu, pentru că bazele de date pot avea valori `NULL` pe care domeniul nu le reprezintă.
9. **Injecție de dependențe:** `AddGradesData(connectionString)` înregistrează contextul și repository-urile ca *scoped*; un `AsyncServiceScope` per rulare; `TimeProvider.System` pentru ceas.
10. **Configurare externă:** `ConnectionStrings:DefaultConnection` din `appsettings.json` sau `dotnet user-secrets`; gazdă generică (`Host.CreateApplicationBuilder`).
11. **Script SQL idempotent** (`IF DB_ID(...) IS NULL`, `IF OBJECT_ID(...) IS NULL`, date de start cu `MERGE`) și mediu Docker opțional.
12. **Excepțiile de infrastructură nu se prind în domeniu**: se propagă la margine (consolă, API).

---

## Sarcina 2: workflow-ul de plasare a comenzii cu baza de date

Flux (`PlaceOrderWorkflow.ExecuteAsync`):

```
1. încarcă din baza de date    clientul + produsele comandate cu stocul lor   (ICustomersRepository, IProductsRepository)
2. logică pură                 Validate → CheckCustomerExists → CheckProductsExist → CheckStock
                               → CheckDeliveryAddress → CalculatePrice → Place         (PlaceOrderWorkflow.Place)
3. salvează în baza de date    comanda + liniile + scăderea stocului, într-o tranzacție   (IOrdersRepository)
4. întoarce                    OrderPlacedEvent (cu Id-ul dat de baza de date) sau PlaceOrderError
```

Cum acoperă cerințele:
- **Încărcarea stării înainte de workflow:** `LoadStoreAsync` citește doar ce are nevoie comanda (un client, produsele cerute) și construiește un `StoreSnapshot`. Codurile cu format greșit nu se caută în bază (le raportează validarea).
- **Verificarea produsului și a stocului cu date din bază:** `CheckCustomerExists`, `CheckProductsExist`, `CheckStock` primesc dicționare citite din baza de date. Rămân funcții pure, deci se testează fără bază de date. Stocul se compară cu cantitatea totală per produs (liniile cu același produs se adună).
- **Salvarea rezultatului:** `OrdersRepository.SaveAsync` adaugă `Order` + `OrderItem` și scade `Products.Stock`, într-un singur `SaveChangesAsync`. Evenimentul se creează după salvare, ca să conțină Id-ul atribuit de baza de date.

Decizii de care trebuie să știți:
- **Coloana `Products.Stock`** nu exista în schema din Lucrarea 4 (`Products(Id, Code, Name, Price, QuantityType)`), dar fără ea stocul nu poate fi verificat din baza de date. Am adăugat-o în `SQL/create-magazin-db.sql` (cu `CHECK (Stock >= 0)`).
- **Concurență:** `Stock` este jeton de concurență în EF. Două comenzi simultane pentru ultimele bucăți nu pot scădea stocul de două ori: a doua primește `DbUpdateConcurrencyException`, afișat în consolă ca mesaj.
- **Orașele de livrare** nu sunt în baza de date (schema nu are tabel pentru ele): vin din `appsettings.json` (`Delivery:Cities`) și intră în workflow ca valoare (`DeliveryZone`).
- **Clientul** se identifică prin cod (`CL0001`) și trebuie să existe în `Customers`. Domeniul nu mai creează clienți.
- **Prețul** nu se salvează pe linia de comandă (schema `OrderItem` nu are coloană pentru el), deci totalurile afișate la „Afișează comenzile” folosesc prețul curent al produsului.
- **Adresa** se salvează ca text „stradă, oraș, cod poștal” în `Order.DeliveryAddress` (maximum 200 de caractere, verificat la validare).
- **Domeniu extins față de Lucrarea 3:** `CustomerCode`, starea `Order.CustomerVerified` și operația `CheckCustomerExists`, starea finală `Order.Placed`, porturile din `Domain/Repositories/`.
