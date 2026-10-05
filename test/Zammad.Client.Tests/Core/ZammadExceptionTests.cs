using System.Net;
using Zammad.Client.Core;
using Zammad.Client.Resources;

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

    [Test]
    public async Task Message_WithExplicitError()
    {
        // PUT /ticket_merge reports missing tickets with 200 OK
        const string content = "{\"result\":\"failed\",\"message\":\"The target ticket number could not be found.\"}";
        var exception = new ZammadException(
            new HttpRequestMessage(HttpMethod.Put, "http://localhost/api/v1/ticket_merge/1/123"),
            Response(HttpStatusCode.OK),
            content,
            "The target ticket number could not be found."
        );

        await Assert
            .That(exception.Message)
            .IsEqualTo(
                "PUT /api/v1/ticket_merge/1/123 failed with 200 Reason: The target ticket number could not be found."
            );
        await Assert.That(exception.Error).IsEqualTo("The target ticket number could not be found.");
        await Assert.That(exception.Content).IsEqualTo(content);
    }

    [Test]
    public async Task Message_WithBooleanError()
    {
        // The mass update endpoints answer {"error": true, "ticket_id": ...}
        const string content = "{\"error\":true,\"ticket_id\":42}";
        var exception = new ZammadException(Request, Response(HttpStatusCode.UnprocessableEntity), content);

        await Assert.That(exception.Error).IsNull();
        await Assert
            .That(exception.Message)
            .IsEqualTo($"DELETE /api/v1/organizations/1 failed with 422 Reason: {content}");
        await Assert.That(exception.GetFailedTicketId()).IsEqualTo(new TicketId(42));
        await Assert.That(exception.GetBlockingTicketIds()).IsEmpty();
    }

    [Test]
    public async Task GetBlockingTicketIds()
    {
        var exception = new ZammadException(
            Request,
            Response(HttpStatusCode.UnprocessableEntity),
            "{\"error\":\"Macro group restrictions do not cover all tickets\",\"blocking_tickets\":[3,5]}"
        );

        await Assert.That(exception.Error).IsEqualTo("Macro group restrictions do not cover all tickets");
        await Assert.That(exception.GetBlockingTicketIds()).IsEquivalentTo([new TicketId(3), new TicketId(5)]);
        await Assert.That(exception.GetFailedTicketId()).IsNull();
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    [Arguments("not json")]
    [Arguments("[1,2]")]
    [Arguments("{\"ticket_id\":\"1\",\"blocking_tickets\":\"1\"}")]
    public async Task TicketIds_MissingOrInvalid(string? content)
    {
        var exception = new ZammadException(Request, Response(HttpStatusCode.UnprocessableEntity), content);

        await Assert.That(exception.GetFailedTicketId()).IsNull();
        await Assert.That(exception.GetBlockingTicketIds()).IsEmpty();
    }
}
