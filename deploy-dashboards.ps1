#Requires -Version 7.0

<#
.SYNOPSIS
    Deploy Grafana dashboards for all microservices
.DESCRIPTION
    Deploys Grafana dashboards to Kubernetes configmap
#>

[CmdletBinding()]
param()

Write-Host "Deploying Grafana dashboards..." -ForegroundColor Cyan

$dashboards = @(
    @{ Name = "customersapi-standard"; File = "dashboards/customersapi-standard.json" },
    @{ Name = "customersapi-logs"; File = "dashboards/customersapi-logs.json" },
    @{ Name = "employeesapi-standard"; File = "dashboards/employeesapi-standard.json" },
    @{ Name = "employeesapi-logs"; File = "dashboards/employeesapi-logs.json" },
    @{ Name = "workloadsapi-standard"; File = "dashboards/workloadsapi-standard.json" },
    @{ Name = "workloadsapi-logs"; File = "dashboards/workloadsapi-logs.json" }
)

foreach ($dashboard in $dashboards) {
    $configMapName = "grafana-dashboard-$($dashboard.Name)"
    
    Write-Host "  Creating ConfigMap: $configMapName" -ForegroundColor Yellow
    
    kubectl create configmap $configMapName `
        --from-file="$($dashboard.Name).json=$($dashboard.File)" `
        --namespace=monitoring `
        --dry-run=client -o yaml | kubectl apply -f -
    
    # Label it so Grafana picks it up
    kubectl label configmap $configMapName `
        grafana_dashboard=1 `
        --namespace=monitoring `
        --overwrite
}

Write-Host "`nAll dashboards deployed successfully!" -ForegroundColor Green
