# Event-Driven Microservices Demo

En demonstration av hur enkelt det är att migrera från **Modular Monolith** till **Microservices** genom event-driven arkitektur med .NET 10.

## 🏛️ Arkitektoniska Mönster: Monolith → Modular Monolith → Microservices

### 1️⃣ Traditional Monolith
**En kodbas, en process, en databas**

```
┌─────────────────────────────────────┐
│         Monolithic Application      │
│  ┌──────────┐  ┌──────────┐         │
│  │Customers │  │Employees │         │
│  │  Logic   │  │  Logic   │         │
│  └─────┬────┘  └─────┬────┘         │
│        └───────┬─────┘              │
│                │                    │
│         ┌──────▼──────┐             │
│         │   Single    │             │
│         │  Database   │             │
│         └─────────────┘             │
└─────────────────────────────────────┘
```

**Egenskaper:**
- ✅ Enkel att utveckla initialt
- ✅ Enkel deployment (en artefakt)
- ✅ Enkel att testa (allt i samma process)
- ❌ Tight coupling mellan moduler
- ❌ Svår att skala (måste skala hela applikationen)
- ❌ Lång deployment-cykel
- ❌ En bugg kan krascha hela systemet
- ❌ Svår att adoptera nya teknologier

---

### 2️⃣ Modular Monolith
**En kodbas, en process, separata databaser per modul**

```
┌─────────────────────────────────────────────────────────┐
│            Modular Monolithic Application               │
│                                                         │
│  ┌───────────────┐         ┌───────────────┐            │
│  │CustomersModule│         │EmployeesModule│            │
│  │               │         │               │            │
│  │   ┌─────┐     │  Event  │   ┌─────┐     │            │
│  │   │Logic│     │  ─────► │   │Logic│     │            │
│  │   └──┬──┘     │  (In-   │   └──┬──┘     │            │
│  │      │        │ Process)│      │        │            │
│  │   ┌──▼──┐     │         │   ┌──▼──┐     │            │
│  │   │ DB  │     │         │   │ DB  │     │            │
│  │   └─────┘     │         │   └─────┘     │            │
│  └───────────────┘         └───────────────┘            │
│          │                        │                     │
│          └────────┬───────────────┘                     │
│                   ▼                                     │
│           WorkloadsModule                               │
│           ┌─────────────┐                               │
│           │   Logic     │                               │
│           │ (Subscriber)│                               │
│           │   ┌─────┐   │                               │
│           │   │ DB  │   │ ← Local copies                │
│           │   └─────┘   │                               │
│           └─────────────┘                               │
└─────────────────────────────────────────────────────────┘
```

**Egenskaper:**
- ✅ Tydlig separation mellan moduler
- ✅ **Separata databaser** - Database per module
- ✅ Event-driven kommunikation (även in-process)
- ✅ Enklare att testa moduler isolerat
- ✅ Enkel deployment (fortfarande en artefakt)
- ✅ **Eventual consistency** mellan moduler
- ✅ Enkel att migrera till microservices
- ⚠️ Fortfarande en deployment-enhet
- ⚠️ Kan inte skala moduler oberoende

---

### 3️⃣ Microservices
**Separata kodbaser, separata processer, separata databaser**

```
┌──────────────┐         ┌──────────────┐         ┌──────────────┐
│CustomersApi  │         │EmployeesApi  │         │WorkloadsApi  │
│              │         │              │         │              │
│  ┌─────┐     │  Event  │  ┌─────┐     │  Event  │  ┌─────┐     │
│  │Logic│     │  ─────► │  │Logic│     │  ─────► │  │Logic│     │
│  └──┬──┘     │ (NATS)  │  └──┬──┘     │ (NATS)  │  └──┬──┘     │
│     │        │         │     │        │         │     │        │
│  ┌──▼──┐     │         │  ┌──▼──┐     │         │  ┌──▼──┐     │
│  │ DB  │     │         │  │ DB  │     │         │  │ DB  │     │
│  └─────┘     │         │  └─────┘     │         │  └─────┘     │
│              │         │              │         │  (Local      │
│              │         │              │         │   copies)    │
└──────────────┘         └──────────────┘         └──────────────┘
       │                        │                        ▲
       │                        │                        │
       └────────────────────────┴────────────────────────┘
                          NATS JetStream
```

**Egenskaper:**
- ✅ Fullständig autonomi per service
- ✅ **Separata databaser** - Database per service
- ✅ Oberoende deployment
- ✅ Oberoende scaling
- ✅ Teknologival per service
- ✅ Team-autonomi
- ✅ **Eventual consistency** via events
- ❌ Mer komplex deployment
- ❌ Kräver orchestration (Kubernetes)
- ❌ Mer komplex debugging (distributed tracing behövs)
- ❌ Network latency

---

## 🎯 Database per Module/Service Pattern

**Kritisk princip:** Varje modul/service äger sin egen databas och delar ALDRIG tabeller.

### CustomersModule Database
```sql
CREATE TABLE Customers (
    Id CHAR(36) PRIMARY KEY,
    OrgNumber VARCHAR(20) NOT NULL,
    Name VARCHAR(200) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    PhoneNumber VARCHAR(20),
    Address VARCHAR(200),
    PostalCode VARCHAR(10),
    City VARCHAR(100),
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6) NOT NULL,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE
);
```

### EmployeesModule Database
```sql
CREATE TABLE Employees (
    Id CHAR(36) PRIMARY KEY,
    SSN VARCHAR(20) NOT NULL,
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    PhoneNumber VARCHAR(20),
    Address VARCHAR(200),
    PostalCode VARCHAR(10),
    City VARCHAR(100),
    Salary DECIMAL(18,2) NOT NULL,
    HireDate DATETIME(6) NOT NULL,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6) NOT NULL,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE
);
```

### WorkloadsModule Database - Local Copies
```sql
-- WorkloadsModule maintains LOCAL COPIES (not foreign keys!)
CREATE TABLE WorkloadCustomers (
    Id CHAR(36) PRIMARY KEY,  -- Same as CustomersModule.Customers.Id
    Name VARCHAR(200) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    PhoneNumber VARCHAR(20),
    -- NOTE: Fewer fields than source! Only what's needed for Workloads
);

CREATE TABLE WorkloadEmployees (
    Id CHAR(36) PRIMARY KEY,  -- Same as EmployeesModule.Employees.Id
    FirstName VARCHAR(100) NOT NULL,
    LastName VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    PhoneNumber VARCHAR(20),
    -- NOTE: SSN, Salary, HireDate NOT included (not needed)
);

CREATE TABLE Workloads (
    Id CHAR(36) PRIMARY KEY,
    CustomerId CHAR(36) NOT NULL,  -- References LOCAL copy
    EmployeeId CHAR(36) NOT NULL,  -- References LOCAL copy
    StartDate DATETIME(6) NOT NULL,
    StopDate DATETIME(6),
    Comment TEXT,
    CreatedAt DATETIME(6) NOT NULL,
    UpdatedAt DATETIME(6) NOT NULL,
    IsDeleted BOOLEAN NOT NULL DEFAULT FALSE,
    FOREIGN KEY (CustomerId) REFERENCES WorkloadCustomers(Id),
    FOREIGN KEY (EmployeeId) REFERENCES WorkloadEmployees(Id)
);
```

**Viktiga principer:**

1. **No Shared Tables** - Varje modul/service har EGNA tabeller
2. **Local Copies** - WorkloadsModule sparar lokala kopior av Customer/Employee
3. **Selective Fields** - Bara de fält som behövs sparas (t.ex. INTE SSN, Salary)
4. **Same ID** - ID:n är samma som i source-modulen för att möjliggöra korrelation
5. **No Foreign Keys Across Modules** - Foreign keys ENDAST till lokala kopior
6. **Event-Driven Sync** - Lokala kopior uppdateras via events

---

## 📡 Event-Driven Data Synchronization

### Customer Event Flow
```
CustomersApi                     NATS JetStream              WorkloadsApi
    │                                   │                          │
    │  1. POST /customers               │                          │
    │     (Create Customer)             │                          │
    │                                   │                          │
    │  2. Save to CustomersDB           │                          │
    │     (Full entity with all fields) │                          │
    │                                   │                          │
    │  3. Publish CustomerCreated       │                          │
    ├──────────────────────────────────►│                          │
    │     Event: {                      │                          │
    │       Id, OrgNumber, Name,        │                          │
    │       Email, PhoneNumber,         │                          │
    │       Address, PostalCode, City   │                          │
    │     }                             │                          │
    │                                   │  4. Subscriber receives  │
    │                                   ├─────────────────────────►│
    │                                   │                          │
    │                                   │  5. Save to WorkloadsDB  │
    │                                   │     (Selective fields!)  │
    │                                   │     WorkloadCustomer: {  │
    │                                   │       Id, Name,          │
    │                                   │       Email, PhoneNumber │
    │                                   │     }                    │
```

### Customer Event Subscriber (WorkloadsModule)
```csharp
public class CustomerEventsSubscriber : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var evt in events.SubscribeAsync<CustomerCreated>(stoppingToken))
        {
            // Create LOCAL copy with ONLY needed fields
            var customer = new WorkloadCustomer
            {
                Id = evt.Id,               // Same ID as source
                Name = evt.Name,           // ✅ Needed
                Email = evt.Email,         // ✅ Needed
                PhoneNumber = evt.PhoneNumber // ✅ Needed
                // OrgNumber, Address, PostalCode, City ❌ NOT stored
            };

            db.WorkloadCustomers.Add(customer);
            await db.SaveChangesAsync(stoppingToken);
        }
    }
}
```

**Fördelar med detta mönster:**

1. **Performance** - WorkloadsApi behöver INTE göra HTTP-anrop till CustomersApi
2. **Resilience** - WorkloadsApi fungerar även om CustomersApi är nere
3. **Autonomy** - Varje service bestämmer vilka fält den behöver
4. **Data Ownership** - Varje service äger sin egen data
5. **Eventual Consistency** - Data synkas asynkront via events
6. **Schema Evolution** - Lägg till fält i source utan att bryta konsumenter

---

## 🎯 Arkitektur

Detta projekt visar två deployment-strategier för samma kod:

### 1. **Modular Monolith** (ModularApi)
- **In-process events** via `ModularEvents` (System.Threading.Channels)
- Alla moduler körs i samma process
- **Separata databaser** - Database per module
- Event-driven eventual consistency (även in-process!)
- Enkel deployment, snabb kommunikation
- Perfekt för mindre team/projekt

### 2. **Microservices** (CustomersApi, EmployeesApi, WorkloadsApi)
- **Distributed events** via `MicroEvents` (NATS JetStream)
- Varje modul körs som separat service
- **Separata databaser** - Database per service
- Event-driven eventual consistency via NATS
- Oberoende scaling och deployment
- Perfekt för större team/organization

## 🏗️ Moduler & Services

| Modul               | Ansvar              | Events Published | Events Consumed  |
|---------------------|---------------------|------------------|------------------|
| **CustomersModule** | Customer management | `CustomerCreated`| -                |
|                     |                     | `CustomerUpdated`|                  |
|                     |                     | `CustomerDeleted`|                  |
| **EmployeesModule** | Employee management | `EmployeeCreated`| -                |
|                     |                     | `EmployeeUpdated`|                  |
|                     |                     | `EmployeeDeleted`|                  |
| **WorkloadsModule** | Workload tracking   | -                | `CustomerCreated`|
|                     |                     |                  | `CustomerUpdated`|
|                     |                     |                  | `CustomerDeleted`|
|                     |                     |                  | `EmployeeCreated`|
|                     |                     |                  | `EmployeeUpdated`|
|                     |                     |                  | `EmployeeDeleted`|

## 📦 Event Transports

### ModularEvents (In-Process)
```csharp
// Använder System.Threading.Channels
builder.Services.AddModularEvents();
```

### MicroEvents (Distributed)
```csharp
// Använder NATS JetStream
builder.Services.AddMicroEvents(builder.Configuration);
```

**Det enda som skiljer** är vilken implementation av `IEvents` som används!

## 🚀 Deployment

### Bygg container images
```powershell
./build.ps1 -EnvironmentFile .\environment-local-hp-customersapi.psd1
./build.ps1 -EnvironmentFile .\environment-local-hp-employeesapi.psd1
./build.ps1 -EnvironmentFile .\environment-local-hp-workloadsapi.psd1
```

### Deploy till Kubernetes
```powershell
./deploy.ps1 -EnvironmentFile .\environment-local-hp-customersapi.psd1
./deploy.ps1 -EnvironmentFile .\environment-local-hp-employeesapi.psd1
./deploy.ps1 -EnvironmentFile .\environment-local-hp-workloadsapi.psd1
```

### Deploy Grafana Dashboards
```powershell
./deploy-dashboards.ps1
```

## 🧪 Verifiering av Event-Driven Flow

1. **Skapa en Customer** via CustomersApi (Scalar UI: `https://customersapi.local/scalar/v1`)
   - CustomerCreated event publiceras till NATS
   - WorkloadsApi lyssnar och sparar customer lokalt

2. **Skapa en Employee** via EmployeesApi (Scalar UI: `https://employeesapi.local/scalar/v1`)
   - EmployeeCreated event publiceras till NATS
   - WorkloadsApi lyssnar och sparar employee lokalt

3. **Skapa en Workload** via WorkloadsApi (Scalar UI: `https://workloadsapi.local/scalar/v1`)
   - Använd CustomerID och EmployeeID från tidigare
   - WorkloadsApi hämtar CustomerName och EmployeeName från sin lokala databas

4. **Lista Workloads**
   - Se att CustomerName och EmployeeName visas korrekt
   - **Detta bekräftar att NATS events fungerar!** ✅

## 📊 Observability

Alla microservices har komplett observability:

- **Serilog → Loki**: Structured logging med app/environment labels
- **OpenTelemetry → Jaeger**: Distributed tracing via OTLP gRPC
- **Prometheus**: Metrics export på `/metrics`
- **Grafana**: Dashboards för CPU, Memory, HTTP, Logs
- **HealthChecks**: `/health/live`, `/health/ready` med DbContext check

### Endpoints
- Loki: `http://loki.monitoring.svc.cluster.local:3100`
- Jaeger: `http://jaeger.monitoring.svc.cluster.local:4317`
- Prometheus: ServiceMonitor scraping
- Grafana: 6 dashboards (2 per service)

## 🛠️ Teknologi Stack

- **.NET 10** - Latest framework med C# 14
- **FastEndpoints** - Modern endpoint routing
- **Entity Framework Core 10** - ORM för MySQL
- **MySQL** - Databas (separate per module)
- **NATS JetStream** - Event streaming
- **Serilog** - Structured logging
- **OpenTelemetry** - Distributed tracing
- **Helm** - Kubernetes deployment
- **Traefik** - Ingress controller
- **Scalar** - OpenAPI UI

## 🔑 Key Learnings

### Enkel Migration från Monolith till Microservices

**Det enda som krävs:**

1. ✅ Bytt `AddModularEvents()` → `AddMicroEvents()`
2. ✅ Lagt till NATS-konfiguration i appsettings.json
3. ✅ Separata deployment-pipelines (build.ps1/deploy.ps1)

**Samma kod används!** Modulerna är identiska i både ModularApi och Microservices.

### Event Contracts som Gränssnitt

- `CustomersContract` - Shared mellan CustomersApi och WorkloadsApi
- `EmployeesContract` - Shared mellan EmployeesApi och WorkloadsApi
- **Versioning** genom namespace/assembly versioning
- **Immutable positional records** för events

### Eventually Consistent by Design

WorkloadsModule har sin egen kopia av Customer/Employee data:
- Snabbare queries (ingen cross-service call)
- Resilient (fungerar även om andra services är nere)
- Eventually consistent (liten fördröjning via NATS)

## 📝 Configuration

### Development (localhost)
```json
{
  "Nats": {
    "Url": "nats://localhost:4222"
  }
}
```

### Production (Kubernetes)
```json
{
  "Nats": {
    "Url": "nats://nats.nats.svc.cluster.local:4222"
  }
}
```

## 🎓 Architectural Patterns

- **Clean Architecture** - Separation of concerns
- **CQRS** - Command Query Responsibility Segregation
- **Vertical Slice Architecture** - Feature folders
- **Event Sourcing (light)** - Events as first-class citizens
- **Saga Pattern** - Eventual consistency via events
- **SOLID Principles** - Throughout the codebase

## 📚 Resources

- [NATS JetStream Documentation](https://docs.nats.io/nats-concepts/jetstream)
- [Event-Driven Architecture](https://martinfowler.com/articles/201701-event-driven.html)
- [Modular Monolith](https://www.kamilgrzybek.com/blog/posts/modular-monolith-primer)
- [FastEndpoints](https://fast-endpoints.com/)

## 🤝 Contributing

Detta är ett demo-projekt för att visa migration från modular monolith till microservices. Feel free to use as reference!

## 📄 License

MIT License - Se LICENSE fil för detaljer.


