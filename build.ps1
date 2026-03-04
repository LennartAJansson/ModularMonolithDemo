#Requires -Version 7.0

<#
.SYNOPSIS
    Build script for application
.DESCRIPTION
    Builds application using BuildTool module
.PARAMETER EnvironmentFile
    Path to environment configuration file (environment-{ClusterName}-{ApplicationName}.psd1)
.PARAMETER IncrementPart
    Which version part to increment: Major, Minor, Build, or Revision (default: Build)
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$EnvironmentFile,
    
    [Parameter(Mandatory = $false)]
    [ValidateSet('Major', 'Minor', 'Build', 'Revision')]
    [string]$IncrementPart = 'Build'
)

# Import modules from PSModulePath
Import-Module CommonTool -Force
Import-Module BuildTool -Force

try {
    Write-StepMessage "Starting build process"
    
    # Read environment configuration
    $envConfig = Read-EnvironmentFile -Path $EnvironmentFile
    Write-InfoMessage "Loaded environment: $($envConfig.ClusterName) - $($envConfig.ApplicationName)"
    
    # Invoke build
    $result = Invoke-ApplicationBuild `
        -EnvironmentConfig $envConfig `
        -IncrementPart $IncrementPart
    
    Write-SuccessMessage "Build completed successfully!"
    Write-InfoMessage "Version: $($result.Version)"
    Write-InfoMessage "Image: $($result.ImageTag)"
}
catch {
    Write-ErrorMessage "Build failed: $_"
    exit 1
}
