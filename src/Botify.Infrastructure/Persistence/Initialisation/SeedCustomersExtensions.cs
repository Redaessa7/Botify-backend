using Botify.Domain.Models.Customers;
using Microsoft.EntityFrameworkCore;

namespace Botify.Infrastructure.Persistence.Initialisation;

public static class SeedCustomersExtensions
{
    public static async Task SeedCustomersAsync(this ApplicationDbContext context, Guid merchantId)
    {
        if (await context.Customers.AnyAsync(c => c.MerchantId == merchantId))
            return;

        var customers = new[]
        {
        new { Id = SeedIds.CustomerJohnDoe,   Name = "John Doe",   Contact = "john@example.com" },
        new { Id = SeedIds.CustomerFatimaAli, Name = "Fatima Ali", Contact = "fatima@example.com" },
        new { Id = SeedIds.CustomerCarlos,    Name = "Carlos Mendes", Contact = "carlos@example.com" }
    };

        foreach (var data in customers)
        {
            var result = Customer.Create(data.Id, merchantId, data.Name, data.Contact);
            if (result.IsSuccess)
                context.Customers.Add(result.Value);
        }
    }

}
