using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KunFarm.DAL.Entities
{
    public class OnlineShopSlot : BaseEntity
    {
        [Required]
        public bool CanBuy { get; set; }

        [Required]
        public int BuyPrice { get; set; }

        public int SellerId { get; set; }
        public PlayerState Seller { get; set; } = null!;

        public int BuyerId { get; set; }
        public PlayerState Buyer { get; set; } = null!;

        public int ItemId { get; set; }
        public Item Item { get; set; } = null!;

    }
}
