using ErXZEService.Models;
using SQLite;

namespace ErXZEService.UnitTest.SQL.Entities
{
    [Table(nameof(MySelectRecursiveTestOneToOneEntity))]
    public class MySelectRecursiveTestOneToOneEntity : Entity
    {
        public string Text { get; set; }
    }
}
