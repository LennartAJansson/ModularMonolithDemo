---
description: PowerShell scripting standards and module conventions for build/deploy automation
applyTo: '**/*.ps1,**/*.psm1,**/*.psd1'
---

# PowerShell Scripting Standards

## Requirements

- **PowerShell Version**: 7.x or higher
- **Module Structure**: Standard PowerShell module layout
- **Error Handling**: Fail fast with strict mode

## Module Organization

### Module Folder Structure

```
modules/
├── ClusterTools/
│   ├── ClusterTools.psm1
│   └── ClusterTools.psd1
├── VersionManagement/
│   ├── VersionManagement.psm1
│   └── VersionManagement.psd1
├── ApplicationBuild/
│   ├── ApplicationBuild.psm1
│   └── ApplicationBuild.psd1
└── HelmDeployment/
    ├── HelmDeployment.psm1
    └── HelmDeployment.psd1
```

### Required Modules

1. **ClusterTools** - K3d/K3s cluster management
   - Create/delete clusters
   - Context switching
   - Cluster status checks

2. **VersionManagement** - BuildVersionService integration
   - Semantic versioning
   - Version increment
   - Registry validation

3. **ApplicationBuild** - .NET application builds
   - Docker builds
   - Version injection
   - Registry push

4. **HelmDeployment** - Kubernetes deployments
   - Helm chart installation
   - Secret management
   - TLS certificates

## Coding Standards

### Function Naming

Use verb-noun format:

```powershell
# Good
Get-ClusterInfo
New-K3dCluster
Set-ActiveContext
Invoke-HelmUpgrade

# Avoid
ClusterInfo
CreateCluster
```

### Parameter Conventions

```powershell
function Deploy-Application {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$ApplicationName,
        
        [Parameter(Mandatory = $true)]
        [string]$ClusterName,
        
        [Parameter(Mandatory = $false)]
        [string]$Version,
        
        [switch]$Force
    )
    
    # Implementation
}
```

### Error Handling (Fail Fast)

```powershell
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

try {
    $result = Invoke-SomeCommand
    
    if (-not $result) {
        throw "Operation failed: expected result not returned"
    }
}
catch {
    Write-Error "Failed to execute: $_"
    throw
}
```

## BuildVersionService Integration

### API Endpoints

Base URL: `https://buildversionservice{ingress-suffix}`

- **Create**: `POST /buildversions` - Initial version (0.0.0-dev.0)
- **Increment**: `POST /buildversions/increase` - Increment version part
- **Get**: `GET /buildversions/by-name/{projectName}` - Current version

### Version Parts

- Major = 0
- Minor = 1
- Build = 2 (default increment)
- Revision = 3

### Increment Behavior

**CRITICAL**: Incrementing resets lower parts to 0

- Major → Minor, Build, Revision = 0
- Minor → Build, Revision = 0
- Build → Revision = 0
- Revision → No reset

### Version Management Functions

```powershell
function Get-BuildVersion {
    param(
        [string]$ProjectName,
        [string]$ClusterName
    )
    
    $baseUrl = Get-BuildVersionServiceUrl -ClusterName $ClusterName
    $encodedName = [uri]::EscapeDataString($ProjectName)
    
    try {
        $response = Invoke-RestMethod -Uri "$baseUrl/buildversions/by-name/$encodedName"
        return $response
    }
    catch {
        Write-Warning "Failed to get version: $_"
        return @{ semanticVersion = "0.0.0-dev.1" }
    }
}

function Increment-BuildVersion {
    param(
        [string]$ProjectName,
        [string]$ClusterName,
        [ValidateSet('Major', 'Minor', 'Build', 'Revision')]
        [string]$VersionPart = 'Build'
    )
    
    $partMap = @{
        'Major' = 0; 'Minor' = 1; 'Build' = 2; 'Revision' = 3
    }
    
    $baseUrl = Get-BuildVersionServiceUrl -ClusterName $ClusterName
    $body = @{
        projectName = $ProjectName
        part = $partMap[$VersionPart]
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri "$baseUrl/buildversions/increase" `
        -Method Post -Body $body -ContentType 'application/json'
    
    Write-Host "Version incremented: $($response.semanticVersion)" -ForegroundColor Green
    return $response
}
```

## Application Build Patterns

### .NET Build with Container Support

```powershell
function Build-DotNetApplication {
    param(
        [string]$ProjectPath,
        [string]$ProjectName,
        [string]$ClusterName,
        [string]$IncrementVersionPart = 'Build'
    )
    
    # Increment version
    $buildVersion = Increment-BuildVersion -ProjectName $ProjectName `
        -ClusterName $ClusterName `
        -VersionPart $IncrementVersionPart
    
    # Set version properties
    $assemblyVersion = "$($buildVersion.major).$($buildVersion.minor).0.0"
    $fileVersion = "$($buildVersion.major).$($buildVersion.minor).$($buildVersion.build).$($buildVersion.revision)"
    $informationalVersion = $buildVersion.semanticVersion
    
    # Get registry
    $registry = Get-ClusterRegistry -ClusterName $ClusterName
    $imageName = $ProjectName.ToLower()
    
    # Build and publish
    $publishArgs = @(
        'publish', $ProjectPath, '-c', 'Release'
        "/p:AssemblyVersion=$assemblyVersion"
        "/p:FileVersion=$fileVersion"
        "/p:InformationalVersion=$informationalVersion"
        '/p:PublishProfile=DefaultContainer'
        "/p:ContainerImageName=$imageName"
        "/p:ContainerImageTag=$($buildVersion.semanticVersion)"
        "/p:ContainerRegistry=$registry"
    )
    
    & dotnet @publishArgs
    
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed"
    }
    
    Write-Host "✓ Built $imageName`:$($buildVersion.semanticVersion)" -ForegroundColor Green
}
```

## Docker Registry Validation

### Registry HTTP API v2

```powershell
function Test-RegistryImage {
    param(
        [string]$ImageName,
        [string]$Tag,
        [string]$ClusterName
    )
    
    $registry = Get-ClusterRegistry -ClusterName $ClusterName
    $registryParts = $registry -split ':'
    $registryHost = $registryParts[0]
    $registryPort = if ($registryParts.Count -gt 1) { $registryParts[1] } else { '5000' }
    
    $apiUrl = "http://${registryHost}:${registryPort}/v2/${ImageName}/tags/list"
    
    try {
        $response = Invoke-RestMethod -Uri $apiUrl
        return $Tag -in $response.tags
    }
    catch {
        return $false
    }
}
```

## Helm Deployment

```powershell
function Install-HelmChart {
    param(
        [string]$ChartPath,
        [string]$ReleaseName,
        [string]$Namespace,
        [hashtable]$Values
    )
    
    # Create namespace if not exists
    kubectl create namespace $Namespace --dry-run=client -o yaml | kubectl apply -f -
    
    # Prepare values
    $valuesFile = Join-Path $env:TEMP "$ReleaseName-values.yaml"
    $Values | ConvertTo-Yaml | Set-Content $valuesFile
    
    # Install chart
    helm upgrade --install $ReleaseName $ChartPath `
        --namespace $Namespace `
        --values $valuesFile `
        --wait
    
    if ($LASTEXITCODE -ne 0) {
        throw "Helm install failed"
    }
    
    Write-Host "✓ Deployed $ReleaseName to $Namespace" -ForegroundColor Green
}
```

## Template Scripts

### build.ps1

```powershell
#Requires -Version 7.0
param(
    [string]$ClusterName = "local-dev",
    [ValidateSet('Major', 'Minor', 'Build', 'Revision')]
    [string]$IncrementVersionPart = 'Build'
)

$ErrorActionPreference = 'Stop'

try {
    Import-Module ApplicationBuild -ErrorAction Stop
    
    $buildResult = Build-DotNetApplication `
        -ProjectPath "./CustomersApi/CustomersApi.csproj" `
        -ProjectName "CustomersApi" `
        -ClusterName $ClusterName `
        -IncrementVersionPart $IncrementVersionPart
    
    Write-Host "`n✓ Build completed!" -ForegroundColor Green
}
catch {
    Write-Error "Build failed: $_"
    exit 1
}
```

### deploy.ps1

```powershell
#Requires -Version 7.0
param(
    [string]$ClusterName = "local-dev",
    [string]$Namespace = "default"
)

$ErrorActionPreference = 'Stop'

try {
    Import-Module HelmDeployment -ErrorAction Stop
    Import-Module VersionManagement -ErrorAction Stop
    
    # Get current version
    $version = Get-BuildVersion -ProjectName "CustomersApi" -ClusterName $ClusterName
    
    # Deploy with Helm
    Install-HelmChart `
        -ChartPath "./charts/customersapi" `
        -ReleaseName "customersapi" `
        -Namespace $Namespace `
        -Values @{
            image = @{
                tag = $version.semanticVersion
            }
        }
    
    Write-Host "`n✓ Deployment completed!" -ForegroundColor Green
}
catch {
    Write-Error "Deploy failed: $_"
    exit 1
}
```

## Best Practices

### Non-Interactive Design
- No `Read-Host` prompts
- All parameters with defaults
- Pipeline compatible

### Logging
```powershell
Write-Host "Starting..." -ForegroundColor Green
Write-Verbose "Details"
Write-Warning "Issue"
Write-Error "Error"
```

### Module Loading
```powershell
Import-Module ModuleName -Force  # For development
Import-Module ModuleName -ErrorAction Stop  # For production
```

### Configuration
Store in `clusterinfo.json` or environment-specific `.psd1` files

## Notes

- Always use `$ErrorActionPreference = 'Stop'`
- Enable `Set-StrictMode -Version Latest`
- URL-encode project names for API calls
- Use registry HTTP API for validation
- Test with both local and remote clusters
