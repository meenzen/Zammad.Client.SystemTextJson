using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public class User
{
    [JsonPropertyName("id")]
    public UserId Id { get; set; }

    [JsonPropertyName("organization_id")]
    public OrganizationId? OrganizationId { get; set; }

    [JsonPropertyName("login")]
    public string? Login { get; set; }

    [JsonPropertyName("firstname")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastname")]
    public string? LastName { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("image")]
    public string? Image { get; set; }

    [JsonPropertyName("image_source")]
    public string? ImageSource { get; set; }

    [JsonPropertyName("web")]
    public string? Web { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("fax")]
    public string? Fax { get; set; }

    [JsonPropertyName("mobile")]
    public string? Mobile { get; set; }

    [JsonPropertyName("department")]
    public string? Department { get; set; }

    [JsonPropertyName("street")]
    public string? Street { get; set; }

    [JsonPropertyName("zip")]
    public string? Zip { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("country")]
    public string? Country { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("vip")]
    public bool? Vip { get; set; }

    [JsonPropertyName("verified")]
    public bool? Verified { get; set; }

    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("last_login")]
    public DateTimeOffset? LastLogin { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("login_failed")]
    public int? LoginFailed { get; set; }

    [JsonPropertyName("out_of_office")]
    public bool? OutOfOffice { get; set; }

    [JsonPropertyName("out_of_office_start_at")]
    public DateTimeOffset? OutOfOfficeStartAt { get; set; }

    [JsonPropertyName("out_of_office_end_at")]
    public DateTimeOffset? OutOfOfficeEndAt { get; set; }

    [JsonPropertyName("out_of_office_replacement_id")]
    public UserId? OutOfOfficeReplacementId { get; set; }

    [JsonPropertyName("preferences")]
    public IDictionary<string, object>? Preferences { get; set; }

    [JsonPropertyName("updated_by_id")]
    public UserId? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}
