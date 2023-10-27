using ErXZEService.Models;
using ErXZEService.Services.SQL.Attributes;
using SQLite;
using System.Collections.Generic;

namespace ErXZEService.UnitTest.SQL.Entities
{
    [Table(nameof(MySelectRecursiveTestEntity))]
    public class MySelectRecursiveTestEntity : Entity
    {
        public long MySelectRecursiveTestEntityId { get; set; }

        [Ignore]
        [OneToOne(nameof(MySelectRecursiveTestEntityId))]
        public MySelectRecursiveTestOneToOneEntity OneToOneEntity { get; set; }

        [Ignore]
        [OneToMany(nameof(MySelectRecursiveTestOneToManyEntity.MySelectRecursiveTestEntityId))]
        public List<MySelectRecursiveTestOneToManyEntity> OneToManyEntities { get; set; } = new List<MySelectRecursiveTestOneToManyEntity>();
    }
}
