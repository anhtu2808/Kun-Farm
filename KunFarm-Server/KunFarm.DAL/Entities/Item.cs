using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KunFarm.DAL.Entities
{
    public class Item : BaseEntity
    {
        [Required]
        [StringLength(50)]
        public string CollectableType { get; set; }

        [Required]
        [StringLength(100)]
        public string ItemName { get; set; }

        [Required]
        [StringLength(200)]
        public string Icon { get; set; }

        public RegularShopSlot RegularShop { get; set; }
    }
}
