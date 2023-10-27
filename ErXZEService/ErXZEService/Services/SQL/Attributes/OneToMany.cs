using ErXZEService.Models;
using System;

namespace ErXZEService.Services.SQL.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class OneToMany : Attribute
    {
        public string EntityIdProperty { get; }
        public string ForeignEntityIdProperty { get; }

        public OneToMany(string entityIdProperty, string foreignEntityIdProperty = nameof(Entity.Id))
        {
            EntityIdProperty = entityIdProperty;
            ForeignEntityIdProperty = foreignEntityIdProperty;
        }
    }
}
