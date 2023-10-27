using ErXZEService.Services;
using ErXZEService.Services.Log;
using ErXZEService.Services.SQL;
using SQLite;

namespace ErXZEService.Models
{
    public abstract class Entity
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        [Ignore]
        public bool Changed { get; set; }

        public virtual void Save()
        {
            using (var session = new SQLiteSession(IoC.Resolve<ILogger>()))
                Save(session);
        }

        public virtual void SaveRecursive()
        {
            using (var session = new SQLiteSession(IoC.Resolve<ILogger>()))
                SaveRecursive(session);
        }

        public virtual void Save(SQLiteSession session)
        {
            session.Save(this);
        }

        public virtual void SaveRecursive(SQLiteSession session)
        {
            session.SaveRecursive(this);
        }
    }
}
