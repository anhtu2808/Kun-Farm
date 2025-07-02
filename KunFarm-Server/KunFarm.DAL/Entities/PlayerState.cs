using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KunFarm.DAL.Entities
{
    public class PlayerState
    {
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }
        
        public int Money { get; set; } = 0;
        
        public float PosX { get; set; } = 0f;
        
        public float PosY { get; set; } = 0f;
        
        public float PosZ { get; set; } = 0f;
        
        public float Health { get; set; } = 100f;
        
        public float Hunger { get; set; } = 100f;
        
        public DateTime LastSaved { get; set; } = DateTime.Now;
        
        // Navigation property
        public User User { get; set; } = null!;

        public ICollection<InventorySlot> InventorySlots { get; set; } = new List<InventorySlot>();
        public ICollection<PlayerRegularShopSlot> PlayerRegularShopSlots { get; set; } = new List<PlayerRegularShopSlot>();

        public ICollection<OnlineShopSlot> SellingOnlineShopSlots { get; set; }
    = new List<OnlineShopSlot>();

        public ICollection<OnlineShopSlot> BuyingOnlineShopSlots { get; set; }
            = new List<OnlineShopSlot>();
    }
} 