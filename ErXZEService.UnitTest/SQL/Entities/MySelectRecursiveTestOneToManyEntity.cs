using ErXZEService.Models;
using SQLite;

namespace ErXZEService.UnitTest.SQL.Entities
{
    [Table(nameof(MySelectRecursiveTestOneToManyEntity))]
    public class MySelectRecursiveTestOneToManyEntity : Entity
    {
        public long MySelectRecursiveTestEntityId { get; set; }

        public string Text { get; set; }
    }
}
