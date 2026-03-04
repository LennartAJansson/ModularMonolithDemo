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
