# HajjSystem — .NET 8 Rewrite

Clean-Architecture rebuild of AlhajjApp — **no ITS.Core / URF.Core dependency**.

## Solution structure

```
HajjSystem/
├── Domain/               ← Entities, Enums, Constants (no dependencies)
├── Application/          ← Interfaces, Services, DTOs  (depends on Domain only)
├── Infrastructure/       ← EF Core, Repositories, UnitOfWork, HRMS HTTP client
└── Web/                  ← ASP.NET Core MVC, Controllers, Program.cs
```

## Key design decisions

| Old pattern | New pattern |
|---|---|
| `ITS.Core.Services.Service<T>` base class | Plain `IRepository<T>` interface + `Repository<T>` EF implementation |
| `IUnitOfWork` from Core library | Own `IUnitOfWork` → `UnitOfWork` wrapping `AppDbContext.SaveChangesAsync` |
| `ITrackableRepository` + change-tracking entities | Standard EF change tracking (`Entry(e).State = Modified`) |
| `IService<T>` with `.Queryable()` | Dedicated service classes per domain concept |
| Business logic in Controllers | Business logic in Application-layer Service classes |
| `Result` returned from services | `Result<T>` discriminated union — no exceptions for expected failures |
| `IOptions<HajjSettings>` everywhere | `IHajjSettingsAccessor` interface allows season rollover to update the year at runtime |

## Business rules preserved

- **Permanent Hajj ban**: NIC with `ConfirmCode=77` in any year blocks re-registration forever
- **Quota enforcement inside DB transaction**: race-condition safe
- **Two-flight mandatory return linkage**: dep-F1 → ret-F2, dep-F2 → ret-F1
- **Admin pilgrims always assigned to departure F1**
- **Multi-tenancy via Global Query Filters**: unit officers see only their data; SysAdmin sees all
- **LDAP authentication** against Active Directory (with local admin bypass for emergencies)
- **Cookie-based auth**: permissions stored as Claims at login

## Getting started

```bash
# 1. Update connection string in Web/appsettings.json
# 2. Run migrations from the solution root
dotnet ef migrations add Initial --project Infrastructure --startup-project Web
dotnet ef database update --project Infrastructure --startup-project Web
# 3. Run
dotnet run --project Web
```
