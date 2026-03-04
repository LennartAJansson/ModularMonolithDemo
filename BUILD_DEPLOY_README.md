# Event-Driven Microservices - Build & Deploy

Detta projekt innehåller tre microservices med fullständig observability-stack.

## Microservices

- **CustomersApi** - Customer domain API
- **EmployeesApi** - Employee domain API  
- **WorkloadsApi** - Workloads domain API

## Observability Stack

Varje microservice har inbyggt stöd för:

- **Serilog** → Strukturerad loggning till Loki
- **OpenTelemetry** → Distributed tracing till Jaeger
- **Prometheus** → Metrics export på `/metrics`
- **HealthChecks** → Kubernetes liveness (`/health/live`) och readiness (`/health/ready`)

## Build & Deploy

### Prerequisit

1. **PowerShell 7+**
2. **BuildTool, DeployTool, CommonTool** - PowerShell modules i `$env:PSModulePath`
3. **Kubernetes cluster** (K3s)
4. **Monitoring stack** - Prometheus, Grafana, Loki, Jaeger deployed i `monitoring` namespace

### Build Image

Bygg en microservice med .NET SDK:

```powershell
# CustomersApi
.\build.ps1 -EnvironmentFile .\environment-local-hp-customersapi.psd1

# EmployeesApi
.\build.ps1 -EnvironmentFile .\environment-local-hp-employeesapi.psd1

# WorkloadsApi
.\build.ps1 -EnvironmentFile .\environment-local-hp-workloadsapi.psd1
```

Build-processen:
1. Läser version från `version.json` (eller skapar 1.0.0.0)
2. Incrementerar build number (default)
3. Bygger .NET image med `dotnet publish`
4. Skapar OCI container image
5. Pushar till local registry (containerd)

### Deploy till Kubernetes

Deploya en microservice med Helm:

```powershell
# CustomersApi
.\deploy.ps1 -EnvironmentFile .\environment-local-hp-customersapi.psd1

# EmployeesApi
.\deploy.ps1 -EnvironmentFile .\environment-local-hp-employeesapi.psd1

# WorkloadsApi
.\deploy.ps1 -EnvironmentFile .\environment-local-hp-workloadsapi.psd1
```

Deploy-processen:
1. Läser version från `version.json`
2. Deployer Helm chart från `charts/{ServiceName}-deploy`
3. Använder values från `charts/{ServiceName}-deploy/{ServiceName}-local-hp.yaml`
4. Skapar namespace om den inte finns
5. Deployer ConfigMap, Secret, Deployment, Service, Ingress, HPA, ServiceMonitor

### Deploy Grafana Dashboards

```powershell
.\deploy-dashboards.ps1
```

Detta skapar ConfigMaps i `monitoring` namespace med alla dashboards:
- `customersapi-standard` - Standard metrics (CPU, Memory, HTTP)
- `customersapi-logs` - Log aggregation från Loki
- `employeesapi-standard` - Standard metrics
- `employeesapi-logs` - Log aggregation
- `workloadsapi-standard` - Standard metrics
- `workloadsapi-logs` - Log aggregation

## Environment Files

Varje microservice har en egen environment-fil (`.psd1`):

```powershell
@{
    ClusterName = 'local-hp'
    ApplicationName = 'customersapi'
    ProjectPath = '.\CustomersApi\CustomersApi.csproj'
    ChartPath = '.\charts\CustomersApi-deploy'
    ChartValues = '.\charts\CustomersApi-deploy\CustomersApi-local-hp.yaml'
    ApplicationType = 'dotnet-webapi'
    DashboardPath = '.\dashboards'
    IngressHost = 'customersapi.local'
    Namespace = 'customersapi'
}
```

## Helm Charts

Varje chart innehåller:

- **Chart.yaml** - Chart metadata
- **values.yaml** - Default values
- **{ServiceName}-local-hp.yaml** - Environment-specific values
- **templates/deployment.yaml** - K8s Deployment med init containers
- **templates/service.yaml** - ClusterIP Service
- **templates/ingress.yaml** - Traefik Ingress med TLS
- **templates/hpa.yaml** - Horizontal Pod Autoscaler
- **templates/servicemonitor.yaml** - Prometheus ServiceMonitor
- **templates/prometheusrule.yaml** - Alert rules
- **templates/configmap.yaml** - Application configuration
- **templates/sealedsecret.yaml** - Sealed secrets
- **templates/pdb.yaml** - Pod Disruption Budget

## Kubernetes Resources

Efter deploy hittar du:

### CustomersApi
- **Namespace**: `customersapi`
- **Ingress**: `https://customersapi.local`
- **Metrics**: `http://customersapi.customersapi.svc.cluster.local:8080/metrics`
- **Health**: `http://customersapi.customersapi.svc.cluster.local:8080/health/ready`

### EmployeesApi
- **Namespace**: `employeesapi`
- **Ingress**: `https://employeesapi.local`
- **Metrics**: `http://employeesapi.employeesapi.svc.cluster.local:8080/metrics`
- **Health**: `http://employeesapi.employeesapi.svc.cluster.local:8080/health/ready`

### WorkloadsApi
- **Namespace**: `workloadsapi`
- **Ingress**: `https://workloadsapi.local`
- **Metrics**: `http://workloadsapi.workloadsapi.svc.cluster.local:8080/metrics`
- **Health**: `http://workloadsapi.workloadsapi.svc.cluster.local:8080/health/ready`

## Observability Endpoints

### Grafana Dashboards
- http://grafana.monitoring.svc.cluster.local/dashboards
  - CustomersApi - Standard Metrics
  - CustomersApi - Logs
  - EmployeesApi - Standard Metrics
  - EmployeesApi - Logs
  - WorkloadsApi - Standard Metrics
  - WorkloadsApi - Logs

### Prometheus
- http://prometheus.monitoring.svc.cluster.local
- Scrapes `/metrics` från alla services via ServiceMonitor

### Loki
- http://loki.monitoring.svc.cluster.local:3100
- Samlar logs från alla pods via Promtail

### Jaeger
- http://jaeger-query.monitoring.svc.cluster.local:16686
- Distributed tracing från OpenTelemetry OTLP exporter

## Development

Lokalt (utanför kluster):
- Serilog loggar till konsol + `http://localhost:3100` (Loki)
- OpenTelemetry traces till `http://localhost:4317` (Jaeger OTLP)
- Prometheus metrics fortfarande tillgängliga på `/metrics`

I kluster (production):
- Serilog → `http://loki.monitoring.svc.cluster.local:3100`
- OpenTelemetry → `http://jaeger-collector.monitoring.svc.cluster.local:4317`
- Prometheus scrape från ServiceMonitor

## Troubleshooting

### Check Pod Status
```powershell
kubectl get pods -n customersapi
kubectl get pods -n employeesapi
kubectl get pods -n workloadsapi
```

### View Logs
```powershell
kubectl logs -n customersapi -l app=customersapi
kubectl logs -n employeesapi -l app=employeesapi
kubectl logs -n workloadsapi -l app=workloadsapi
```

### Check Ingress
```powershell
kubectl get ingress -n customersapi
kubectl get ingress -n employeesapi
kubectl get ingress -n workloadsapi
```

### Test Health Endpoints
```powershell
curl https://customersapi.local/health/live
curl https://customersapi.local/health/ready
curl https://customersapi.local/metrics
```

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                     Kubernetes Cluster                       │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ CustomersApi │  │ EmployeesApi │  │ WorkloadsApi │      │
│  │  Namespace   │  │  Namespace   │  │  Namespace   │      │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘      │
│         │                  │                  │              │
│         └──────────────────┼──────────────────┘              │
│                            │                                 │
│                    ┌───────▼───────┐                         │
│                    │   Monitoring   │                        │
│                    │   Namespace    │                        │
│                    ├────────────────┤                        │
│                    │  Prometheus    │◄─── ServiceMonitor    │
│                    │  Grafana       │◄─── Dashboards        │
│                    │  Loki          │◄─── Logs              │
│                    │  Jaeger        │◄─── Traces            │
│                    │  AlertManager  │◄─── Alerts            │
│                    └────────────────┘                        │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

## Event Communication

Services kommunicerar via NATS JetStream:

```
CustomersApi ──┐
               ├──► NATS JetStream ──► Event Subscribers
EmployeesApi ──┤
               │
WorkloadsApi ──┘
```

Event contracts finns i:
- `CustomersContract` - CustomerCreated, CustomerUpdated, CustomerDeleted
- `EmployeesContract` - EmployeeCreated, EmployeeUpdated, EmployeeDeleted
