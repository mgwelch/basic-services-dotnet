using Flurl.Http.Testing;

namespace JohnsonControls.Metasys.BasicServices;

public static class HttpCallAssertionExtensions
{
    /// <summary>
    /// Asserts that the specified content was passed as a utf8 encoded byte array
    /// </summary>
    /// <param name="assertion"></param>
    /// <param name="expectedContent"></param>
    /// <returns></returns>
    public static HttpCallAssertion WithCapturedByteArrayContent(this HttpCallAssertion assertion, string expectedContent)
    {
        return assertion.With(call =>
        {
            var actualContent = ((CapturedByteArrayContent)call.Request.Content).Content;
            return actualContent == expectedContent;
        });
    }
}
