using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client.Tests;

public class HttpResponseParserTest
{
    public const string TicketSerialized =
        "{\"id\":1,\"group_id\":1,\"priority_id\":2,\"state_id\":1,\"organization_id\":1,\"number\":\"96001\",\"title\":\"Welcome to Zammad!\",\"owner_id\":1,\"customer_id\":2,\"note\":null,\"first_response_at\":null,\"first_response_escalation_at\":null,\"first_response_in_min\":null,\"first_response_diff_in_min\":null,\"close_at\":null,\"close_escalation_at\":null,\"close_in_min\":null,\"close_diff_in_min\":null,\"update_escalation_at\":null,\"update_in_min\":null,\"update_diff_in_min\":null,\"last_contact_at\":\"2017-09-25T14:50:50.946Z\",\"last_contact_agent_at\":null,\"last_contact_customer_at\":\"2017-09-25T14:50:50.946Z\",\"last_owner_update_at\":null,\"create_article_type_id\":5,\"create_article_sender_id\":2,\"article_count\":1,\"escalation_at\":null,\"pending_time\":null,\"type\":null,\"time_unit\":null,\"preferences\":{},\"updated_by_id\":3,\"created_by_id\":2,\"created_at\":\"2017-09-25T14:50:50.910Z\",\"updated_at\":\"2017-09-30T14:47:55.177Z\"}";

    [Fact]
    public async Task ParseSuccessStatus_Success_Test()
    {
        var httpResponse = CreateTestResponse();

        var success = await httpResponse.ParseAsync<bool>();

        Assert.True(success);
    }

    [Fact]
    public async Task ParseStatusCode_Success_Test()
    {
        var httpResponse = CreateTestResponse();

        var httpStatusCode = await httpResponse.ParseAsync<HttpStatusCode>();

        Assert.Equal(HttpStatusCode.OK, httpStatusCode);
    }

    [Fact]
    public async Task ParseJsonContentAsync_Success_TestAsync()
    {
        var httpResponse = CreateTestResponse();

        var ticket = await httpResponse.ParseAsync<Ticket>();

        Assert.NotNull(ticket);
        Assert.Equal(new TicketId(1), ticket.Id);
        Assert.Equal(new GroupId(1), ticket.GroupId);
        Assert.Equal(new PriorityId(2), ticket.PriorityId);
        Assert.Equal(new StateId(1), ticket.StateId);
        Assert.Equal(new OrganizationId(1), ticket.OrganizationId);
        Assert.Equal("96001", ticket.Number);
        Assert.Equal("Welcome to Zammad!", ticket.Title);
        Assert.Equal(new UserId(1), ticket.OwnerId);
        Assert.Equal(new UserId(2), ticket.CustomerId);
        Assert.Null(ticket.Note);
        Assert.Null(ticket.FirstResponseAt);
        Assert.Null(ticket.FirstResponseEscalationAt);
        Assert.Null(ticket.FirstResponseInMin);
        Assert.Null(ticket.FirstResponseDiffInMin);
        Assert.Null(ticket.CloseAt);
        Assert.Null(ticket.CloseEscalationAt);
        Assert.Null(ticket.CloseInMin);
        Assert.Null(ticket.CloseDiffInMin);
        Assert.Null(ticket.UpdateEscalationAt);
        Assert.Null(ticket.UpdateInMin);
        Assert.Null(ticket.UpdateDiffInMin);
        Assert.Equal(DateTimeOffset.Parse("2017-09-25T14:50:50.946Z"), ticket.LastContactAt);
        Assert.Null(ticket.LastContactAgentAt);
        Assert.Equal(DateTimeOffset.Parse("2017-09-25T14:50:50.946Z"), ticket.LastContactCustomerAt);
        Assert.Null(ticket.LastOwnerUpdateAt);
        Assert.Equal(new ArticleTypeId(5), ticket.CreateArticleTypeId);
        Assert.Equal(new UserId(2), ticket.CreateArticleSenderId);
        Assert.Equal(1, ticket.ArticleCount);
        Assert.Null(ticket.EscalationAt);
        Assert.Null(ticket.PendingTime);
        Assert.Null(ticket.Type);
        Assert.Null(ticket.TimeUnit);
        Assert.Equal(new Dictionary<string, object>(), ticket.Preferences);
        Assert.Equal(new UserId(3), ticket.UpdatedById);
        Assert.Equal(new UserId(2), ticket.CreatedById);
        Assert.Equal(DateTimeOffset.Parse("2017-09-25T14:50:50.910Z"), ticket.CreatedAt);
        Assert.Equal(DateTimeOffset.Parse("2017-09-30T14:47:55.177Z"), ticket.UpdatedAt);
    }

    [Fact]
    public async Task ParseStreamContentAsync_Success_TestAsync()
    {
        var httpResponse = CreateTestResponse();
        using var ticketStream = await httpResponse.ParseAsync<Stream>();

        Assert.NotNull(ticketStream);

        using var reader = new StreamReader(ticketStream);
        var ticketString = await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
        Assert.Equal(TicketSerialized, ticketString);
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
