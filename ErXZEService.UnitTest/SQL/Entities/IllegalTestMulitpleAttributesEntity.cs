using ErXZEService.Models;
using ErXZEService.Services.SQL.Attributes;
using SQLite;
using System.Collections.Generic;

namespace ErXZEService.UnitTest.SQL.Entities
{
    [Table(nameof(IllegalTestMulitpleAttributesEntity))]
    public class IllegalTestMulitpleAttributesEntity : Entity
    {
        public long MySelectRecursiveTestEntityId { get; set; }

        [Ignore]
        [OneToOne(nameof(MySelectRecursiveTestEntityId))]
        [OneToMany(nameof(MySelectRecursiveTestEntityId))]
        public MySelectRecursiveTestOneToOneEntity OneToOneEntity { get; set; }
    }
}
