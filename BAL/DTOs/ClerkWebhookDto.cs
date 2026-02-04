using System.Text.Json.Serialization;

namespace BAL.DTOs
{
    /// <summary>
    /// DTO cho webhook payload từ Clerk
    /// </summary>
    public class ClerkWebhookDto
    {
        [JsonPropertyName("object")]
        public string? Object { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("data")]
        public ClerkWebhookDataDto? Data { get; set; }

        [JsonPropertyName("timestamp")]
        public long? Timestamp { get; set; }

        [JsonPropertyName("instance_id")]
        public string? InstanceId { get; set; }

        [JsonPropertyName("event_attributes")]
        public ClerkEventAttributesDto? EventAttributes { get; set; }

        // Capture các property không biết để tránh lỗi deserialization
        [JsonExtensionData]
        public Dictionary<string, object>? ExtensionData { get; set; }
    }

    public class ClerkEventAttributesDto
    {
        [JsonPropertyName("http_request")]
        public ClerkHttpRequestDto? HttpRequest { get; set; }
    }

    public class ClerkHttpRequestDto
    {
        [JsonPropertyName("client_ip")]
        public string? ClientIp { get; set; }

        [JsonPropertyName("user_agent")]
        public string? UserAgent { get; set; }
    }

    public class ClerkWebhookDataDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("object")]
        public string? Object { get; set; }

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

        [JsonPropertyName("deleted")]
        public bool? Deleted { get; set; }

        // Các field bổ sung có thể có trong payload
        [JsonPropertyName("backup_code_enabled")]
        public bool? BackupCodeEnabled { get; set; }

        [JsonPropertyName("banned")]
        public bool? Banned { get; set; }

        [JsonPropertyName("bypass_client_trust")]
        public bool? BypassClientTrust { get; set; }

        [JsonPropertyName("create_organization_enabled")]
        public bool? CreateOrganizationEnabled { get; set; }

        [JsonPropertyName("delete_self_enabled")]
        public bool? DeleteSelfEnabled { get; set; }

        [JsonPropertyName("enterprise_accounts")]
        public List<object>? EnterpriseAccounts { get; set; }

        [JsonPropertyName("external_accounts")]
        public List<ClerkExternalAccountDto>? ExternalAccounts { get; set; }

        [JsonPropertyName("external_id")]
        public string? ExternalId { get; set; }

        [JsonPropertyName("has_image")]
        public bool? HasImage { get; set; }

        [JsonPropertyName("last_active_at")]
        public long? LastActiveAt { get; set; }

        [JsonPropertyName("last_sign_in_at")]
        public long? LastSignInAt { get; set; }

        [JsonPropertyName("legal_accepted_at")]
        public long? LegalAcceptedAt { get; set; }

        [JsonPropertyName("locale")]
        public string? Locale { get; set; }

        [JsonPropertyName("locked")]
        public bool? Locked { get; set; }

        [JsonPropertyName("lockout_expires_in_seconds")]
        public int? LockoutExpiresInSeconds { get; set; }

        [JsonPropertyName("mfa_disabled_at")]
        public long? MfaDisabledAt { get; set; }

        [JsonPropertyName("mfa_enabled_at")]
        public long? MfaEnabledAt { get; set; }

        [JsonPropertyName("passkeys")]
        public List<object>? Passkeys { get; set; }

        [JsonPropertyName("password_enabled")]
        public bool? PasswordEnabled { get; set; }

        [JsonPropertyName("password_last_updated_at")]
        public long? PasswordLastUpdatedAt { get; set; }

        [JsonPropertyName("primary_email_address_id")]
        public string? PrimaryEmailAddressId { get; set; }

        [JsonPropertyName("primary_phone_number_id")]
        public string? PrimaryPhoneNumberId { get; set; }

        [JsonPropertyName("primary_web3_wallet_id")]
        public string? PrimaryWeb3WalletId { get; set; }

        [JsonPropertyName("profile_image_url")]
        public string? ProfileImageUrl { get; set; }

        [JsonPropertyName("requires_password_reset")]
        public bool? RequiresPasswordReset { get; set; }

        [JsonPropertyName("saml_accounts")]
        public List<object>? SamlAccounts { get; set; }

        [JsonPropertyName("totp_enabled")]
        public bool? TotpEnabled { get; set; }

        [JsonPropertyName("two_factor_enabled")]
        public bool? TwoFactorEnabled { get; set; }

        [JsonPropertyName("unsafe_metadata")]
        public Dictionary<string, object>? UnsafeMetadata { get; set; }

        [JsonPropertyName("verification_attempts_remaining")]
        public int? VerificationAttemptsRemaining { get; set; }

        [JsonPropertyName("web3_wallets")]
        public List<object>? Web3Wallets { get; set; }

        // Capture các property không biết để tránh lỗi deserialization
        [JsonExtensionData]
        public Dictionary<string, object>? ExtensionData { get; set; }
    }

    public class ClerkExternalAccountDto
    {
        [JsonPropertyName("approved_scopes")]
        public string? ApprovedScopes { get; set; }

        [JsonPropertyName("avatar_url")]
        public string? AvatarUrl { get; set; }

        [JsonPropertyName("created_at")]
        public long? CreatedAt { get; set; }

        [JsonPropertyName("email_address")]
        public string? EmailAddress { get; set; }

        [JsonPropertyName("external_account_id")]
        public string? ExternalAccountId { get; set; }

        [JsonPropertyName("family_name")]
        public string? FamilyName { get; set; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("given_name")]
        public string? GivenName { get; set; }

        [JsonPropertyName("google_id")]
        public string? GoogleId { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("identification_id")]
        public string? IdentificationId { get; set; }

        [JsonPropertyName("image_url")]
        public string? ImageUrl { get; set; }

        [JsonPropertyName("label")]
        public string? Label { get; set; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }

        [JsonPropertyName("object")]
        public string? Object { get; set; }

        [JsonPropertyName("picture")]
        public string? Picture { get; set; }

        [JsonPropertyName("provider")]
        public string? Provider { get; set; }

        [JsonPropertyName("provider_user_id")]
        public string? ProviderUserId { get; set; }

        [JsonPropertyName("public_metadata")]
        public Dictionary<string, object>? PublicMetadata { get; set; }

        [JsonPropertyName("updated_at")]
        public long? UpdatedAt { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("verification")]
        public ClerkVerificationDto? Verification { get; set; }
    }
}
