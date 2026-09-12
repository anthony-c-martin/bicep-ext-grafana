using System.Net;
using System.Text.Json;
using Bicep.Extension.Grafana.Handlers;

namespace Bicep.Extension.Grafana.Tests;

[TestClass]
public sealed class DashboardTests
{
    [TestMethod]
    public async Task Upserts_dashboard_with_uid_and_definition()
    {
        var mock = new MockHttpMessageHandler((_, _) =>
            MockHttpMessageHandler.Json(
                HttpStatusCode.OK,
                """{"id":1,"slug":"platform","status":"success","uid":"platform"}"""));
        var handler = new DashboardHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "Dashboard", new
        {
            uid = "platform",
            title = "Platform",
            folderUid = "operations",
            definitionJson =
                """{"tags":["managed-by-bicep"],"panels":[],"refresh":"30s"}""",
        });

        Assert.IsNull(response.ErrorData);
        var request = mock.Requests.Single();
        Assert.AreEqual(HttpMethod.Post, request.Method);
        Assert.AreEqual("/api/dashboards/db", request.Uri.AbsolutePath);
        Assert.AreEqual("Bearer", request.AuthorizationScheme);

        var body = JsonSerializer.Deserialize<JsonElement>(request.Body);
        Assert.AreEqual("platform", body.GetProperty("dashboard").GetProperty("uid").GetString());
        Assert.AreEqual("Platform", body.GetProperty("dashboard").GetProperty("title").GetString());
        Assert.AreEqual("30s", body.GetProperty("dashboard").GetProperty("refresh").GetString());
        Assert.IsTrue(body.GetProperty("overwrite").GetBoolean());
        Assert.AreEqual("operations", body.GetProperty("folderUid").GetString());
    }
}
