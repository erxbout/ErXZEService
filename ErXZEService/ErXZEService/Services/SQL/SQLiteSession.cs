using ErXZEService.Helper;
using ErXZEService.Models;
using ErXZEService.Services.Log;
using ErXZEService.Services.Paths;
using ErXZEService.Services.SQL.Attributes;
using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace ErXZEService.Services.SQL
{
    public class SQLiteSession : ISession
    {
        private SQLiteConnection _connection = null;
        private ILogger _logger;
        private string _connectionPath;

        private List<Entity> CachedRecursiveSelectedEntities { get; set; } = new List<Entity>();

        private object _saveLock = new object();

        private SQLiteConnection Connection
        {
            get
            {
                if (_connectionPath == null)
                {
                    _connection = new SQLiteConnection(StoragePathProvider.DatabasePath);
                }
                else
                {
                    if (_connection == null)
                        _connection = new SQLiteConnection(_connectionPath);
                }

                return _connection;
            }
        }

        public bool Error { get; private set; }

        public SQLiteSession(ILogger logger) : this(null, logger)
        {
        }

        public SQLiteSession(string path, ILogger logger)
        {
            _logger = logger;
            _connectionPath = path;
            int tries = 0;
        retry:
            try
            {
                InitConnection(path);
            }
            catch (SQLiteException ex)
            {
                if (ex.Message.Contains("database is locked"))
                {
                    tries++;

                    if (tries < 5)
                    {
                        _logger.LogInformation("Failed to save database, database is locked, retry.");
                        Thread.Sleep(50);
                        goto retry;
                    }

                    _logger.LogError("Failed to save database after several attempts", ex);
                }
            }
        }

        private void InitConnection(string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                var con = Connection;
            }
        }

        public void SelectRecursive(Entity ent, bool ignoreCache = false)
        {
            if (!ignoreCache)
            {
                var cachedEntity = CachedRecursiveSelectedEntities.FirstOrDefault(x => x.Id == ent.Id && x.GetType() == ent.GetType());

                if (cachedEntity != null)
                {
                    ent = cachedEntity;
                    return;
                }
            }

            var properties = ent.GetType().GetProperties();

            var oneToOnes = properties
                .Select(x => new { OneToOne = x.GetCustomAttributeOrDefault<OneToOne>(), PropertyInfo = x })
                .Where(x => x.OneToOne != null);

            var oneToManys = properties
                .Select(x => new { OneToMany = x.GetCustomAttributeOrDefault<OneToMany>(), PropertyInfo = x })
                .Where(x => x.OneToMany != null);

            var selectedProperties = oneToOnes.Select(x => x.PropertyInfo).ToList();
            selectedProperties.AddRange(oneToManys.Select(x => x.PropertyInfo));

            foreach (var property in selectedProperties)
            {
                var oneToMany = (OneToMany)property.GetCustomAttributeOrDefault(typeof(OneToMany));
                var oneToOne = (OneToOne)property.GetCustomAttributeOrDefault(typeof(OneToOne));

                if (oneToOne == null && oneToMany == null)
                    continue;

                if (oneToOne != null && oneToMany != null)
                    throw new InvalidOperationException("Illegal Multiple Attributes on Property (OneToOne and OneToMany)");

                TableAttribute foreignEntityTableAttribute = null;
                PropertyInfo foreignEntityIdProperty = null;
                PropertyInfo entityIdProperty = null;

                TableMapping tableMapping = null;

                if (oneToOne != null)
                {
                    foreignEntityTableAttribute = (TableAttribute)property.PropertyType.GetCustomAttributeOrDefault(typeof(TableAttribute));
                    foreignEntityIdProperty = property.PropertyType.GetProperties().First(x => x.Name == oneToOne.ForeignEntityIdProperty);
                    entityIdProperty = properties.First(x => x.Name == oneToOne.EntityIdProperty);

                    tableMapping = Connection.GetMapping(property.PropertyType);

                    List<object> list = new List<object>();
                    try
                    {
                        list = Query(ent, foreignEntityTableAttribute, foreignEntityIdProperty, entityIdProperty,
                            tableMapping);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"Cannot select one to one entity", e);
                        throw;
                    }

                    if (list.Count == 1)
                    {
                        Entity selectedEntity = (Entity)list[0];
                        property.SetValue(ent, selectedEntity);

                        SelectRecursive(selectedEntity);
                    }
                    else if (list.Count == 0)
                    {
                        _logger.LogInformation("Tried to select one to one entity, did not find any");
                        property.SetValue(ent, null);
                    }

                    //this case is not possible due to sqlite library who creates a unique constraint to pk and also change the id of inserted if it fails insert...
                    //else 
                    //    throw new InvalidOperationException(
                    //        $"Duplicate key with type:{foreignEntityIdProperty.PropertyType.Name} qry:{foreignEntityIdProperty.Name}={entityIdProperty.GetValue(ent)}");
                }

                if (oneToMany != null)
                {
                    foreignEntityTableAttribute = (TableAttribute)property.PropertyType.GetGenericArguments()[0].GetCustomAttributeOrDefault(typeof(TableAttribute));
                    foreignEntityIdProperty = property.PropertyType.GetGenericArguments()[0].GetProperties().FirstOrDefault(x => x.Name == oneToMany.EntityIdProperty);
                    entityIdProperty = properties.First(x => x.Name == oneToMany.ForeignEntityIdProperty);

                    tableMapping = Connection.GetMapping(property.PropertyType.GetGenericArguments()[0]);

                    var list = Query(ent, foreignEntityTableAttribute, foreignEntityIdProperty, entityIdProperty, tableMapping);
                    var currentList = (IList)property.GetValue(ent);

                    if (currentList == null)
                        throw new InvalidOperationException("Property needs an instance of type list<EntityImplementation>");

                    currentList.Clear();
                    list.ForEach(x =>
                    {
                        currentList.Add(x);
                        SelectRecursive((Entity)x);
                    });

                    property.SetValue(ent, currentList);
                }
            }

            CachedRecursiveSelectedEntities.Add(ent);
        }

        private List<object> Query(Entity ent, TableAttribute foreignEntityTableAttribute, PropertyInfo foreignEntityIdProperty, PropertyInfo entityIdProperty, TableMapping tableMapping)
        {
            var tableQuery = new TableQuery<Entity>(Connection).Where(x => x.Id == 0);

            var queryString = tableQuery.Table.GetByPrimaryKeySql;
            queryString = queryString.Replace(nameof(Entity), foreignEntityTableAttribute.Name);
            queryString = queryString.Replace(nameof(ent.Id), foreignEntityIdProperty.Name);

            try
            {
                return Connection.Query(tableMapping, queryString, entityIdProperty.GetValue(ent));
            }
            catch (SQLiteException e)
            {
                if (!e.Message.Contains("no such table"))
                    throw;
                else
                    _logger.LogInformation($"Warning: no such table.. sql:{queryString}");
            }
            catch (Exception e)
            {
                _logger.LogError($"Cannot select qry:{queryString}", e);
                throw;
            }

            return new List<object>();
        }

        #region select
        public List<T> SelectMany<T>(Expression<Func<T, bool>> whereExpression = null, int limit = 0) where T : Entity, new()
        {
            var result = Connection.Table<T>();

            if (limit > 0)
            {
                result = result.Take(limit);
            }
            if (limit < 0)
            {
                result = result.OrderByDescending(x => x.Id).Take(-limit);
            }

            if (whereExpression != null)
            {
                result = result.Where(whereExpression);
            }

            return result.ToList();
        }

        public TableQuery<T> SelectQuery<T>() where T : Entity, new()
        {
            return Connection.Table<T>();
        }

        public T SelectSingle<T>(Expression<Func<T, bool>> whereExpression) where T : Entity, new()
        {
            var selected = SelectSingleOrDefault(whereExpression);

            if (selected == null)
                throw new InvalidOperationException("Found no Entity for SelectSingle");

            return selected;
        }

        public T SelectSingleOrDefault<T>(Expression<Func<T, bool>> whereExpression) where T : Entity, new()
        {
            var count = Connection.Table<T>().Where(whereExpression).Count();

            if (count > 1)
                throw new InvalidOperationException("found to many Entities for SelectSingle");
            else if (count == 0)
                return null;

            return Connection.Table<T>().Where(whereExpression).First();
        }

        public T SelectLastOrDefault<T>(Expression<Func<T, bool>> whereExpression = null) where T : Entity, new()
        {
            if (whereExpression == null)
            {
                return Connection.Table<T>().LastOrDefault();
            }
            else
            {
                return Connection.Table<T>().Where(whereExpression).LastOrDefault();
            }
        }

        #endregion
        public void Save(IEnumerable<Entity> entities)
        {
            Save(entities, false);

            CloseOpenTransaction();
        }

        public void Save(Entity toSave)
        {
            Save(toSave, false);
        }

        private void Save(object toSave, bool beginTransaction)
        {
            if (beginTransaction && !Connection.IsInTransaction)
            {
                Connection.BeginTransaction();
                _logger.LogInformation("Opened New Transaction");
            }

            Retry:
            try
            {
                if (toSave is IEnumerable<Entity> enumerable)
                {
                    if (!(enumerable is List<Entity> list))
                        list = enumerable.ToList();

                    Connection.UpdateAll(list.Where(x => x.Id != 0));
                    Connection.InsertAll(list.Where(x => x.Id == 0));
                    list.ForEach(x => x.Changed = false);
                }

                else if (toSave is Entity entity)
                {
                    if (entity.Id == 0)
                        Connection.Insert(entity);
                    else
                        Connection.Update(entity);

                    entity.Changed = false;
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("no such table") || e.Message.Contains("has no column named"))
                {
                    if (toSave is IEnumerable<Entity> enumerable)
                        Connection.CreateTable(enumerable.First().GetType());
                    else
                        Connection.CreateTable(toSave.GetType());

                    goto Retry;
                }

                _logger.LogError("Entity Save failed", e);

                if (Connection.IsInTransaction)
                    Error = true;
            }
        }

        public void SaveRecursive<T>(List<T> entities) where T : Entity
        {
            Save(entities);

            entities.ForEach(x => SaveRecursive(x, true));
        }

        public void SaveRecursive(Entity toSave, bool alreadySavedBecauseOfList = false)
        {
            if (!alreadySavedBecauseOfList && (toSave.Id == 0 || toSave.Changed))
                Save(toSave);

            var properties = toSave.GetType().GetProperties();

            var oneToOnes = properties
                .Select(x => new { OneToOne = x.GetCustomAttributeOrDefault<OneToOne>(), PropertyInfo = x })
                .Where(x => x.OneToOne != null);

            var oneToManys = properties
                .Select(x => new { OneToMany = x.GetCustomAttributeOrDefault<OneToMany>(), PropertyInfo = x })
                .Where(x => x.OneToMany != null);

            foreach (var oneToOne in oneToOnes)
            {
                var entity = (Entity)oneToOne.PropertyInfo.GetValue(toSave);
                Save(entity, false);

                var entityIdProperty = properties.First(x => x.Name == oneToOne.OneToOne.EntityIdProperty);
                var foreignEntityIdProperty = entity.GetType().GetProperty(oneToOne.OneToOne.ForeignEntityIdProperty);

                entityIdProperty.SetValue(toSave, foreignEntityIdProperty.GetValue(entity));

                Save(toSave);
                SaveRecursive(entity);
            }

            foreach (var oneToMany in oneToManys)
            {
                var list = ((IEnumerable<Entity>)oneToMany.PropertyInfo.GetValue(toSave)).ToList();

                foreach (var item in list)
                {
                    var entityIdProperty = item.GetType().GetProperty(oneToMany.OneToMany.EntityIdProperty);
                    var foreignEntityIdProperty = properties.First(x => x.Name == oneToMany.OneToMany.ForeignEntityIdProperty);
                    entityIdProperty.SetValue(item, foreignEntityIdProperty.GetValue(toSave));
                }

                SaveRecursive(list);
            }
        }

        public int Count<T>(Expression<Func<T, bool>> whereExpression) where T : Entity, new()
        {
            var count = Connection.Table<T>().Where(whereExpression).Count();

            return count;
        }

        public void CloseOpenTransaction()
        {
            if (Connection.IsInTransaction && Error)
            {
                Connection.Rollback();
                _logger.LogInformation("Rollback Transaction");
            }

            if (Connection.IsInTransaction && !Error)
            {
                Connection.Commit();
                _logger.LogInformation("Commited Transaction");
            }
        }

        public void Dispose()
        {
            CloseOpenTransaction();

            Connection.Close();
            _connection = null;
        }
    }
}
