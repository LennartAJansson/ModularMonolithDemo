@{
    ClusterName = 'local-hp'
    ApplicationName = 'employeesapi'
    ProjectPath = '.\EmployeesApi\EmployeesApi.csproj'
    ChartPath = '.\charts\EmployeesApi-deploy'
    ChartValues = '.\charts\EmployeesApi-deploy\EmployeesApi-local-hp.yaml'
    ApplicationType = 'dotnet-webapi'
    DashboardPath = '.\dashboards'
    IngressHost = 'employeesapi.local'
    Namespace = 'employeesapi'
}
