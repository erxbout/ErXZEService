using ErXZEService.Models;
using ErXZEService.Services.SQL.Attributes;
using SQLite;
using System.Collections.Generic;

namespace ErXZEService.UnitTest.SQL.Entities
{
    public class IllegalTestEntityHasNoTableAttributeEntity : Entity
    {
        public long TestId { get; set; }
    }
}
