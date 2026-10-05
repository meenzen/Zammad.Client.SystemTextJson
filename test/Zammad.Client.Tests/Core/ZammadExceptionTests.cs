using System.Net;
using Zammad.Client.Core;

namespace Zammad.Client.Tests.Core;

public class ZammadExceptionTests
{
    private static readonly HttpRequestMessage Request = new(
        HttpMethod.Delete,
        "http://localhost/api/v1/organizations/1"
    );

    private static HttpResponseMessage Response(HttpStatusCode code) => new(code) { ReasonPhrase = "Reason" };

    [Test]
    public async Task Message_WithoutContent()
    {
        var exception = new ZammadException(Request, Response(HttpStatusCode.UnprocessableEntity));

        await Assert.That(exception.Message).IsEqualTo("DELETE /api/v1/organizations/1 failed with 422 Reason");
        await Assert.That(exception.Content).IsNull();
        await Assert.That(exception.Error).IsNull();
    }

    [Test]
    public async Task Message_WithJsonError()
    {
        const string content =
            "{\"error\":\"Can't delete, object has references.\",\"error_human\":\"Can't delete, object has references.\"}";
        var exception = new ZammadException(Request, Response(HttpStatusCode.UnprocessableEntity), content);

        await Assert
            .That(exception.Message)
            .IsEqualTo("DELETE /api/v1/organizations/1 failed with 422 Reason: Can't delete, object has references.");
        await Assert.That(exception.Content).IsEqualTo(content);
        await Assert.That(exception.Error).IsEqualTo("Can't delete, object has references.");
    }

    [Test]
    public async Task Message_PrefersHumanError()
    {
        var exception = new ZammadException(
            Request,
            Response(HttpStatusCode.InternalServerError),
            "{\"error\":\"technical\",\"error_human\":\"human\"}"
        );

        await Assert.That(exception.Error).IsEqualTo("human");
    }

    [Test]
    public async Task Message_FallsBackToErrorWhenHumanErrorIsBlank()
    {
        var exception = new ZammadException(
            Request,
            Response(HttpStatusCode.UnprocessableEntity),
            "{\"error\":\"technical\",\"error_human\":\" \"}"
        );

        await Assert.That(exception.Error).IsEqualTo("technical");
        await Assert
            .That(exception.Message)
            .IsEqualTo("DELETE /api/v1/organizations/1 failed with 422 Reason: technical");
    }

    [Test]
    public async Task Message_WithNonJsonContent()
    {
        var exception = new ZammadException(Request, Response(HttpStatusCode.BadGateway), "bad gateway");

        await Assert
            .That(exception.Message)
            .IsEqualTo("DELETE /api/v1/organizations/1 failed with 502 Reason: bad gateway");
        await Assert.That(exception.Error).IsNull();
    }

    [Test]
    public async Task Message_TruncatesLongContent()
    {
        var exception = new ZammadException(Request, Response(HttpStatusCode.BadGateway), new string('x', 5000));

        await Assert.That(exception.Message.Length).IsLessThan(1200);
        await Assert.That(exception.Content!.Length).IsEqualTo(5000);
    }
}
