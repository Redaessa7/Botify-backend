// File: AssignShopRequest.cs (in Application/Services/Merchants/Requests)

using System.ComponentModel.DataAnnotations;
using Botify.Domain.Models.Merchants.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace Botify.Application.Services.Merchants.Requests;

/// <summary>
/// Request DTO for assigning shop settings to a merchant.
/// </summary>
public sealed record AssignShopRequest(
    [Required] string Name,
    [Required] string Currency,
    [Required] string Language,
    [Required] string Address,
    SocialMediaLinks? SocialMediaLinks,
    IFormFile? Image,
    decimal? TaxRate
);