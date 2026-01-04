using System;
using Microsoft.SemanticKernel;

namespace Botify.Infrastructure.AIServices.Plugins.Types
{
    public class FreePlugins : IPlugin
    {
        [KernelFunction("SayToHelloToUser")]
        public string SayHello(string name)
        {
            Console.WriteLine($"[PROCESS] FreePlugins.SayHello called with: {name}");
            return $"Hello {name}, welcome to FreePlugins!";
        }

        [KernelFunction("GetFreeFeatures")]
        public string GetFreeFeatures()
        {
            Console.WriteLine("[PROCESS] Fetching free features...");
            return "Free tier includes: Basic product browsing, limited merchant details, and simple order placement.";
        }

        [KernelFunction("GetMerchantDetails")]
        public string GetMerchantDetails(string merchantId)
        {
            Console.WriteLine($"[PROCESS] Retrieving merchant details for {merchantId} (Free tier)...");

            // Simulated limited merchant data for free users
            var merchantDetails = merchantId switch
            {
                "M001" => "Merchant: TechWorld (basic info only)",
                "M002" => "Merchant: GadgetHub (basic info only)",
                _ => $"Merchant {merchantId} not found in Free tier."
            };

            return merchantDetails;
        }
    }
}
