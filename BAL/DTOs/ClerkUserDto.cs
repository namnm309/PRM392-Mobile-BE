using System.Text.Json.Serialization;

namespace BAL.DTOs
{
    /// <summary>
    /// DTO cho thông tin user từ Clerk webhook
    /// </summary>
    public class ClerkUserDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("email_addresses")]
        public List<ClerkEmailAddressDto>? EmailAddresses { get; set; }

        [JsonPropertyName("phone_numbers")]
        public List<ClerkPhoneNumberDto>? PhoneNumbers { get; set; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }

        [JsonPropertyName("image_url")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("created_at")]
        public long? CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public long? UpdatedAt { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("public_metadata")]
        public Dictionary<string, object>? PublicMetadata { get; set; }

        [JsonPropertyName("private_metadata")]
        public Dictionary<string, object>? PrivateMetadata { get; set; }
    }

    public class ClerkEmailAddressDto
    {
        [JsonPropertyName("email_address")]
        public string? EmailAddress { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("verification")]
        public ClerkVerificationDto? Verification { get; set; }

        [JsonPropertyName("created_at")]
        public long? CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public long? UpdatedAt { get; set; }

        [JsonPropertyName("linked_to")]
        public List<ClerkLinkedToDto>? LinkedTo { get; set; }

        [JsonPropertyName("matches_sso_connection")]
        public bool? MatchesSsoConnection { get; set; }

        [JsonPropertyName("object")]
        public string? Object { get; set; }

        [JsonPropertyName("reserved")]
        public bool? Reserved { get; set; }
    }

    public class ClerkPhoneNumberDto
    {
        [JsonPropertyName("phone_number")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("verification")]
        public ClerkVerificationDto? Verification { get; set; }

        [JsonPropertyName("created_at")]
        public long? CreatedAt { get; set; }

        [JsonPropertyName("updated_at")]
        public long? UpdatedAt { get; set; }

        [JsonPropertyName("linked_to")]
        public List<ClerkLinkedToDto>? LinkedTo { get; set; }

        [JsonPropertyName("object")]
        public string? Object { get; set; }

        [JsonPropertyName("reserved")]
        public bool? Reserved { get; set; }
    }

    public class ClerkLinkedToDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }

    public class ClerkVerificationDto
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("strategy")]
        public string? Strategy { get; set; }

        [JsonPropertyName("object")]
        public string? Object { get; set; }

        [JsonPropertyName("attempts")]
        public int? Attempts { get; set; }

        [JsonPropertyName("expire_at")]
        public long? ExpireAt { get; set; }
    }
}
