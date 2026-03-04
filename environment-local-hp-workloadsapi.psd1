@{
    ClusterName = 'local-hp'
    ApplicationName = 'workloadsapi'
    ProjectPath = '.\WorkloadsApi\WorkloadsApi.csproj'
    ChartPath = '.\charts\WorkloadsApi-deploy'
    ChartValues = '.\charts\WorkloadsApi-deploy\WorkloadsApi-local-hp.yaml'
    ApplicationType = 'dotnet-webapi'
    DashboardPath = '.\dashboards'
    IngressHost = 'workloadsapi.local'
    Namespace = 'workloadsapi'
}
