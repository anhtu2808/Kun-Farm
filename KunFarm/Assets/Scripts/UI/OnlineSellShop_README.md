# OnlineSellShop System

## Tổng quan
OnlineSellShop là hệ thống UI cho phép người chơi bán các vật phẩm trong inventory của mình và theo dõi lịch sử bán hàng.

## Cấu trúc hệ thống

### 1. OnlineSellShop_UI.cs
Script chính quản lý toàn bộ OnlineSellShop UI.

**Chức năng:**
- Quản lý việc mở/đóng shop
- Tích hợp PlayerSell_Scroll và SellShop_Scroll
- Theo dõi tổng thu nhập và số lượng items đã bán
- Reset dữ liệu shop

**Cách sử dụng:**
```csharp
// Mở shop
onlineSellShop.OpenShop();

// Đóng shop
onlineSellShop.CloseShop();

// Toggle shop
onlineSellShop.ToggleShop();

// Reset dữ liệu
onlineSellShop.ResetShopData();
```

### 2. PlayerSellScroll_UI.cs
Script quản lý PlayerSell_Scroll - hiển thị inventory items của player.

**Chức năng:**
- Hiển thị các vật phẩm trong inventory
- Hiển thị giá bán và số lượng
- Cho phép bán từng item hoặc bán tất cả
- Tự động refresh khi inventory thay đổi

**Cấu trúc UI:**
- Container: `Player_Items`
- Mỗi item slot cần có:
  - Icon (Image)
  - Quantity text (TextMeshProUGUI)
  - Price text (TextMeshProUGUI)
  - Sell button (Button)

### 3. SellShopScroll_UI.cs
Script quản lý SellShop_Scroll - hiển thị lịch sử bán hàng.

**Chức năng:**
- Lưu trữ lịch sử các vật phẩm đã bán
- Hiển thị thông tin: tên item, số lượng, thu nhập, thời gian
- Tự động highlight items mới bán
- Xóa lịch sử

**Cấu trúc UI:**
- Container: `SellShop_Items`
- Mỗi sold item slot cần có:
  - Icon (Image)
  - Item name (TextMeshProUGUI)
  - Quantity (TextMeshProUGUI)
  - Earnings (TextMeshProUGUI)
  - Time (TextMeshProUGUI)

### 4. OnlineSellShopManager.cs
Script quản lý việc mở/đóng OnlineSellShop và tích hợp với hệ thống hiện tại.

**Chức năng:**
- Quản lý input (phím tắt)
- Tích hợp với Inventory và Shop hiện tại
- Ngăn chặn xung đột UI

**Phím tắt:**
- `O`: Mở/đóng OnlineSellShop
- `Tab`: Mở/đóng Inventory
- `B`: Mở/đóng Regular Shop
- `ESC`: Đóng tất cả UI

### 5. PlayerSellItemSlot_UI.cs
Script cho individual item slot trong PlayerSell_Scroll.

**Chức năng:**
- Hiển thị thông tin item
- Xử lý click bán
- Animation khi bán

### 6. SoldItemSlot_UI.cs
Script cho individual sold item slot trong SellShop_Scroll.

**Chức năng:**
- Hiển thị thông tin item đã bán
- Animation khi thêm item mới
- Highlight items mới bán

## Cách setup trong Unity

### 1. Tạo OnlineSellShop UI
1. Trong Unity, vào menu `Assets > Create > UI > OnlineSellShop Prefab`
2. Prefab sẽ được tạo tại `Assets/Prefabs/UI/OnlineSellShop.prefab`
3. Kéo prefab vào scene

### 2. Setup References
1. Chọn OnlineSellShop GameObject
2. Trong Inspector, gán các references:
   - **Shop Panel**: GameObject chính của shop
   - **Close Button**: Nút đóng shop
   - **Refresh Button**: Nút refresh (tùy chọn)
   - **Player Sell Scroll**: Reference đến PlayerSellScroll_UI
   - **Sell Shop Scroll**: Reference đến SellShopScroll_UI
   - **Total Earnings Text**: Text hiển thị tổng thu nhập
   - **Items Sold Text**: Text hiển thị số items đã bán
   - **Shop Manager**: Reference đến ShopManager
   - **Player**: Reference đến Player

### 3. Setup PlayerSell_Scroll
1. Chọn PlayerSellScroll GameObject
2. Gán references:
   - **Shop Manager**: Reference đến ShopManager
   - **Player**: Reference đến Player
   - **Player Items Container**: Transform chứa các item slots
   - **Sell All Button**: Nút bán tất cả (tùy chọn)
   - **Sell All Text**: Text của nút bán tất cả (tùy chọn)

### 4. Setup SellShop_Scroll
1. Chọn SellShopScroll GameObject
2. Gán references:
   - **Shop Manager**: Reference đến ShopManager
   - **Player**: Reference đến Player
   - **Sold Items Container**: Transform chứa các sold item slots
   - **Clear History Button**: Nút xóa lịch sử (tùy chọn)
   - **Total History Text**: Text hiển thị thông tin lịch sử (tùy chọn)

### 5. Setup OnlineSellShopManager
1. Tạo empty GameObject và thêm OnlineSellShopManager script
2. Gán references:
   - **Online Sell Shop**: Reference đến OnlineSellShop_UI
   - **Inventory UI**: Reference đến InventoryUI
   - **Regular Shop**: Reference đến Shop_UI
   - **Open Online Sell Shop Button**: Nút mở shop (tùy chọn)
   - **Close Online Sell Shop Button**: Nút đóng shop (tùy chọn)

## Cấu trúc UI Hierarchy

```
OnlineSellShop
├── Background
├── Header
│   ├── Title
│   └── CloseButton
├── MainContent
│   ├── PlayerSellSection
│   │   ├── Title
│   │   ├── Player_Items (ScrollRect)
│   │   │   ├── Viewport
│   │   │   └── Content (GridLayoutGroup)
│   │   └── SellAllButton
│   └── SellShopSection
│       ├── Title
│       ├── SellShop_Items (ScrollRect)
│       │   ├── Viewport
│       │   └── Content (VerticalLayoutGroup)
│       └── ClearHistoryButton
└── Footer
    └── InfoText
```

## Tùy chỉnh

### 1. Thay đổi phím tắt
Trong OnlineSellShopManager, thay đổi các biến:
```csharp
public KeyCode openShopKey = KeyCode.O;
public KeyCode toggleInventoryKey = KeyCode.Tab;
public KeyCode toggleRegularShopKey = KeyCode.B;
```

### 2. Thay đổi màu sắc
Trong các script UI, thay đổi các biến Color:
```csharp
public Color normalColor = Color.white;
public Color sellableColor = Color.green;
public Color unsellableColor = Color.red;
```

### 3. Thay đổi giới hạn lịch sử
Trong SellShopScroll_UI:
```csharp
public int maxHistoryItems = 50;
```

## Lưu ý

1. **Tích hợp với ShopManager**: Hệ thống sử dụng ShopManager hiện tại để xử lý logic bán hàng
2. **Tích hợp với Inventory**: Tự động refresh khi inventory thay đổi
3. **Performance**: Lịch sử bán hàng được giới hạn để tránh memory leak
4. **Persistence**: Lịch sử bán hàng không được lưu tự động, cần implement save/load system

## Troubleshooting

### 1. Shop không mở được
- Kiểm tra references trong OnlineSellShop_UI
- Đảm bảo ShopManager và Player đã được gán

### 2. Items không hiển thị
- Kiểm tra Player_Items container
- Đảm bảo inventory có items
- Kiểm tra ShopData có thông tin items

### 3. Không bán được items
- Kiểm tra ShopData có `canSell = true` cho items
- Kiểm tra `sellPrice > 0`
- Đảm bảo Player có items trong inventory

### 4. Lịch sử không hiển thị
- Kiểm tra SellShop_Items container
- Đảm bảo items đã được bán thành công
- Kiểm tra maxHistoryItems không quá nhỏ 