using Botify.Domain.Models.Merchants.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace Botify.Application.Features.Merchants.Requests;
    public record CreateShopRequest
    {
        public Guid MerchantId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Currency { get; init; } = string.Empty;
        public string Language { get; init; } = string.Empty;
        public string Address { get; init; } = string.Empty;
        public SocialMediaLinks? SocialMediaLinks { get; init; }
        public IFormFile? Image { get; init; }
        public decimal? TaxRate { get; init; }
    }