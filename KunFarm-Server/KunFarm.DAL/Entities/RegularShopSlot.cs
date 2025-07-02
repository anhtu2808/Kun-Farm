using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KunFarm.DAL.Entities
{
    public class RegularShopSlot : BaseEntity
    {
        [Required]
        public bool CanBuy { get; set; }

        [Required]
        public int BuyPrice { get; set; }

        public int? StockLimit { get; set; }

        [Required]
        public int ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item Item { get; set; }

        public ICollection<PlayerRegularShopSlot> PlayerRegularShopSlots { get; set; } = new List<PlayerRegularShopSlot>();
    }
}
