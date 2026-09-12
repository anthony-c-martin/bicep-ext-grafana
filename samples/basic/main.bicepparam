using 'main.bicep'

// export GRAFANA_URL='https://grafana.example.com'
// export GRAFANA_SERVICE_ACCOUNT_TOKEN='<service-account-token>'
// export PROMETHEUS_URL='https://prometheus.example.com'
param grafanaUrl = readEnvironmentVariable('GRAFANA_URL')
param grafanaToken = readEnvironmentVariable('GRAFANA_SERVICE_ACCOUNT_TOKEN')
param prometheusUrl = readEnvironmentVariable('PROMETHEUS_URL')
