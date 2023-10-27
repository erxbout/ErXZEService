using ErXZEService.Models;
using ErXZEService.Services.SQL.Attributes;
using SQLite;
using System.Collections.Generic;

namespace ErXZEService.UnitTest.SQL.Entities
{
    [Table(nameof(IllegalTestOneToManyListIsNotInstancedEntity))]
    public class IllegalTestOneToManyListIsNotInstancedEntity : Entity
    {
        public long MySelectRecursiveTestEntityId { get; set; }

        [Ignore]
        [OneToMany(nameof(MySelectRecursiveTestOneToManyEntity.MySelectRecursiveTestEntityId))]
        public List<MySelectRecursiveTestOneToManyEntity> OneToManyEntities { get; set; }
    }
}
