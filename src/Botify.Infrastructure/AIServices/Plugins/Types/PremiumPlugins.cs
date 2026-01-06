using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SemanticKernel;

namespace Botify.Infrastructure.AIServices.Plugins.Types
{
    public class PremiumPlugins : IPlugin
    {
        [KernelFunction("SayToHelloToUser")]
        public string SayHello(string name)
        {
            return $"Hello {name}";
        }
    }
}
