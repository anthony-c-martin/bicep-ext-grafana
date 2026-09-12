using Bicep.Local.Extension.Host.Handlers;
using Microsoft.Kiota.Abstractions.Serialization;
using Soenneker.Grafana.OpenApiClient.Models;

namespace Bicep.Extension.Grafana.Handlers;

public sealed class DashboardHandler : GrafanaResourceHandlerBase<Dashboard, DashboardIdentifiers>
{
    protected override Task<ResourceResponse> Preview(ResourceRequest request, CancellationToken cancellationToken)
        => Task.FromResult(GetResponse(request));

#pragma warning disable CS0618
    protected override Task<ResourceResponse> CreateOrUpdate(ResourceRequest request, CancellationToken cancellationToken)
        => HandleRequest(request, async client =>
        {
            var dashboard = new SaveDashboardCommandDashboard
            {
                AdditionalData = KiotaJson.ParseObject(request.Properties.DefinitionJson),
            };
            dashboard.AdditionalData["uid"] = new UntypedString(request.Properties.Uid);
            dashboard.AdditionalData["title"] = new UntypedString(request.Properties.Title);

            await client.Dashboards.Db.PostAsync(
                new SaveDashboardCommand
                {
                    Dashboard = dashboard,
                    FolderUid = request.Properties.FolderUid,
                    Message = request.Properties.Message,
                    Overwrite = true,
                },
                cancellationToken: cancellationToken);

            return GetResponse(request);
        });
#pragma warning restore CS0618

    protected override DashboardIdentifiers GetIdentifiers(Dashboard properties)
        => new() { Uid = properties.Uid };
}
