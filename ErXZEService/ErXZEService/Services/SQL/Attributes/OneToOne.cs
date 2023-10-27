using ErXZEService.Models;
using System;

namespace ErXZEService.Services.SQL.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OneToOne : Attribute
    {
        public string EntityIdProperty { get; }
        public string ForeignEntityIdProperty { get; }

        public OneToOne(string entityIdProperty, string foreignEntityIdProperty = nameof(Entity.Id))
        {
            EntityIdProperty = entityIdProperty;
            ForeignEntityIdProperty = foreignEntityIdProperty;
        }
    }
}
