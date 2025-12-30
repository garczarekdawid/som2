# Struktura projektu — kluczowe pliki i zależności (zaktualizowane)

Poniżej znajduje się zaktualizowane drzewo kluczowych plików oraz krótki opis zależności między nimi. Ścieżki są względne do katalogu repozytorium (`D:\programowanie\som2`). Plik ma służyć jako szybka referencja dla dewelopera i dla AI.

## Kluczowe pliki / katalogi
- `app/SOM2/SOM2/`
  - `Program.cs`
  - `Controllers/`
    - `ManagedHostController.cs`
    - `ManagedHostsListController.cs`
  - `Views/`
    - `Shared/`
      - `_Layout.cshtml`
      - `_Layout.cshtml.css`
    - `ManagedHost/`
      - `Index.cshtml`
      - `Edit.cshtml`
    - `ManagedHostsList/`
      - `Index.cshtml`
  - `Application/`
    - `Interfaces/`
      - `IManagedHostService.cs`
      - `IHostActionExecutor.cs`
    - `Services/`
      - `ManagedHostService.cs`
      - `HostActionExecutor.cs`
    - `DTO/`
      - `ManagedHostDto.cs`
  - `Domain/`
    - `Entities/`
      - `ManagedHost.cs`
      - `HostActionExecution.cs`
    - `Enums/`
      - `HostStatus.cs`
      - `HostActionType.cs`
      - `HostActionStatus.cs`
    - `Interfaces/`
      - `IManagedHostRepository.cs`
      - `IHostActionRepository.cs`
  - `Infrastructure/`
    - `Persistence/`
      - `AppDbContext.cs`
    - `Repositories/`
      - `ManagedHostRepository.cs`
      - `HostActionRepository.cs`
    - `Workers/`
      - `HostActionWorker.cs`
    - `Migrations/` (pliki migracji EF)
- `SOM2.Models/`
  - `ManagedHost/`
    - `ManagedHostCreateViewModel.cs`
    - `ManagedHostEditViewModel.cs`
    - `ManagedHostIndexViewModel.cs`

## Zależności i przepływ odpowiedzialności (skrót)
- `Program.cs`
  - Rejestruje DI: mapuje interfejsy na implementacje (np. `IManagedHostService -> ManagedHostService`, `IManagedHostRepository -> ManagedHostRepository`, `IHostActionRepository -> HostActionRepository`, `IHostActionExecutor -> HostActionExecutor`).
  - Rejestruje hosted service: `HostActionWorker`.
  - Rejestruje `AppDbContext` (Npgsql) — punkt startowy dla dostępu do bazy.

- Kontrolery widoków
  - `ManagedHostController` i `ManagedHostsListController`
    - Zależność: przyjmują w konstruktorze `IManagedHostService`.
    - Odpowiedzialność: obsługa żądań HTTP, walidacja wejścia (ModelState), konwersje między ViewModelami (`SOM2.Models`) a DTO (`SOM2.Application.DTO`), zwracanie widoków (`Views/*`).
    - Przykład: `ManagedHostController.Create(ManagedHostCreateViewModel vm)` → tworzy `ManagedHostCreateDto` → `_service.AddAsync(dto)`.

- Warstwa aplikacji
  - `IManagedHostService` / `ManagedHostService`
    - Zależność: używa `IManagedHostRepository` (iniekcja w service).
    - Odpowiedzialność: logika aplikacyjna, mapowanie encji ↔ DTO, agregacja wyników (paginacja itp.).
    - Eksponuje metody używane przez kontrolery (GetAllAsync, GetPagedAsync, AddAsync, UpdateAsync, DeleteAsync, GetByIdAsync, GetByIdsAsync).

- Warstwa domenowa i repozytoria
  - `ManagedHost` (encja) — reguły domenowe (metody `SetXxx` itp.).
  - `IManagedHostRepository` / `ManagedHostRepository`
    - Zależność: `ManagedHostRepository` używa `AppDbContext` (EF Core).
    - Odpowiedzialność: dostęp do danych (CRUD, GetPagedAsync). Zwraca encje domenowe do `ManagedHostService`.

- DbContext i migracje
  - `AppDbContext` definiuje `DbSet<ManagedHost>` i inne encje.
  - Migracje w `Infrastructure/Migrations` odpowiadają zmianom encji i konfiguracji DB.

- Host action (worker)
  - `HostActionWorker` (hosted service)
    - Zależności: `IHostActionRepository`, `IHostActionExecutor` (rejestracja w `Program.cs`).
    - Odpowiedzialność: background processing zadań związanych z wykonywaniem akcji na hostach (np. włącz/wyłącz/restart), odczyt/aktualizacja encji `HostActionExecution`.
  - `HostActionExecutor`
    - Realizuje faktyczne wywołanie akcji na hoście (może używać `IManagedHostRepository` do pobrania hosta i `IHostActionRepository` do zapisu statusu).

- DTO ↔ ViewModel ↔ Views
  - `Application.DTO` (np. `ManagedHostDto`, `ManagedHostCreateDto`, `ManagedHostUpdateDto`) — format przesyłany między warstwami serwisu a kontrolerem.
  - `SOM2.Models.*ViewModel` — walidacja i model widoku używany bezpośrednio przez widoki Razor (`Views/*`).
  - Kontroler robi mapping:
    - ViewModel (formularz) → DTO → Service → Repository → Entity.
    - Entity/DTO → ViewModel → View.

- Widoki (`Views/*`)
  - `Views/ManagedHost/Index.cshtml` i `Views/ManagedHostsList/Index.cshtml`
    - Oczekują konkretnego modelu (ViewModel lub `List<ManagedHostDto>`).
    - Wywołują akcje kontrolera (`asp-action`) — formularze POST/GET.

## Krótka mapa przepływu dla typowej operacji "Dodaj hosta"
1. Użytkownik otwiera stronę `GET /ManagedHost/Create` → `ManagedHostController.Create()` → zwraca widok z `ManagedHostCreateViewModel`.
2. Formularz POST -> `ManagedHostController.Create(ManagedHostCreateViewModel vm)`:
   - walidacja ModelState,
   - mapowanie `ManagedHostCreateViewModel` → `ManagedHostCreateDto`,
   - `_service.AddAsync(dto)` (warstwa aplikacji).
3. `ManagedHostService.AddAsync`:
   - konwersja DTO → encja (`ManagedHost`),
   - `_repo.AddAsync(host)` (repozytorium zapisuje przez `AppDbContext`),
   - migracje / baza persistent.

## Uwagi praktyczne / wskazówki
- Jeśli chcesz przejść na Razor Pages: zamień kontrolery na `Pages/ManagedHost/*.cshtml.cs` PageModel korzystające z `IManagedHostService`; widoki przenieś do `Pages/ManagedHost`.
- Dla spójności: kontrolery powinny używać DTO/Service, a widoki ViewModeli. Nigdy nie odwołuj się bezpośrednio do `AppDbContext` w kontrolerach.
- Worker i executor powinny aktualizować status akcji w oddzielnej encji (`HostActionExecution`), nie modyfikować bezpośrednio stanu `ManagedHost` bez transakcji/rekordu historii.
- Dodaj komentarz w dokumentacji przy każdej zmianie migracji/encji, aby utrzymać spójną dokumentację.