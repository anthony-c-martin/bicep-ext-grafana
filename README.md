# Grafana Bicep Extension

This extension manages Grafana data-plane resources from Bicep by using Grafana's HTTP API through
the Kiota-generated `Soenneker.Grafana.OpenApiClient` C# SDK. It does not create or configure an
Azure Managed Grafana workspace or any other control-plane resource.

## Resources

| Resource | Identity | Managed settings |
| --- | --- | --- |
| `Folder` | `uid` | Title, description, and parent folder |
| `DataSource` | `uid` | Core connection settings, plugin JSON, and secure plugin JSON |
| `Dashboard` | `uid` | Title, folder, change message, and arbitrary dashboard JSON |

All resources are upserted. Arbitrary JSON is supplied through `definitionJson`, `jsonDataJson`, and
`secureJsonDataJson`; use Bicep's `string()` function to serialize object literals as shown in the
sample. Data source `secureJsonDataJson` is write-only and is omitted from deployment outputs.

## Usage

1. Set `GRAFANA_URL`, `GRAFANA_SERVICE_ACCOUNT_TOKEN`, and `PROMETHEUS_URL`.
1. Open the `samples` folder in VS Code and select `basic/main.bicepparam`.
1. Launch the [Deploy Pane](https://github.com/Azure/bicep/blob/main/docs/experimental/deploy-ui.md) to run the deployment.

The Grafana URL may be supplied with or without the `/api` suffix. The token can be either a Grafana
service account token or an Azure Managed Grafana Microsoft Entra token, and must have permission to
create and update the requested resources.

### Azure Managed Grafana

The Azure Managed Grafana sample deploys a
[workspace](./samples/azure-managed-grafana/managed-grafana.bicep), then uses its generated endpoint
with the [Grafana data-plane extension](./samples/azure-managed-grafana/main.bicep) to create a
folder and dashboard.

Bicep extension configuration must be known at the start of a deployment, while Azure generates the
workspace endpoint during deployment. The sample therefore uses two stages:

```sh
az account set --subscription '<subscription-id>'
az group create --name bicep-ext-grafana-sample --location eastus

export GRAFANA_URL=$(az deployment group create \
  --resource-group bicep-ext-grafana-sample \
  --template-file ./samples/azure-managed-grafana/managed-grafana.bicep \
  --parameters grafanaName='<globally-unique-name>' \
  --query properties.outputs.endpoint.value \
  --output tsv)

export GRAFANA_TOKEN=$(az account get-access-token \
  --resource https://dashboard.azure.com \
  --query accessToken \
  --output tsv)

bicep local-deploy ./samples/azure-managed-grafana/main.bicepparam
```

The workspace enables `creatorCanAdmin`, so the identity creating it receives Grafana Admin access.
Role propagation can take several minutes; refresh `GRAFANA_TOKEN` and retry if the first data-plane
request gets an authorization error.

> [!NOTE]
> Extension binary packages are not signed on a Mac. If you see the following error, you will need to manually sign the extension package:
> 
> `Failed to launch provider: Failed to connect to provider .../extensions$grafana/.../extension.bin`
> 
> To work around it, run the following in a terminal window, using the path from the error message:
> 
> `codesign -s - '<path-from-the-error>/extension.bin'`

## Build + Test Locally

### Rebuild the extension
These commands publish the extension to the local file system, and updates the sample bicepconfig to point to the local extension.
```sh
./scripts/publish.sh ./bin/bicep-ext-grafana
jq '.extensions.grafana="../bin/bicep-ext-grafana"' ./samples/bicepconfig.json > ./samples/bicepconfig.new.json
mv ./samples/bicepconfig.new.json ./samples/bicepconfig.json
```

### Test the extension
Run the deployment.
```sh
bicep local-deploy ./samples/basic/main.bicepparam
```

To enable verbose tracing, run the following beforehand.
```sh
export BICEP_TRACING_ENABLED=true
```

## Releasing

Releases are cut manually so that versioning stays under explicit control — pushing to `main` does
not publish anything. To release, run the **Release** workflow from the Actions tab (or with
`gh workflow run release.yml -f version=0.2.0`) and supply the exact version to publish.

The workflow validates the version, builds and tests, publishes
`br:ghcr.io/anthony-c-martin/bicep-ext-grafana:<version>` and then pushes a `v`-prefixed git tag
(`v0.2.0`) and GitHub Release. Note that the OCI artifact is tagged with the bare version, while the
git tag carries the `v` prefix. The workflow refuses to run if the tag already exists, so published
versions are never replaced; releases must be cut from `main`.

The version supplied to the workflow is stamped into the binary via `-p:Version=`, and is what the
extension reports to Bicep. Local builds use the placeholder `0.0.1-dev` version from
[`Bicep.Extension.Grafana.csproj`](./src/Bicep.Extension.Grafana/Bicep.Extension.Grafana.csproj).

To pick up a new version after publishing, update your `bicepconfig.json` to reference the new tag.

## Building other extensions

This repo is also intended to demonstrate how to build + publish an end-to-end Bicep extension in C#. Feel free to copy, rename and modify it to prototype building an extension to extend other services.