using System.Net;
using System.Text.Json;
using Bicep.Extension.Grafana.Handlers;

namespace Bicep.Extension.Grafana.Tests;

[TestClass]
public sealed class DataSourceTests
{
    [TestMethod]
    public async Task Creates_data_source_and_redacts_secrets()
    {
        var mock = new MockHttpMessageHandler((request, _) =>
            request.Method == HttpMethod.Get
                ? MockHttpMessageHandler.Json(HttpStatusCode.NotFound, """{"message":"Data source not found"}""")
                : MockHttpMessageHandler.Json(
                    HttpStatusCode.OK,
                    """{"datasource":{"uid":"prometheus"},"id":1,"message":"Datasource added","name":"Prometheus"}"""));
        var handler = new DataSourceHandler { MessageHandlerOverride = mock };

        var response = await HandlerHarness.CreateOrUpdateAsync(handler, "DataSource", new
        {
            uid = "prometheus",
            name = "Prometheus",
            type = "prometheus",
            url = "https://prometheus.example.com",
            jsonDataJson = """{"httpMethod":"POST","timeInterval":"30s"}""",
            secureJsonDataJson = """{"basicAuthPassword":"secret"}""",
        });

        var create = mock.Requests.Single(request => request.Method == HttpMethod.Post);
        Assert.AreEqual("/api/datasources", create.Uri.AbsolutePath);
        var body = JsonSerializer.Deserialize<JsonElement>(create.Body);
        Assert.AreEqual("prometheus", body.GetProperty("uid").GetString());
        Assert.AreEqual(
            "POST",
            body.GetProperty("jsonData").GetProperty("httpMethod").GetString());
        Assert.AreEqual(
            "secret",
            body.GetProperty("secureJsonData").GetProperty("basicAuthPassword").GetString());
        Assert.IsFalse(response.ResourceProperties().TryGetProperty("secureJsonDataJson", out _));
    }

    [TestMethod]
    public async Task Updates_existing_data_source_with_version()
    {
        var mock = new MockHttpMessageHandler((request, _) =>
            request.Method == HttpMethod.Get
                ? MockHttpMessageHandler.Json(
                    HttpStatusCode.OK,
                    """{"id":1,"uid":"prometheus","name":"Prometheus","type":"prometheus","version":7}""")
                : MockHttpMessageHandler.Json(
                    HttpStatusCode.OK,
                    """{"datasource":{"uid":"prometheus"},"id":1,"message":"Datasource updated","name":"Prometheus"}"""));
        var handler = new DataSourceHandler { MessageHandlerOverride = mock };

        await HandlerHarness.CreateOrUpdateAsync(handler, "DataSource", new
        {
            uid = "prometheus",
            name = "Prometheus",
            type = "prometheus",
            url = "https://prometheus.example.com",
        });

        var update = mock.Requests.Single(request => request.Method == HttpMethod.Put);
        Assert.AreEqual("/api/datasources/uid/prometheus", update.Uri.AbsolutePath);
        var body = JsonSerializer.Deserialize<JsonElement>(update.Body);
        Assert.AreEqual(7, body.GetProperty("version").GetInt64());
    }
}
