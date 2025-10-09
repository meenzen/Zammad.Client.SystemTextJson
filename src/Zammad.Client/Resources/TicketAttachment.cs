using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;


namespace Zammad.Client.Resources
{
    
    public class TicketAttachment
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("filename")]
        public string Filename { get; set; }

        [JsonPropertyName("data")]
        public string Data { get; set; }

        [JsonPropertyName("mime-type")]
        public string MimeType { get; set; }

        [JsonPropertyName("preferences")]
        public IDictionary<string, object> Preferences { get; set; }

        public static TicketAttachment CreateFromFile(string fileName, string mimeType)
        {
            var buffer = File.ReadAllBytes(fileName);
            var base64 = Convert.ToBase64String(buffer);

            return new TicketAttachment
            {
                Filename = Path.GetFileName(fileName),
                Data = base64,
                MimeType = mimeType
            };
        }
    }
}
