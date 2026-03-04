#Requires -Version 7.0

<#
.SYNOPSIS
    Deploy script for application
.DESCRIPTION
    Deploys application using DeployTool module
.PARAMETER EnvironmentFile
    Path to environment configuration file (environment-{ClusterName}-{ApplicationName}.psd1)
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$EnvironmentFile
)

# Import modules from PSModulePath
Import-Module CommonTool -Force
Import-Module DeployTool -Force

try {
    Write-StepMessage "Starting deployment process"
    
    # Read environment configuration
    $envConfig = Read-EnvironmentFile -Path $EnvironmentFile
    Write-InfoMessage "Loaded environment: $($envConfig.ClusterName) - $($envConfig.ApplicationName)"
    
    # Invoke deployment
    Invoke-ApplicationDeploy -EnvironmentConfig $envConfig
    
    Write-SuccessMessage "Deployment completed successfully!"
}
catch {
    Write-ErrorMessage "Deployment failed: $_"
    exit 1
}
