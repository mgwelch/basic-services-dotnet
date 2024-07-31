using Flurl.Http.Testing;

namespace JohnsonControls.Metasys.BasicServices;

public static class HttpCallAssertionExtensions
{
    public static HttpCallAssertion WithCapturedByteArrayContent(this HttpCallAssertion assertion, string expectedContent)
    {
        return assertion.With(call =>
        {
            var actualContent = ((CapturedByteArrayContent)call.Request.Content).Content;
            return actualContent == expectedContent;
        });
    }
}
