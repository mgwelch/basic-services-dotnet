using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http.Testing;

#nullable enable

namespace JohnsonControls.Metasys.BasicServices;

/// <summary>Content provided by a byte array</summary>
/// <remarks>
/// Works just like a ByteArrayContent but in testing mode (HttpTest.Current isn't null) it retains the
/// contents of the request as a string for verification purposes. In production it does not do that.
///
/// The content is assumed to be a utf8 encoded string.
/// </remarks>
public class CapturedByteArrayContent : HttpContent
{
    private readonly byte[] payload;

    /// <summary>
    /// Create an HttpContent instance from the specified payload.
    /// </summary>
    /// <param name="payload"></param>
    public CapturedByteArrayContent(byte[] payload)
    {
        this.payload = payload;
        if (HttpTest.Current != null)
        {
            Content = Encoding.UTF8.GetString(payload);
        }
    }

    /// <inheritdoc/>
    protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
    {
        try
        {
            return stream.WriteAsync(payload, 0, payload.Length);
        }
        finally
        {
            Array.Clear(payload, 0, payload.Length);
        }

    }

    /// <summary>
    /// Returns the decoded byte array.
    /// </summary>
    /// <remarks>The byte array is assumed to be utf8 encoded string.
    /// <para>This will always be null in production.</para>
    /// </remarks>
    public string? Content { get; private set; }

    /// <inheritdoc/>
    protected override bool TryComputeLength(out long length)
    {
        length = payload.Length;
        return true;
    }


}
