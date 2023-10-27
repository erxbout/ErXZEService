using ErXZEService.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ErXZEService.Services.SQL
{
    public interface ISession : IDisposable
    {
        List<T> SelectMany<T>(Expression<Func<T, bool>> whereExpression = null, int limit = 0) where T : Entity, new();
        T SelectSingle<T>(Expression<Func<T, bool>> whereExpression) where T : Entity, new();
        T SelectSingleOrDefault<T>(Expression<Func<T, bool>> whereExpression) where T : Entity, new();

        void SelectRecursive(Entity ent, bool ignoreCache = false);
        void SaveRecursive<T>(List<T> entities) where T : Entity;

        void Save(Entity entity);
        void Save(IEnumerable<Entity> entities);
        TableQuery<T> SelectQuery<T>() where T : Entity, new();
        int Count<T>(Expression<Func<T, bool>> whereExpression) where T : Entity, new();
    }
}
