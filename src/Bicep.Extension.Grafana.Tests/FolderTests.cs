using System.Net;
using System.Text.Json;
using Bicep.Extension.Grafana.Handlers;

namespace Bicep.Extension.Grafana.Tests;

[TestClass]
public sealed class FolderTests
{
    [TestMethod]
    public async Task Creates_folder_when_uid_is_absent()
    {
        var mock = new MockHttpMessageHandler((request, _) =>
            request.Method == HttpMethod.Get
                ? MockHttpMessageHandler.Json(HttpStatusCode.NotFound, """{"message":"Folder not found"}""")
                : MockHttpMessageHandler.Json(
                    HttpStatusCode.OK,
                    """{"id":1,"uid":"operations","title":"Operations","version":1}"""));
        var handler = new FolderHandler { MessageHandlerOverride = mock };

        await HandlerHarness.CreateOrUpdateAsync(handler, "Folder", new
        {
            uid = "operations",
            title = "Operations",
            description = "Operational dashboards",
        });

        var create = mock.Requests.Single(request => request.Method == HttpMethod.Post);
        Assert.AreEqual("/api/folders", create.Uri.AbsolutePath);
        var body = JsonSerializer.Deserialize<JsonElement>(create.Body);
        Assert.AreEqual("operations", body.GetProperty("uid").GetString());
        Assert.AreEqual("Operations", body.GetProperty("title").GetString());
    }

    [TestMethod]
    public async Task Updates_existing_folder()
    {
        var mock = new MockHttpMessageHandler((request, _) =>
            request.Method == HttpMethod.Get
                ? MockHttpMessageHandler.Json(
                    HttpStatusCode.OK,
                    """{"id":1,"uid":"operations","title":"Old","version":3}""")
                : MockHttpMessageHandler.Json(
                    HttpStatusCode.OK,
                    """{"id":1,"uid":"operations","title":"Operations","version":4}"""));
        var handler = new FolderHandler { MessageHandlerOverride = mock };

        await HandlerHarness.CreateOrUpdateAsync(handler, "Folder", new
        {
            uid = "operations",
            title = "Operations",
        });

        var update = mock.Requests.Single(request => request.Method == HttpMethod.Put);
        Assert.AreEqual("/api/folders/operations", update.Uri.AbsolutePath);
        var body = JsonSerializer.Deserialize<JsonElement>(update.Body);
        Assert.AreEqual(3, body.GetProperty("version").GetInt64());
    }
}
