using System.Text.Json;
using churchAttendace.Data;
using churchAttendace.Models.Entities;
using churchAttendace.Services.Interfaces;

namespace churchAttendace.Services.Implementations
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string tableName, string recordId, string action, object? changes = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userId = httpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var ipAddress = httpContext?.Connection?.RemoteIpAddress?.ToString();
            var userAgent = httpContext?.Request?.Headers["User-Agent"].ToString();

            //var auditLog = new AuditLog
            //{
            //    UserId = userId,
            //    TableName = tableName,
            //    RecordId = recordId,
            //    Action = action,
            //    ChangesJson = changes != null ? JsonSerializer.Serialize(changes) : null,
            //    IpAddress = ipAddress,
            //    UserAgent = userAgent,
            //    Timestamp = DateTime.UtcNow
            //};

            //_context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogCreateAsync<T>(T entity) where T : class
        {
            var tableName = typeof(T).Name;
            var recordId = GetEntityId(entity);
            await LogAsync(tableName, recordId, "Create", entity);
        }

        public async Task LogUpdateAsync<T>(T originalEntity, T updatedEntity) where T : class
        {
            var tableName = typeof(T).Name;
            var recordId = GetEntityId(updatedEntity);

            var changes = new
            {
                Original = originalEntity,
                Updated = updatedEntity
            };

            await LogAsync(tableName, recordId, "Update", changes);
        }

        public async Task LogDeleteAsync<T>(T entity) where T : class
        {
            var tableName = typeof(T).Name;
            var recordId = GetEntityId(entity);
            await LogAsync(tableName, recordId, "Delete", entity);
        }

        public async Task LogSoftDeleteAsync<T>(T entity) where T : class
        {
            var tableName = typeof(T).Name;
            var recordId = GetEntityId(entity);
            await LogAsync(tableName, recordId, "SoftDelete", entity);
        }

        private static string GetEntityId<T>(T entity)
        {
            var idProperty = typeof(T).GetProperty("Id");
            return idProperty?.GetValue(entity)?.ToString() ?? "Unknown";
        }
    }
}
