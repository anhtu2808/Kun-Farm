# SoldItemSlot Prefab Setup Guide

## ✅ **Tạo Prefab cho Sold Items**

### **1. Tạo GameObject Structure**
```
SoldItemSlot (GameObject với SoldItemSlot_UI script)
├── ItemIcon (Image)
├── ItemName (TextMeshPro - Text)
├── Quantity (TextMeshPro - Text) 
├── Price (TextMeshPro - Text)
├── Status (TextMeshPro - Text)
└── ClaimButton (Button)
    └── ButtonText (TextMeshPro - Text)
```

### **2. Assign References trong SoldItemSlot_UI**
- **ItemIcon**: Drag ItemIcon Image component
- **ItemNameText**: Drag ItemName TextMeshPro component  
- **QuantityText**: Drag Quantity TextMeshPro component
- **PriceText**: Drag Price TextMeshPro component
- **StatusText**: Drag Status TextMeshPro component
- **ClaimButton**: Drag ClaimButton Button component

### **3. Layout Suggestions**
```
[Icon] ItemName        Quantity: x5    Price: 150G    Status: Đã bán    [Claim 150G]
[🍎]   Apple           x5              150G           Đã bán            [Claim 150G]
[🌾]   Wheat           x10             200G           Đang bán          (button ẩn)
```

### **4. Assign vào OnlSellShopManager**
- **soldItemSlotPrefab**: Drag prefab vừa tạo vào field này
- **sellSlotContainer**: Transform container để chứa các slots

### **5. Logic hoạt động**
1. **Bấm O**: Load tất cả items đã đăng bán từ API
2. **canBuy = true**: Hiển thị "Đang bán", ẩn button Claim
3. **canBuy = false**: Hiển thị "Đã bán", hiện button "Claim XG"
4. **Click Claim**: Gọi API claim money cho item đó
5. **Sau claim**: Refresh lại danh sách items

### **6. Test Flow**
1. Bán item online → Item xuất hiện với status "Đang bán"
2. Người khác mua → Item chuyển status "Đã bán" + button "Claim"
3. Click Claim → Nhận tiền + item biến mất khỏi danh sách

✅ **Không còn auto-claim khi mở shop!** 