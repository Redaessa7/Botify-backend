using Botify.Application.CrossCutting.Interfaces;
using Botify.Infrastructure.Persistence;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Botify.Infrastructure.CrossCutting.HealthChecks;

public class DBHealthCheck : IHealthCheck
{
  private readonly ApplicationDbContext _db;

  public DBHealthCheck(ApplicationDbContext db)
  {
    _db = db;
  }

  public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
  {
    return await _db.Database.CanConnectAsync(cancellationToken)
        ? HealthCheckResult.Healthy("Database is reachable")
        : HealthCheckResult.Unhealthy("Database is not reachable");
  }
}