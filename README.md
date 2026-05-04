# CurrencyApp

Desktop application built with WPF (.NET 10) that displays currency exchange rates from the Central Bank of Russia (https://www.cbr-xml-daily.ru/daily_json.js). 
Implemented using MVVM. Supports offline use through local storage with a choice of JSON, SQLite, or both backends.

## Features

- Fetch currency rates from the API on demand and display them in a sortable list with formatted numeric columns.
- Add custom currencies through a validated form. User-added entries are visually distinguished and persist across refreshes — refreshing the API does not overwrite them.
- Delete any currency, whether fetched or user-added. Deleted API entries return on the next refresh; deleted user entries stay deleted.
- Switch storage backend between JSON, SQLite, or Both at runtime through a Settings page. Each mode keeps its own dataset.
- Track and display the timestamp of the previous session.
- Navigation between three pages (Currencies, Add Currency, Settings) through a sidebar.

## Architecture

The project is organized into four folders matching the standard MVVM layout: `Models`, `ViewModels`, `Views`, and `Services`. 
Code-behind files are kept minimal — the only logic in `MainWindow.xaml.cs` is navigation wiring; all feature pages are pure MVVM with no code-behind.

ViewModels expose state through `[ObservableProperty]` and behavior through `[RelayCommand]`, both from `CommunityToolkit.Mvvm`. 
The toolkit's source generators produce the `INotifyPropertyChanged` boilerplate at compile time, keeping ViewModels readable. Collections bound to the UI use `ObservableCollection<T>` so list-level changes propagate to bindings automatically.

Services are accessed through interfaces. `IStorageService` has three implementations: `JsonStorageService`, `SqliteStorageService`, and `CompositeStorageService`. 
The composite wraps the other two and routes reads and writes based on the active storage mode — this is the Composite pattern. ViewModels depend only on the interface and have no knowledge of which backend is active.

Navigation is handled by a small `NavigationService` that owns the WPF `Frame` and exposes a `NavigateTo(string)` method. 
ViewModels do not touch UI elements directly. A static `ServiceLocator` holds a singleton reference to the navigation service so the shell's click handlers can reach it.

Settings (storage mode and last session timestamp) are persisted as JSON to `%LOCALAPPDATA%\CurrencyApp\settings.json` through a dedicated `AppSettingsService`.

## Trade-offs and decisions

**WPF over UWP.** The task expressed a preference for UWP. I chose WPF given my lack of prior XAML experience — it has lower setup overhead and the MVVM concepts transfer directly to UWP / WinUI 3 / MAUI.

**`Environment.SpecialFolder.LocalApplicationData` instead of `ApplicationData.Current.LocalFolder`.** The task referenced the UWP API, which in WPF requires a Windows Application Packaging Project.
The native WPF approach resolves to the same per-user app-data location without dated tooling.

**Static service locator instead of a DI container.** A 3-page application doesn't justify the overhead of `Microsoft.Extensions.DependencyInjection`. 
In a larger codebase I would inject services through constructors and let a container manage lifetimes.

**ViewModels instantiated in XAML via `Page.DataContext`.** Concise for this scope. Production code would inject ViewModels through DI to make them testable and to control their lifetime.

**Decimal values stored as TEXT in SQLite.** SQLite has no native decimal type. Using REAL (float) introduces rounding errors unacceptable for monetary data. Storing as TEXT in `InvariantCulture` preserves exact values.

**Replace-all on save (delete-then-insert) wrapped in a transaction.** Simpler than diff-and-update. Acceptable at the scale of ~50–100 currencies. A row-level update strategy would be needed at higher volumes.

**Storage modes are independent — no auto-migration on switch.** Each backend keeps its own dataset. This makes each backend independently demonstrable and avoids subtle migration bugs.
The active mode is shown in the Currencies page header so the user always knows which dataset they are viewing.

**Both mode unifies divergent stores at load time.** When both backends contain the same currency code with different values, the SQLite copy wins. 
This makes Both mode self-healing if the stores diverge while the user was in single-backend mode. A production system might use last-modified timestamps or surface conflicts to the user.

**Manual string parsing for numeric input on the Add Currency form.** Bypasses WPF's culture-aware binding to accept both `,` and `.` as decimal separator predictably, regardless of system locale.

**A single `Currency` model used for both API deserialization and storage.** Simpler than maintaining separate API DTOs and persistence DTOs. 
The `IsUserAdded` flag is absent from the API response and defaults to `false` for fetched entries — exactly what we want.

## Running the project

Requirements: Visual Studio 2022 with the .NET 10 SDK and the WPF workload installed.

Open `CurrencyApp.sln` and press F5. No external configuration is needed. Internet access is required for the initial Refresh; subsequent launches load from local storage.
After testing on different machine: if the very first build fails on the nuget package download, close and reopen the solution, then rebuild. Works fine after that.

Application data is stored at `%LOCALAPPDATA%\CurrencyApp\`:
- `currencies.json` — JSON-mode data
- `currencies.db` — SQLite-mode data
- `settings.json` — last session timestamp and active storage mode

## Known limitations

- No automated tests. ViewModels and storage services are structured to be testable (interface-based dependencies) but I did not write tests within the time budget.
- No retry or backoff on transient API failures. A single failed refresh shows an error message; the user must click again.
- Both-mode conflict resolution is deterministic (SQLite wins) but not user-visible. A real product would surface conflicts.

## Development notes

I built this project with no prior WPF or MVVM experience. I used AI assistance (Claude) as a teaching tool throughout: explaining concepts (data binding, `INotifyPropertyChanged`, async/await, dependency inversion, the Composite pattern etc.), 
reviewing my code as I wrote it, helping me reason through architectural trade-offs, and helping me write this README. 
All design decisions documented above are ones I deliberated on and chose; the code was written, debugged, and tested by me.