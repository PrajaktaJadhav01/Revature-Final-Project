var builder = WebApplication.CreateBuilder(args);

// Configure a named HttpClient that points to the Customer API.
builder.Services.AddHttpClient("CustomerApi", client =>
{
    client.BaseAddress = new Uri("http://localhost:5292");
});

var app = builder.Build();

app.MapGet("/", () => "Api Gateway running. Use /customer/...");

app.Map("/customer/{**catchAll}", async (HttpContext http, IHttpClientFactory factory) =>
{
    var targetPath = http.Request.Path.Value ?? string.Empty;
    var forwardPath = targetPath.ReplaceFirst("/customer", "/api/v1/customer");

    // Copy the incoming request into a new outgoing request
    var outgoing = new HttpRequestMessage(new HttpMethod(http.Request.Method), forwardPath + http.Request.QueryString);

    if (http.Request.ContentLength > 0)
    {
        outgoing.Content = new StreamContent(http.Request.Body);
        if (http.Request.ContentType is not null)
            outgoing.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(http.Request.ContentType);
    }

    foreach (var header in http.Request.Headers)
    {
        if (!outgoing.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) && outgoing.Content is not null)
        {
            outgoing.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }
    }

    var client = factory.CreateClient("CustomerApi");
    using var response = await client.SendAsync(outgoing, HttpCompletionOption.ResponseHeadersRead, http.RequestAborted);

    http.Response.StatusCode = (int)response.StatusCode;

    foreach (var header in response.Headers)
        http.Response.Headers[header.Key] = header.Value.ToArray();

    foreach (var header in response.Content.Headers)
        http.Response.Headers[header.Key] = header.Value.ToArray();

    http.Response.Headers.Remove("transfer-encoding");

    await response.Content.CopyToAsync(http.Response.Body);
});

app.Run();

static class StringExtensions
{
    public static string ReplaceFirst(this string text, string search, string replace)
    {
        var pos = text.IndexOf(search, StringComparison.OrdinalIgnoreCase);
        if (pos < 0)
            return text;
        return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
    }
}
