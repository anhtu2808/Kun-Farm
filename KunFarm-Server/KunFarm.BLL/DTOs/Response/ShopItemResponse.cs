using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KunFarm.BLL.DTOs.Response
{
    public class ShopItemResponse
    {
        public int SlotId { get; set; }
        public string CollectableType { get; set; }
        public string ItemName { get; set; }
        public string Icon { get; set; }

        public bool CanBuy { get; set; }
        public int BuyPrice { get; set; }
        public int? StockLimit { get; set; }
        public int? CurrentStock { get; set; }
    }
}
