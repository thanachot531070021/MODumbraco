using System.Net.Http.Headers;

namespace MODumbraco.Ckan;

/// <summary>
/// Sends request bodies with a Content-Length instead of chunked transfer encoding.
/// Pkg.Umbraco.CKAN posts with PostAsJsonAsync (chunked), and CKAN behind uWSGI reads a chunked
/// body as empty, so every create/patch fails with "Missing value".
/// </summary>
public sealed class BufferedRequestContentHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Content is { } content && content.Headers.ContentLength is null)
        {
            byte[] body = await content.ReadAsByteArrayAsync(cancellationToken);
            var buffered = new ByteArrayContent(body);
            foreach (KeyValuePair<string, IEnumerable<string>> header in content.Headers)
            {
                buffered.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            buffered.Headers.ContentLength = body.Length;
            request.Content = buffered;
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
