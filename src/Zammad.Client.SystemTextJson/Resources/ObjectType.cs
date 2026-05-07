using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ObjectType
{
    [JsonStringEnumMemberName("Ticket")]
    Ticket,

    [JsonStringEnumMemberName("User")]
    User,

    [JsonStringEnumMemberName("Organization")]
    Organization,

    [JsonStringEnumMemberName("Group")]
    Group,
}
