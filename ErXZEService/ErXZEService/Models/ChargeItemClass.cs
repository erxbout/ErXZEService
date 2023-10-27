using SQLite;

namespace ErXZEService.Models
{
    [Table(nameof(ChargeItemGroup))]
    public class ChargeItemGroup : Entity
    {
        public string Caption { get; set; }
    }
}
