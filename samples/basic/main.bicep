targetScope = 'local'

@description('The absolute URL of the Grafana instance.')
param grafanaUrl string

@secure()
@description('A Grafana service account token with dashboard, folder, and data source permissions.')
param grafanaToken string

@description('The URL of the Prometheus server to configure.')
param prometheusUrl string

extension grafana with {
  baseUrl: grafanaUrl
  token: grafanaToken
}

resource operations 'Folder' = {
  uid: 'operations'
  title: 'Operations'
  description: 'Operational dashboards managed by Bicep'
}

resource prometheus 'DataSource' = {
  uid: 'prometheus'
  name: 'Prometheus'
  type: 'prometheus'
  url: prometheusUrl
  access: 'proxy'
  isDefault: true
  jsonDataJson: string({
    httpMethod: 'POST'
    timeInterval: '30s'
  })
}

resource overview 'Dashboard' = {
  uid: 'operations-overview'
  title: 'Operations overview'
  folderUid: operations.uid
  message: 'Managed by Bicep'
  definitionJson: string({
    tags: [
      'bicep'
      'operations'
    ]
    timezone: 'browser'
    schemaVersion: 41
    refresh: '30s'
    time: {
      from: 'now-6h'
      to: 'now'
    }
    panels: [
      {
        id: 1
        type: 'timeseries'
        title: 'Prometheus up'
        datasource: {
          type: 'prometheus'
          uid: prometheus.uid
        }
        targets: [
          {
            refId: 'A'
            expr: 'up'
          }
        ]
        gridPos: {
          h: 8
          w: 24
          x: 0
          y: 0
        }
      }
    ]
  })
}

output dashboardUid string = overview.uid
