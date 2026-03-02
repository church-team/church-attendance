using System.Security.Claims;
using churchAttendace.Data;
using churchAttendace.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace churchAttendace.Services.Implementations
{
    public class UserScopeService : IUserScopeService
    {
        private const string AllowedStageIdsKey = "AllowedStageIds";
        private const string AllowedClassIdsKey = "AllowedClassIds";

        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserScopeService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? CurrentUser => _httpContextAccessor.HttpContext?.User;

        public Task<string?> GetUserIdAsync()
        {
            var userId = CurrentUser?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Task.FromResult<string?>(userId);
        }

        public bool IsAdmin() => CurrentUser?.IsInRole("Admin") == true;

        public bool IsStageManager() => CurrentUser?.IsInRole("StageManager") == true;

        public bool IsServant() => CurrentUser?.IsInRole("Servant") == true;

        public Task<List<int>> GetAllowedStageIdsAsync()
        {
            return GetCachedListAsync(AllowedStageIdsKey, async () =>
            {
                if (IsAdmin())
                {
                    return await _context.Stages.Select(s => s.Id).ToListAsync();
                }

                var userId = await GetUserIdAsync();
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return new List<int>();
                }

                if (IsStageManager())
                {
                    return await _context.StageManagerStages
                        .Where(s => s.UserId == userId && s.IsActive)
                        .Select(s => s.StageId)
                        .Distinct()
                        .ToListAsync();
                }

                if (IsServant())
                {
                    var classIds = await GetAllowedClassIdsAsync();
                    return await _context.Classes
                        .Where(c => classIds.Contains(c.Id))
                        .Select(c => c.StageId)
                        .Distinct()
                        .ToListAsync();
                }

                return new List<int>();
            });
        }

        public Task<List<int>> GetAllowedClassIdsAsync()
        {
            return GetCachedListAsync(AllowedClassIdsKey, async () =>
            {
                if (IsAdmin())
                {
                    return await _context.Classes.Select(c => c.Id).ToListAsync();
                }

                var userId = await GetUserIdAsync();
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return new List<int>();
                }

                if (IsStageManager())
                {
                    var stageIds = await GetAllowedStageIdsAsync();
                    return await _context.Classes
                        .Where(c => stageIds.Contains(c.StageId))
                        .Select(c => c.Id)
                        .Distinct()
                        .ToListAsync();
                }

                return await _context.ClassServants
                    .Where(c => c.UserId == userId && c.IsActive)
                    .Select(c => c.ClassId)
                    .Distinct()
                    .ToListAsync();
            });
        }

        public async Task<bool> CanAccessStageAsync(int stageId)
        {
            if (IsAdmin())
            {
                return true;
            }

            if (IsStageManager())
            {
                var stageIds = await GetAllowedStageIdsAsync();
                return stageIds.Contains(stageId);
            }

            if (IsServant())
            {
                var classIds = await GetAllowedClassIdsAsync();
                return await _context.Classes.AnyAsync(c => classIds.Contains(c.Id) && c.StageId == stageId);
            }

            return false;
        }

        public async Task<bool> CanAccessClassAsync(int classId)
        {
            if (IsAdmin())
            {
                return true;
            }

            if (IsStageManager())
            {
                var stageIds = await GetAllowedStageIdsAsync();
                return await _context.Classes.AnyAsync(c => c.Id == classId && stageIds.Contains(c.StageId));
            }

            if (IsServant())
            {
                var classIds = await GetAllowedClassIdsAsync();
                return classIds.Contains(classId);
            }

            return false;
        }

        public async Task<bool> CanAccessSessionAsync(int sessionId)
        {
            if (IsAdmin())
            {
                return true;
            }

            if (IsStageManager())
            {
                var stageIds = await GetAllowedStageIdsAsync();
                return await _context.Sessions.AnyAsync(s => s.Id == sessionId && stageIds.Contains(s.Class!.StageId));
            }

            if (IsServant())
            {
                var classIds = await GetAllowedClassIdsAsync();
                return await _context.Sessions.AnyAsync(s => s.Id == sessionId && classIds.Contains(s.ClassId));
            }

            return false;
        }

        public async Task<bool> CanAccessChildAsync(int childId)
        {
            if (IsAdmin())
            {
                return true;
            }

            if (IsStageManager())
            {
                var stageIds = await GetAllowedStageIdsAsync();
                return await _context.Children.AnyAsync(c => c.Id == childId && stageIds.Contains(c.Class!.StageId));
            }

            if (IsServant())
            {
                var classIds = await GetAllowedClassIdsAsync();
                return await _context.Children.AnyAsync(c => c.Id == childId && classIds.Contains(c.ClassId));
            }

            return false;
        }

        private async Task<List<int>> GetCachedListAsync(string key, Func<Task<List<int>>> factory)
        {
            var items = _httpContextAccessor.HttpContext?.Items;
            if (items != null && items.TryGetValue(key, out var cached) && cached is List<int> cachedList)
            {
                return cachedList;
            }

            var result = await factory();
            if (items != null)
            {
                items[key] = result;
            }

            return result;
        }
    }
}
