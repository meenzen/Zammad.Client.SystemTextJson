using System.Net;
using System.Text;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client.Tests;

public class HttpResponseParserTest
{
    public const string TicketSerialized =
        "{\"id\":1,\"group_id\":1,\"priority_id\":2,\"state_id\":1,\"organization_id\":1,\"number\":\"96001\",\"title\":\"Welcome to Zammad!\",\"owner_id\":1,\"customer_id\":2,\"note\":null,\"first_response_at\":null,\"first_response_escalation_at\":null,\"first_response_in_min\":null,\"first_response_diff_in_min\":null,\"close_at\":null,\"close_escalation_at\":null,\"close_in_min\":null,\"close_diff_in_min\":null,\"update_escalation_at\":null,\"update_in_min\":null,\"update_diff_in_min\":null,\"last_contact_at\":\"2017-09-25T14:50:50.946Z\",\"last_contact_agent_at\":null,\"last_contact_customer_at\":\"2017-09-25T14:50:50.946Z\",\"last_owner_update_at\":null,\"create_article_type_id\":5,\"create_article_sender_id\":2,\"article_count\":1,\"escalation_at\":null,\"pending_time\":null,\"type\":null,\"time_unit\":null,\"preferences\":{},\"updated_by_id\":3,\"created_by_id\":2,\"created_at\":\"2017-09-25T14:50:50.910Z\",\"updated_at\":\"2017-09-30T14:47:55.177Z\"}";

    [Test]
    public async Task ParseSuccessStatus_Success_Test()
    {
        var httpResponse = CreateTestResponse();

        var success = await httpResponse.ParseAsync<bool>();

        await Assert.That(success).IsTrue();
    }

    [Test]
    public async Task ParseStatusCode_Success_Test()
    {
        var httpResponse = CreateTestResponse();

        var httpStatusCode = await httpResponse.ParseAsync<HttpStatusCode>();

        await Assert.That(httpStatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task ParseJsonContentAsync_Success_TestAsync()
    {
        var httpResponse = CreateTestResponse();

        var ticket = await httpResponse.ParseAsync<Ticket>();

        await Assert.That(ticket).IsNotNull();
        await Assert.That(ticket.Id).IsEqualTo(new TicketId(1));
        await Assert.That(ticket.GroupId).IsEqualTo(new GroupId(1));
        await Assert.That(ticket.PriorityId).IsEqualTo(new PriorityId(2));
        await Assert.That(ticket.StateId).IsEqualTo(new StateId(1));
        await Assert.That(ticket.OrganizationId).IsEqualTo(new OrganizationId(1));
        await Assert.That(ticket.Number).IsEqualTo("96001");
        await Assert.That(ticket.Title).IsEqualTo("Welcome to Zammad!");
        await Assert.That(ticket.OwnerId).IsEqualTo(new UserId(1));
        await Assert.That(ticket.CustomerId).IsEqualTo(new UserId(2));
        await Assert.That(ticket.Note).IsNull();
        await Assert.That(ticket.FirstResponseAt).IsNull();
        await Assert.That(ticket.FirstResponseEscalationAt).IsNull();
        await Assert.That(ticket.FirstResponseInMin).IsNull();
        await Assert.That(ticket.FirstResponseDiffInMin).IsNull();
        await Assert.That(ticket.CloseAt).IsNull();
        await Assert.That(ticket.CloseEscalationAt).IsNull();
        await Assert.That(ticket.CloseInMin).IsNull();
        await Assert.That(ticket.CloseDiffInMin).IsNull();
        await Assert.That(ticket.UpdateEscalationAt).IsNull();
        await Assert.That(ticket.UpdateInMin).IsNull();
        await Assert.That(ticket.UpdateDiffInMin).IsNull();
        await Assert.That(ticket.LastContactAt).IsEqualTo(DateTimeOffset.Parse("2017-09-25T14:50:50.946Z"));
        await Assert.That(ticket.LastContactAgentAt).IsNull();
        await Assert.That(ticket.LastContactCustomerAt).IsEqualTo(DateTimeOffset.Parse("2017-09-25T14:50:50.946Z"));
        await Assert.That(ticket.LastOwnerUpdateAt).IsNull();
        await Assert.That(ticket.CreateArticleTypeId).IsEqualTo(new ArticleTypeId(5));
        await Assert.That(ticket.CreateArticleSenderId).IsEqualTo(new UserId(2));
        await Assert.That(ticket.ArticleCount).IsEqualTo(1);
        await Assert.That(ticket.EscalationAt).IsNull();
        await Assert.That(ticket.PendingTime).IsNull();
        await Assert.That(ticket.Type).IsNull();
        await Assert.That(ticket.TimeUnit).IsNull();
        await Assert.That(ticket.Preferences).IsEquivalentTo(new Dictionary<string, object>());
        await Assert.That(ticket.UpdatedById).IsEqualTo(new UserId(3));
        await Assert.That(ticket.CreatedById).IsEqualTo(new UserId(2));
        await Assert.That(ticket.CreatedAt).IsEqualTo(DateTimeOffset.Parse("2017-09-25T14:50:50.910Z"));
        await Assert.That(ticket.UpdatedAt).IsEqualTo(DateTimeOffset.Parse("2017-09-30T14:47:55.177Z"));
    }

    [Test]
    public async Task ParseStreamContentAsync_Success_TestAsync(CancellationToken cancellationToken)
    {
        var httpResponse = CreateTestResponse();
        await using var ticketStream = await httpResponse.ParseAsync<Stream>();

        await Assert.That(ticketStream).IsNotNull();

        using var reader = new StreamReader(ticketStream);
        var ticketString = await reader.ReadToEndAsync(cancellationToken);
        await Assert.That(ticketString).IsEqualTo(TicketSerialized);
    }

    private HttpResponseMessage CreateTestResponse()
    {
        return new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(TicketSerialized, Encoding.UTF8, "application/json"),
        };
    }
}
