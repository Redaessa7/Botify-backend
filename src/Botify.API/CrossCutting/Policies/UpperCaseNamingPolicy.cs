using System.Text.Json;

namespace Botify.API.CrossCutting.Policies
{

    public sealed class UpperCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
            => name.ToUpperInvariant();
    }

}
