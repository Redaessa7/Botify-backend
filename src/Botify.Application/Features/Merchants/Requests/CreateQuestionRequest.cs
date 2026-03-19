using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.Models.SupportRequests;
using Botify.Domain.Models.SupportRequests.Enums;
using Botify.Domain.ResultPattern;

namespace Botify.Application.Features.Merchants.Requests
{
    public sealed class CreateQuestionRequest
    {
        public Guid BotId { get; init; }
        public string BotName { get; init; } = string.Empty;
        public BotPlatform Platform { get; init; } 
        public SupportRequestType Type { get; init; } 
        public string CustomerName { get; init; } = string.Empty;
        public string? Email { get; init; }
        public string? Telegram { get; init; }
        public string? Whatsapp { get; init; }
        public string? Messenger { get; init; }
        public string Message { get; init; } = string.Empty;

      
        }
    }
