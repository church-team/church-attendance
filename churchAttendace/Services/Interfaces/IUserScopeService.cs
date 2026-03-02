namespace churchAttendace.Services.Interfaces
{
    public interface IUserScopeService
    {
        Task<string?> GetUserIdAsync();
        bool IsAdmin();
        bool IsStageManager();
        bool IsServant();
        Task<List<int>> GetAllowedStageIdsAsync();
        Task<List<int>> GetAllowedClassIdsAsync();
        Task<bool> CanAccessStageAsync(int stageId);
        Task<bool> CanAccessClassAsync(int classId);
        Task<bool> CanAccessSessionAsync(int sessionId);
        Task<bool> CanAccessChildAsync(int childId);
    }
}
