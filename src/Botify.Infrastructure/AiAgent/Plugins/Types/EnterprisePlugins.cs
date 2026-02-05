using System;
using System.Collections.Generic;
using Microsoft.SemanticKernel;

namespace Botify.Infrastructure.AIServices.Plugins.Types
{
    public class EnterprisePlugins : IPlugin
    {
        // Simulated product catalog
        private readonly Dictionary<int, string> _products = new()
        {
            { 1, "Laptop - Dell XPS 15" },
            { 2, "Smartphone - Samsung Galaxy S24" },
            { 3, "Headphones - Sony WH-1000XM5" }
        };

        [KernelFunction("GetProducts")]
        public string GetProducts()
        {
            Console.WriteLine("[PROCESS] Fetching product list...");
            var list = string.Join(", ", _products.Values);
            return $"Available products: {list}";
        }

        [KernelFunction("PlaceOrder")]
        public string PlaceOrder(int productId, string customerName)
        {
            Console.WriteLine($"[PROCESS] Placing order for product {productId} by {customerName}...");

            if (_products.TryGetValue(productId, out var product))
            {
                return $"Order placed successfully for {product} by {customerName}.";
            }

            return $"Product with ID {productId} not found.";
        }

        [KernelFunction("GetMerchantDetails")]
        public string GetMerchantDetails(string merchantId)
        {
            Console.WriteLine($"[PROCESS] Retrieving merchant details for {merchantId}...");

            // Simulated merchant data
            var merchantDetails = merchantId switch
            {
                "M001" => "Merchant: TechWorld, Location: New York, Rating: 4.8",
                "M002" => "Merchant: GadgetHub, Location: San Francisco, Rating: 4.5",
                _ => $"Merchant {merchantId} not found."
            };

            return merchantDetails;
        }
    }
}
