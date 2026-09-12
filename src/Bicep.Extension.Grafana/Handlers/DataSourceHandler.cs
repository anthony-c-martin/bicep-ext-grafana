using Bicep.Local.Extension.Host.Handlers;
using Microsoft.Kiota.Abstractions;
using Soenneker.Grafana.OpenApiClient.Models;

namespace Bicep.Extension.Grafana.Handlers;

public sealed class DataSourceHandler : GrafanaResourceHandlerBase<DataSource, DataSourceIdentifiers>
{
    protected override Task<ResourceResponse> Preview(ResourceRequest request, CancellationToken cancellationToken)
        => Task.FromResult(GetResponse(request));

    protected override Task<ResourceResponse> CreateOrUpdate(ResourceRequest request, CancellationToken cancellationToken)
        => HandleRequest(request, async client =>
        {
            Soenneker.Grafana.OpenApiClient.Models.DataSource? existing = null;
            try
            {
                existing = await client.Datasources.Uid[request.Properties.Uid].GetAsync(
                    cancellationToken: cancellationToken);
            }
            catch (ApiException exception) when (IsNotFound(exception))
            {
            }

            if (existing is null)
            {
                await client.Datasources.PostAsync(
                    new AddDataSourceCommand
                    {
                        Uid = request.Properties.Uid,
                        Name = request.Properties.Name,
                        Type = request.Properties.Type,
                        Url = request.Properties.Url,
                        Access = request.Properties.Access,
                        IsDefault = request.Properties.IsDefault,
                        Database = request.Properties.Database,
                        User = request.Properties.User,
                        BasicAuth = request.Properties.BasicAuth,
                        BasicAuthUser = request.Properties.BasicAuthUser,
                        WithCredentials = request.Properties.WithCredentials,
                        JsonData = new AddDataSourceCommandJsonData
                        {
                            AdditionalData = KiotaJson.ParseObject(request.Properties.JsonDataJson),
                        },
                        SecureJsonData = CreateSecureJsonData(request.Properties.SecureJsonDataJson),
                    },
                    cancellationToken: cancellationToken);
            }
            else
            {
                await client.Datasources.Uid[request.Properties.Uid].PutAsync(
                    new UpdateDataSourceCommand
                    {
                        Uid = request.Properties.Uid,
                        Name = request.Properties.Name,
                        Type = request.Properties.Type,
                        Url = request.Properties.Url,
                        Access = request.Properties.Access,
                        IsDefault = request.Properties.IsDefault,
                        Database = request.Properties.Database,
                        User = request.Properties.User,
                        BasicAuth = request.Properties.BasicAuth,
                        BasicAuthUser = request.Properties.BasicAuthUser,
                        WithCredentials = request.Properties.WithCredentials,
                        Version = existing.Version,
                        JsonData = new UpdateDataSourceCommandJsonData
                        {
                            AdditionalData = KiotaJson.ParseObject(request.Properties.JsonDataJson),
                        },
                        SecureJsonData = CreateUpdateSecureJsonData(request.Properties.SecureJsonDataJson),
                    },
                    cancellationToken: cancellationToken);
            }

            request.Properties.SecureJsonDataJson = null;
            return GetResponse(request);
        });

    protected override DataSourceIdentifiers GetIdentifiers(DataSource properties)
        => new() { Uid = properties.Uid };

    private static AddDataSourceCommandSecureJsonDataProperty? CreateSecureJsonData(
        string? secureJsonData)
        => secureJsonData is null
            ? null
            : new() { AdditionalData = KiotaJson.ParseObject(secureJsonData) };

    private static UpdateDataSourceCommandSecureJsonDataProperty? CreateUpdateSecureJsonData(
        string? secureJsonData)
        => secureJsonData is null
            ? null
            : new() { AdditionalData = KiotaJson.ParseObject(secureJsonData) };
}
