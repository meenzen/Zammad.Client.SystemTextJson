using System.Text.Json.Serialization;

namespace Zammad.Client.Resources.Internal;

internal sealed class TicketCustomerRequest
{
    [JsonPropertyName("customer_id")]
    public required UserId CustomerId { get; set; }

    [JsonPropertyName("organization_id")]
    public OrganizationId? OrganizationId { get; set; }
}
