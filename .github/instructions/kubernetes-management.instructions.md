---
description: Kubernetes cluster management with k3d/k3s for deploying microservices
applyTo: '**/charts/**/*.yaml,**/*.ps1'
---

# Kubernetes Cluster Management

## Overview

Deploy microservices (CustomersApi, EmployeesApi, WorkloadsApi) to K3s/K3d Kubernetes clusters using:
- **k3d** - Lightweight Kubernetes in Docker
- **Helm** - Package manager for Kubernetes
- **PowerShell** - Automation scripts

## Cluster Architecture

### k3d Local Cluster
- Single server node
- Local registry (registry.local:5000)
- Traefik ingress controller
- NATS JetStream for events
- MySQL for persistence
- Monitoring stack (Prometheus, Grafana, Loki, Jaeger)

### Application Deployment
```
Cluster
├── default namespace
│   ├── CustomersApi (deployment + service + ingress)
│   ├── EmployeesApi (deployment + service + ingress)
│   ├── WorkloadsApi (deployment + service + ingress)
│   └── NATS (messaging)
├── mysql namespace
│   └── MySQL (database)
└── monitoring namespace
    ├── Prometheus
    ├── Grafana
    ├── Loki
    └── Jaeger
```

## NATS JetStream Configuration

### Installation (Helm)

```powershell
helm repo add nats https://nats-io.github.io/k8s/helm/charts/
helm install nats nats/nats `
    --namespace default `
    --set nats.jetstream.enabled=true `
    --set nats.jetstream.memStorage.enabled=true `
    --set nats.jetstream.memStorage.size=1Gi `
    --set nats.jetstream.fileStorage.enabled=true `
    --set nats.jetstream.fileStorage.size=10Gi
```

### NATS Configuration for MicroEvents

**Service URL**: `nats://nats.default.svc.cluster.local:4222`

In application `appsettings.json`:
```json
{
  "Nats": {
    "Url": "nats://nats.default.svc.cluster.local:4222"
  }
}
```

### Stream Setup (PowerShell)

```powershell
# Using NATS CLI (C:\Apps\natsclient\nats.exe)
$natsExe = "C:\Apps\natsclient\nats.exe"

# Port-forward NATS for CLI access
kubectl port-forward svc/nats 4222:4222 -n default

# Create stream for customer events
& $natsExe stream add CUSTOMERS `
    --subjects "events.Customer*" `
    --retention workqueue `
    --storage file `
    --max-msgs=-1 `
    --max-age=7d

# Create consumer for WorkloadsApi
& $natsExe consumer add CUSTOMERS workloads-consumer `
    --ack explicit `
    --max-deliver 5 `
    --wait 30s
```

## MySQL Configuration

### Installation (Helm)

```powershell
helm repo add bitnami https://charts.bitnami.com/bitnami
helm install mysql bitnami/mysql `
    --namespace mysql `
    --create-namespace `
    --set auth.rootPassword=MyPassword123 `
    --set primary.persistence.size=10Gi
```

### Database Setup

```powershell
# Port-forward MySQL
kubectl port-forward svc/mysql 3306:3306 -n mysql

# Create databases
mysql -h 127.0.0.1 -u root -pMyPassword123 <<EOF
CREATE DATABASE customers;
CREATE DATABASE employees;
CREATE DATABASE workloads;
EOF
```

### Connection Strings

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=mysql.mysql.svc.cluster.local;Database=customers;User=root;Password=MyPassword123"
  }
}
```

## Helm Chart Structure

### Standard Chart Layout

```
charts/customersapi/
├── Chart.yaml
├── values.yaml
└── templates/
    ├── _helpers.tpl
    ├── deployment.yaml
    ├── service.yaml
    ├── ingress.yaml
    ├── configmap.yaml
    ├── secret.yaml
    ├── servicemonitor.yaml
    └── hpa.yaml
```

### values.yaml

```yaml
replicaCount: 2

image:
  repository: registry.local/customersapi
  tag: "0.1.0-dev.1"
  pullPolicy: IfNotPresent

service:
  type: ClusterIP
  port: 8080

ingress:
  enabled: true
  className: traefik
  hosts:
    - customersapi.local
  tls:
    enabled: true
    secretName: customersapi-tls

resources:
  requests:
    memory: "256Mi"
    cpu: "200m"
  limits:
    memory: "512Mi"
    cpu: "1000m"

env:
  - name: ASPNETCORE_ENVIRONMENT
    value: "Production"
  - name: ConnectionStrings__DefaultConnection
    valueFrom:
      secretKeyRef:
        name: customersapi-secret
        key: connectionString
  - name: Nats__Url
    value: "nats://nats.default.svc.cluster.local:4222"

healthChecks:
  liveness:
    path: /health/live
    initialDelaySeconds: 30
  readiness:
    path: /health/ready
    initialDelaySeconds: 10

autoscaling:
  enabled: true
  minReplicas: 2
  maxReplicas: 10
  targetCPUUtilizationPercentage: 70

monitoring:
  prometheus:
    enabled: true
    port: 8080
    path: /metrics
```

### deployment.yaml

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: {{ include "customersapi.fullname" . }}
spec:
  replicas: {{ .Values.replicaCount }}
  selector:
    matchLabels:
      app: {{ include "customersapi.name" . }}
  template:
    metadata:
      labels:
        app: {{ include "customersapi.name" . }}
    spec:
      containers:
      - name: {{ .Chart.Name }}
        image: "{{ .Values.image.repository }}:{{ .Values.image.tag }}"
        imagePullPolicy: {{ .Values.image.pullPolicy }}
        ports:
        - name: http
          containerPort: 8080
        env:
        {{- toYaml .Values.env | nindent 8 }}
        livenessProbe:
          httpGet:
            path: {{ .Values.healthChecks.liveness.path }}
            port: http
          initialDelaySeconds: {{ .Values.healthChecks.liveness.initialDelaySeconds }}
        readinessProbe:
          httpGet:
            path: {{ .Values.healthChecks.readiness.path }}
            port: http
          initialDelaySeconds: {{ .Values.healthChecks.readiness.initialDelaySeconds }}
        resources:
          {{- toYaml .Values.resources | nindent 10 }}
```

## Monitoring Integration

### ServiceMonitor (Prometheus)

```yaml
apiVersion: monitoring.coreos.com/v1
kind: ServiceMonitor
metadata:
  name: customersapi
  namespace: default
  labels:
    release: kube-prometheus-stack
spec:
  selector:
    matchLabels:
      app: customersapi
  endpoints:
  - port: http
    path: /metrics
    interval: 30s
```

### Grafana Dashboards

Deploy dashboards as ConfigMaps:

```powershell
# deploy-dashboards.ps1
kubectl create configmap customersapi-dashboards `
    --from-file=grafana-dashboards/ `
    --namespace monitoring `
    --dry-run=client -o yaml | `
kubectl apply -f -

kubectl label configmap customersapi-dashboards `
    grafana_dashboard=1 `
    --namespace monitoring
```

## TLS Certificates

### Using mkcert

```powershell
# Install mkcert
choco install mkcert

# Install local CA
mkcert -install

# Generate certificates
mkcert customersapi.local employeesapi.local workloadsapi.local

# Create Kubernetes secret
kubectl create secret tls api-tls `
    --cert=customersapi.local+2.pem `
    --key=customersapi.local+2-key.pem `
    --namespace default
```

## Deployment Workflow

### 1. Build Application

```powershell
.\build.ps1 -ClusterName local-dev -IncrementVersionPart Build
```

### 2. Deploy to Kubernetes

```powershell
.\deploy.ps1 -ClusterName local-dev -Namespace default
```

### 3. Verify Deployment

```powershell
# Check pods
kubectl get pods -n default

# Check services
kubectl get svc -n default

# Check ingress
kubectl get ingress -n default

# Test health
curl https://customersapi.local/health/ready
```

## Troubleshooting

### Pod Not Starting

```powershell
# Check pod status
kubectl describe pod <pod-name> -n default

# Check logs
kubectl logs <pod-name> -n default

# Check events
kubectl get events -n default --sort-by='.lastTimestamp'
```

### Database Connection Issues

```powershell
# Test MySQL connectivity from pod
kubectl exec -it <pod-name> -n default -- /bin/sh
nc -zv mysql.mysql.svc.cluster.local 3306
```

### NATS Connection Issues

```powershell
# Check NATS pod
kubectl get pods -n default | grep nats

# Check NATS logs
kubectl logs -n default <nats-pod-name>

# Test NATS connectivity
kubectl exec -it <pod-name> -n default -- /bin/sh
nc -zv nats.default.svc.cluster.local 4222
```

## Resource Management

### MySQL Init Container

Add to deployment for applications using MySQL:

```yaml
initContainers:
- name: wait-for-mysql
  image: busybox:1.35
  command: ['sh', '-c', 'until nc -zv mysql.mysql.svc.cluster.local 3306; do sleep 2; done']
```

### EF Core Retry Logic

Always enable in applications:

```csharp
options.UseMySql(connectionString, serverVersion, mySqlOptions =>
{
    mySqlOptions.EnableRetryOnFailure(
        maxRetryCount: 10,
        maxRetryDelay: TimeSpan.FromSeconds(5));
});
```

## Scaling

### Manual Scaling

```powershell
kubectl scale deployment customersapi --replicas=5 -n default
```

### Horizontal Pod Autoscaler

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: customersapi
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: customersapi
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
```

## Network Policies

### Allow NATS Communication

```yaml
apiVersion: networking.k8s.io/v1
kind: NetworkPolicy
metadata:
  name: allow-nats
  namespace: default
spec:
  podSelector:
    matchLabels:
      app: customersapi
  policyTypes:
  - Egress
  egress:
  - to:
    - podSelector:
        matchLabels:
          app.kubernetes.io/name: nats
    ports:
    - protocol: TCP
      port: 4222
```

## Best Practices

### Resource Limits
- ✅ Always set requests and limits
- ✅ Use init containers for dependencies
- ✅ Enable retry logic in applications

### Health Checks
- ✅ Implement liveness and readiness probes
- ✅ Use different endpoints for each
- ✅ Set appropriate delays and timeouts

### Monitoring
- ✅ Expose /metrics endpoint
- ✅ Create ServiceMonitor
- ✅ Deploy Grafana dashboards
- ✅ Enable structured logging to Loki

### NATS
- ✅ Use JetStream for persistence
- ✅ Enable file storage for reliability
- ✅ Set appropriate retention policies
- ✅ Create durable consumers

### Scaling
- ✅ Use HPA for automatic scaling
- ✅ Set min replicas >= 2 for HA
- ✅ Monitor pod resource usage
- ✅ Adjust limits based on metrics
