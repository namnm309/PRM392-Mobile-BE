namespace BAL.DTOs.LinkedAccount
{
    public class LinkedAccountDto
    {
        public Guid Id { get; set; }
        public string Provider { get; set; } = string.Empty;
        public string? ProviderEmail { get; set; }
        public string? ProviderName { get; set; }
        public string? ProviderAvatarUrl { get; set; }
        public DateTime LinkedAt { get; set; }
        public DateTime? LastUsedAt { get; set; }
    }

    public class LinkAccountRequest
    {
        public string Provider { get; set; } = string.Empty;
        public string ProviderUserId { get; set; } = string.Empty;
        public string? ProviderEmail { get; set; }
        public string? ProviderName { get; set; }
        public string? ProviderAvatarUrl { get; set; }
    }

    public class LinkedAccountsResponse
    {
        public List<LinkedAccountDto> Accounts { get; set; } = new();
        public List<AvailableProvider> AvailableProviders { get; set; } = new();
    }

    public class AvailableProvider
    {
        public string Provider { get; set; } = string.Empty;
        public bool IsLinked { get; set; }
    }
}
