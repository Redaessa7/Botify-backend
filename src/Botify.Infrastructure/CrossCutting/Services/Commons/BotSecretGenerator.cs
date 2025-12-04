using Botify.Application.CrossCutting.Interfaces;
using System.Security.Cryptography;

namespace Botify.Infrastructure.CrossCutting.Services.Commons
{
    public sealed class BotSecretGenerator : IBotSecretGenerator
    {
        public string Generate()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}
