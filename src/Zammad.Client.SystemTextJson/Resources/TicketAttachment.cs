using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public class TicketAttachment
{
    [JsonPropertyName("id")]
    public AttachmentId Id { get; set; }

    [JsonPropertyName("store_file_id")]
    public StoreFileId? StoreFileId { get; set; }

    [JsonPropertyName("filename")]
    public string? Filename { get; set; }

    [JsonPropertyName("size")]
    public string? Size { get; set; }

    [JsonPropertyName("data")]
    public string? Data { get; set; }

    [JsonPropertyName("mime-type")]
    public string? MimeType { get; set; }

    [JsonPropertyName("preferences")]
    public IDictionary<string, object>? Preferences { get; set; }

    public static TicketAttachment CreateFromFile(string fileName, string mimeType)
    {
        var buffer = File.ReadAllBytes(fileName);
        var base64 = Convert.ToBase64String(buffer);

        return new TicketAttachment
        {
            Filename = Path.GetFileName(fileName),
            Data = base64,
            MimeType = mimeType,
        };
    }

    public static TicketAttachment CreateFromBytes(byte[] data, string fileName, string mimeType)
    {
        var base64 = Convert.ToBase64String(data);

        return new TicketAttachment
        {
            Filename = fileName,
            Data = base64,
            MimeType = mimeType,
        };
    }

    public static TicketAttachment CreateFromStream(Stream stream, string fileName, string mimeType)
    {
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        var data = memoryStream.ToArray();
        var base64 = Convert.ToBase64String(data);

        return new TicketAttachment
        {
            Filename = fileName,
            Data = base64,
            MimeType = mimeType,
        };
    }
}
