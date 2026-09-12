using Bicep.Local.Extension.Host.Handlers;
using Microsoft.Kiota.Abstractions;
using Soenneker.Grafana.OpenApiClient.Models;

namespace Bicep.Extension.Grafana.Handlers;

public sealed class FolderHandler : GrafanaResourceHandlerBase<Folder, FolderIdentifiers>
{
    protected override Task<ResourceResponse> Preview(ResourceRequest request, CancellationToken cancellationToken)
        => Task.FromResult(GetResponse(request));

#pragma warning disable CS0618
    protected override Task<ResourceResponse> CreateOrUpdate(ResourceRequest request, CancellationToken cancellationToken)
        => HandleRequest(request, async client =>
        {
            Soenneker.Grafana.OpenApiClient.Models.Folder? existing = null;
            try
            {
                existing = await client.Folders[request.Properties.Uid].GetAsync(cancellationToken: cancellationToken);
            }
            catch (ApiException exception) when (IsNotFound(exception))
            {
            }

            if (existing is null)
            {
                await client.Folders.PostAsync(
                    new CreateFolderCommand
                    {
                        Uid = request.Properties.Uid,
                        Title = request.Properties.Title,
                        Description = request.Properties.Description,
                        ParentUid = request.Properties.ParentUid,
                    },
                    cancellationToken: cancellationToken);
            }
            else
            {
                await client.Folders[request.Properties.Uid].PutAsync(
                    new UpdateFolderCommand
                    {
                        Title = request.Properties.Title,
                        Description = request.Properties.Description,
                        Overwrite = true,
                        Version = existing.Version,
                    },
                    cancellationToken: cancellationToken);

                if (!string.Equals(existing.ParentUid, request.Properties.ParentUid, StringComparison.Ordinal))
                {
                    await client.Folders[request.Properties.Uid].Move.PostAsync(
                        new MoveFolderCommand { ParentUid = request.Properties.ParentUid },
                        cancellationToken: cancellationToken);
                }
            }

            return GetResponse(request);
        });
#pragma warning restore CS0618

    protected override FolderIdentifiers GetIdentifiers(Folder properties)
        => new() { Uid = properties.Uid };
}
