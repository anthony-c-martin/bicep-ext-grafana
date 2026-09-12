using Azure.Bicep.Types.Concrete;
using Bicep.Local.Extension.Types.Attributes;

namespace Bicep.Extension.Grafana;

public class DashboardIdentifiers
{
    [TypeProperty("The stable Grafana dashboard UID.", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Uid { get; set; }
}

[ResourceType("Dashboard")]
public sealed class Dashboard : DashboardIdentifiers
{
    [TypeProperty("The dashboard title.", ObjectTypePropertyFlags.Required)]
    public required string Title { get; set; }

    [TypeProperty("The UID of the folder that contains the dashboard.")]
    public string? FolderUid { get; set; }

    [TypeProperty("A message describing the dashboard change.")]
    public string? Message { get; set; }

    [TypeProperty("A JSON object containing dashboard fields such as panels, tags, templating, and time settings.", ObjectTypePropertyFlags.Required)]
    public required string DefinitionJson { get; set; }
}

public class FolderIdentifiers
{
    [TypeProperty("The stable Grafana folder UID.", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Uid { get; set; }
}

[ResourceType("Folder")]
public sealed class Folder : FolderIdentifiers
{
    [TypeProperty("The folder title.", ObjectTypePropertyFlags.Required)]
    public required string Title { get; set; }

    [TypeProperty("The folder description.")]
    public string? Description { get; set; }

    [TypeProperty("The parent folder UID. Omit for a root-level folder.")]
    public string? ParentUid { get; set; }
}

public class DataSourceIdentifiers
{
    [TypeProperty("The stable Grafana data source UID.", ObjectTypePropertyFlags.Identifier | ObjectTypePropertyFlags.Required)]
    public required string Uid { get; set; }
}

[ResourceType("DataSource")]
public sealed class DataSource : DataSourceIdentifiers
{
    [TypeProperty("The data source name.", ObjectTypePropertyFlags.Required)]
    public required string Name { get; set; }

    [TypeProperty("The data source plugin type, for example prometheus.", ObjectTypePropertyFlags.Required)]
    public required string Type { get; set; }

    [TypeProperty("The data source URL.")]
    public string? Url { get; set; }

    [TypeProperty("The access mode. Grafana commonly uses proxy.")]
    public string Access { get; set; } = "proxy";

    [TypeProperty("Whether this is the default data source.")]
    public bool IsDefault { get; set; }

    [TypeProperty("The database name.")]
    public string? Database { get; set; }

    [TypeProperty("The user name.")]
    public string? User { get; set; }

    [TypeProperty("Whether basic authentication is enabled.")]
    public bool BasicAuth { get; set; }

    [TypeProperty("The basic authentication user name.")]
    public string? BasicAuthUser { get; set; }

    [TypeProperty("Whether credentials are sent with cross-site requests.")]
    public bool WithCredentials { get; set; }

    [TypeProperty("A JSON object containing plugin-specific non-secret configuration.")]
    public string JsonDataJson { get; set; } = "{}";

    [TypeProperty("A JSON object containing plugin-specific secret configuration.", ObjectTypePropertyFlags.WriteOnly, isSecure: true)]
    public string? SecureJsonDataJson { get; set; }
}

public sealed class Configuration
{
    [TypeProperty("The absolute Grafana URL. The /api suffix is optional.", ObjectTypePropertyFlags.Required)]
    public required string BaseUrl { get; set; }

    [TypeProperty("A Grafana service account token or an Azure Managed Grafana Microsoft Entra access token.", ObjectTypePropertyFlags.Required, isSecure: true)]
    public required string Token { get; set; }
}
