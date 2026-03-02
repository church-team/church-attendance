using churchAttendace.Models.Entities;
using churchAttendace.Services.Interfaces;

namespace churchAttendace.Utilities
{
    public static class QueryableScopeExtensions
    {
        public static IQueryable<Stage> ApplyStageScope(this IQueryable<Stage> query, IUserScopeService scope, IReadOnlyCollection<int>? allowedStageIds = null)
        {
            if (scope.IsAdmin())
            {
                return query;
            }

            var stageIds = allowedStageIds ?? Array.Empty<int>();
            return stageIds.Count == 0 ? query.Where(_ => false) : query.Where(s => stageIds.Contains(s.Id));
        }

        public static IQueryable<Class> ApplyClassScope(this IQueryable<Class> query, IUserScopeService scope, IReadOnlyCollection<int>? allowedStageIds = null, IReadOnlyCollection<int>? allowedClassIds = null)
        {
            if (scope.IsAdmin())
            {
                return query;
            }

            if (scope.IsStageManager())
            {
                var stageIds = allowedStageIds ?? Array.Empty<int>();
                return stageIds.Count == 0 ? query.Where(_ => false) : query.Where(c => stageIds.Contains(c.StageId));
            }

            var classIds = allowedClassIds ?? Array.Empty<int>();
            return classIds.Count == 0 ? query.Where(_ => false) : query.Where(c => classIds.Contains(c.Id));
        }

        public static IQueryable<Child> ApplyChildScope(this IQueryable<Child> query, IUserScopeService scope, IReadOnlyCollection<int>? allowedStageIds = null, IReadOnlyCollection<int>? allowedClassIds = null)
        {
            if (scope.IsAdmin())
            {
                return query;
            }

            if (scope.IsStageManager())
            {
                var stageIds = allowedStageIds ?? Array.Empty<int>();
                return stageIds.Count == 0 ? query.Where(_ => false) : query.Where(c => stageIds.Contains(c.Class!.StageId));
            }

            var classIds = allowedClassIds ?? Array.Empty<int>();
            return classIds.Count == 0 ? query.Where(_ => false) : query.Where(c => classIds.Contains(c.ClassId));
        }

        public static IQueryable<Session> ApplySessionScope(this IQueryable<Session> query, IUserScopeService scope, IReadOnlyCollection<int>? allowedStageIds = null, IReadOnlyCollection<int>? allowedClassIds = null)
        {
            if (scope.IsAdmin())
            {
                return query;
            }

            if (scope.IsStageManager())
            {
                var stageIds = allowedStageIds ?? Array.Empty<int>();
                return stageIds.Count == 0 ? query.Where(_ => false) : query.Where(s => stageIds.Contains(s.Class!.StageId));
            }

            var classIds = allowedClassIds ?? Array.Empty<int>();
            return classIds.Count == 0 ? query.Where(_ => false) : query.Where(s => classIds.Contains(s.ClassId));
        }

        public static IQueryable<Attendance> ApplyAttendanceScope(this IQueryable<Attendance> query, IUserScopeService scope, IReadOnlyCollection<int>? allowedStageIds = null, IReadOnlyCollection<int>? allowedClassIds = null)
        {
            if (scope.IsAdmin())
            {
                return query;
            }

            if (scope.IsStageManager())
            {
                var stageIds = allowedStageIds ?? Array.Empty<int>();
                return stageIds.Count == 0 ? query.Where(_ => false) : query.Where(a => stageIds.Contains(a.Session!.Class!.StageId));
            }

            var classIds = allowedClassIds ?? Array.Empty<int>();
            return classIds.Count == 0 ? query.Where(_ => false) : query.Where(a => classIds.Contains(a.Session!.ClassId));
        }
    }
}
