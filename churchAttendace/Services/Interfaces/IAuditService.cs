namespace churchAttendace.Services.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(string tableName, string recordId, string action, object? changes = null);
        Task LogCreateAsync<T>(T entity) where T : class;
        Task LogUpdateAsync<T>(T originalEntity, T updatedEntity) where T : class;
        Task LogDeleteAsync<T>(T entity) where T : class;
        Task LogSoftDeleteAsync<T>(T entity) where T : class;
    }
}
