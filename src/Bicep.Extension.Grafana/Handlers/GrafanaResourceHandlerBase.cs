using System.Net.Http.Headers;
using Bicep.Local.Extension.Host.Handlers;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Soenneker.Grafana.OpenApiClient;
using Soenneker.Grafana.OpenApiClient.Models;

namespace Bicep.Extension.Grafana.Handlers;

public abstract class GrafanaResourceHandlerBase<TProperties, TIdentifiers>
    : TypedResourceHandler<TProperties, TIdentifiers, Configuration>
    where TProperties : class
    where TIdentifiers : class
{
    internal HttpMessageHandler? MessageHandlerOverride { get; set; }

    protected async Task<ResourceResponse> HandleRequest(
        ResourceBase resource,
        Func<GrafanaOpenApiClient, Task<ResourceResponse>> execute)
    {
        using var httpClient = MessageHandlerOverride is { } handler
            ? new HttpClient(handler, disposeHandler: false)
            : new HttpClient();

        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", resource.Config!.Token);
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("bicep-ext-grafana/1.0");

        var adapter = new HttpClientRequestAdapter(
            new AnonymousAuthenticationProvider(),
            httpClient: httpClient)
        {
            BaseUrl = NormalizeApiUrl(resource.Config.BaseUrl),
        };

        var client = new GrafanaOpenApiClient(adapter);

        try
        {
            return await execute(client);
        }
        catch (ResourceErrorException)
        {
            throw;
        }
        catch (ApiException exception)
        {
            var message = exception is ErrorResponseBody error && !string.IsNullOrWhiteSpace(error.Message)
                ? error.Message
                : exception.Message;

            throw new ResourceErrorException(
                "GrafanaApiError",
                $"Grafana API request failed with status {exception.ResponseStatusCode}: {message}");
        }
        catch (HttpRequestException exception)
        {
            throw new ResourceErrorException(
                "GrafanaConnectionError",
                $"Unable to communicate with Grafana: {exception.Message}");
        }
        catch (System.Text.Json.JsonException exception)
        {
            throw new ResourceErrorException("InvalidJson", exception.Message);
        }
    }

    protected static bool IsNotFound(ApiException exception) => exception.ResponseStatusCode == 404;

    private static string NormalizeApiUrl(string baseUrl)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ResourceErrorException(
                "InvalidConfiguration",
                "Grafana baseUrl must be an absolute HTTP or HTTPS URL.");
        }

        var normalized = baseUrl.TrimEnd('/');
        return normalized.EndsWith("/api", StringComparison.OrdinalIgnoreCase)
            ? normalized
            : $"{normalized}/api";
    }
}
