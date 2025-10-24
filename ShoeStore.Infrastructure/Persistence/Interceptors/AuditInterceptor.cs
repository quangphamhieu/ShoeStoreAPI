using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ShoeStore.Domain.Entities;
using ShoeStore.Infrastructure.Persistence;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace ShoeStore.Infrastructure.Persistence.Interceptors
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context as ShoeStoreDbContext;
            if (context == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

            var userId = GetCurrentUserId();
            var ipAddress = GetClientIpAddress();

            var auditEntries = new List<AuditLog>();

            foreach (var entry in context.ChangeTracker.Entries().Where(e => e.Entity is not AuditLog))
            {
                if (entry.State == EntityState.Added ||
                    entry.State == EntityState.Modified ||
                    entry.State == EntityState.Deleted)
                {
                    var audit = new AuditLog
                    {
                        UserId = userId,
                        TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                        CreatedAt = DateTime.UtcNow,
                        IPAddress = ipAddress
                    };

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            audit.Action = "CREATE";
                            audit.RecordId = GetPrimaryKey(entry);
                            audit.NewValue = JsonSerializer.Serialize(entry.CurrentValues.ToObject());
                            break;

                        case EntityState.Modified:
                            audit.Action = "UPDATE";
                            audit.RecordId = GetPrimaryKey(entry);
                            audit.OldValue = JsonSerializer.Serialize(GetOriginalValues(entry));
                            audit.NewValue = JsonSerializer.Serialize(entry.CurrentValues.ToObject());
                            break;

                        case EntityState.Deleted:
                            audit.Action = "DELETE";
                            audit.RecordId = GetPrimaryKey(entry);
                            audit.OldValue = JsonSerializer.Serialize(entry.OriginalValues.ToObject());
                            break;
                    }

                    auditEntries.Add(audit);
                }
            }

            if (auditEntries.Count > 0)
                context.AuditLogs.AddRange(auditEntries);

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private long? GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return null;

            var idClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return long.TryParse(idClaim, out var id) ? id : null;
        }

        private string? GetClientIpAddress()
        {
            return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }

        private long? GetPrimaryKey(EntityEntry entry)
        {
            var keyName = entry.Metadata.FindPrimaryKey()?.Properties.FirstOrDefault()?.Name;
            var value = entry.Property(keyName ?? "Id").CurrentValue;
            return value != null ? Convert.ToInt64(value) : null;
        }

        private object GetOriginalValues(EntityEntry entry)
        {
            var dict = new Dictionary<string, object?>();
            foreach (var prop in entry.OriginalValues.Properties)
                dict[prop.Name] = entry.OriginalValues[prop];
            return dict;
        }
    }
}
