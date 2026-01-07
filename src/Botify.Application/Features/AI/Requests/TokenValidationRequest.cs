using Botify.Domain.Models.AI.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Botify.Application.Features.AI.Requests
{
    public sealed class TokenValidationRequest
    {
        public string ApiKey { get; init; } = string.Empty;
        public AIPlatform Platform { get; init; }
        public string? BaseUrl { get; init; } // required for Azure
    }
}
