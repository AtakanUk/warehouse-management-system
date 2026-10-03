# Contributing

Thanks for your interest in improving the Warehouse Management System.

## Development setup

See [Getting started](README.md#getting-started). In short: open `Warehouse.sln` in Visual Studio
2022 and press F5. The database is created on SQL Server LocalDB on first start.

## Where things go

- **Business rules** belong in the domain model (`src/Warehouse.Core/Domain`). Raise a
  `BusinessRuleException` with a message that can be shown to users.
- **Use cases** belong in a service (`src/Warehouse.Core/Services`). A service method loads what
  it needs, calls the domain and finishes with `Commit()`.
- **Pages** stay thin. They read input, call a service inside `TryRun(...)` and redirect with
  `RedirectWithMessage(...)` after a successful POST.
- **Schema changes** go into `database/schema.sql`, mirrored in `WarehouseDbContext`. If the
  change affects existing data, also update `database/seed.sql`.
- **Styles** go into `src/Warehouse.Web/Content/site.css`; avoid inline style blocks.

## Before opening a pull request

1. Build with markup compilation, then run the tests:

   ```powershell
   msbuild Warehouse.sln -restore -p:Configuration=Release -p:PrecompileWebForms=true
   dotnet test tests\Warehouse.Core.Tests -c Release --no-build
   ```

2. Add or update tests for behaviour you changed. Domain rules get unit tests, and anything that
   touches the database gets an integration test.
3. Keep the build free of warnings (they are treated as errors).
4. Describe the user-facing change and attach screenshots for UI changes.

Never commit credentials, production connection strings, database files or customer data.
