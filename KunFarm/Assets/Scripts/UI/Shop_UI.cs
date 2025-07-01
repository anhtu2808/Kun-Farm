// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public class Shop_UI : MonoBehaviour
// {
//     [Header("UI References")]
//     public GameObject shopPanel;
//     public Transform shopSlotsContainer;
//     public Button closeButton;
//     public Button refreshButton;
    
//     [Header("Shop Slot Prefab")]
//     public GameObject shopSlotPrefab;
    
//     [Header("Shop System")]
//     public ShopManager shopManager;
    
//     // Shop slots
//     private List<ShopSlot_UI> shopSlots = new List<ShopSlot_UI>();
    
//     // Property để kiểm tra Shop có đang mở không
//     public bool IsOpen => shopPanel.activeSelf;

//     void Start()
//     {
//         // Đảm bảo shop luôn tắt khi bắt đầu
//         shopPanel.SetActive(false);
        
//         // Setup buttons
//         if (closeButton != null)
//             closeButton.onClick.AddListener(CloseShop);
            
//         if (refreshButton != null)
//             refreshButton.onClick.AddListener(RefreshShop);
        
//         // Tự động tìm ShopManager nếu chưa assign
//         if (shopManager == null)
//             shopManager = FindObjectOfType<ShopManager>();
            
//         // Subscribe to shop events
//         if (shopManager != null)
//             shopManager.OnShopUpdated += RefreshShopSlots;
            
//         // Initialize shop slots
//         InitializeShopSlots();
//     }

//     void Update()
//     {
//         if (Input.GetKeyDown(KeyCode.B))
//         {
//             ToggleShop();
//         }
        
//         // Escape key để đóng shop
//         if (Input.GetKeyDown(KeyCode.Escape) && IsOpen)
//         {
//             CloseShop();
//         }
//     }

//     void OnDestroy()
//     {
//         // Unsubscribe from events
//         if (shopManager != null)
//             shopManager.OnShopUpdated -= RefreshShopSlots;
//     }

//     /// <summary>
//     /// Initialize shop slots từ ShopData
//     /// </summary>
//     private void InitializeShopSlots()
//     {
//         if (shopManager == null || shopManager.shopData == null || shopSlotsContainer == null)
//         {
//             Debug.LogWarning("Shop_UI: Missing references for shop initialization!");
//             return;
//         }

//         // Debug trước khi clear
//         DebugSlotInfo("BEFORE Clear");

//         // Clear existing slots
//         ClearShopSlots();
        
//                 // ✅ ĐỢI MỘT FRAME ĐỂ DESTROY HOÀN THÀNH
//         StartCoroutine(CreateSlotsAfterClear());
//     }
    
//     /// <summary>
//     /// Tạo slots sau khi clear hoàn thành
//     /// </summary>
//     private System.Collections.IEnumerator CreateSlotsAfterClear()
//     {
//         // Đợi 1 frame để Destroy() hoàn thành
//         yield return null;
        
//         // Debug sau khi clear
//         DebugSlotInfo("AFTER Clear");
        
//         // ✅ ĐẢM BẢO LAYOUT GROUP SETTINGS ĐÚNG
//         EnsureLayoutSettings();

//         // ✅ CHỈ TẠO SLOTS CHO ITEMS CÓ THỂ MUA
//         int slotIndex = 0;
//         foreach (var shopItem in shopManager.shopData.shopItems)
//         {
//             // Chỉ hiển thị items có thể mua trong shop
//             if (shopItem.showInShop && shopItem.canBuy)
//             {
//                 CreateShopSlot(shopItem, slotIndex);
//                 slotIndex++;
//             }
//         }
        
//         // Debug sau khi tạo
//         DebugSlotInfo("AFTER Create");
        
//         Debug.Log($"Shop_UI: Created {slotIndex} shop slots successfully!");
//     }
    
//     /// <summary>
//     /// Debug thông tin về slots trong container
//     /// </summary>
//     private void DebugSlotInfo(string phase)
//     {
//         if (shopSlotsContainer == null) return;
        
//         int totalChildren = shopSlotsContainer.childCount;
//         int shopSlotChildren = 0;
        
//         for (int i = 0; i < totalChildren; i++)
//         {
//             Transform child = shopSlotsContainer.GetChild(i);
//             if (child.name.Contains("Shop_Slot"))
//             {
//                 shopSlotChildren++;
//             }
//         }
        
//         Debug.Log($"Shop_UI [{phase}]: Total children = {totalChildren}, Shop_Slot children = {shopSlotChildren}, Dynamic slots in list = {shopSlots.Count}");
//     }

//     /// <summary>
//     /// Đảm bảo Layout Group settings đúng cho container
//     /// </summary>
//     private void EnsureLayoutSettings()
//     {
//         // Kiểm tra và setup Grid Layout Group nếu cần
//         var gridLayout = shopSlotsContainer.GetComponent<UnityEngine.UI.GridLayoutGroup>();
//         if (gridLayout != null)
//         {
//             // Đảm bảo start axis đúng hướng
//             gridLayout.startAxis = UnityEngine.UI.GridLayoutGroup.Axis.Horizontal;
//             gridLayout.startCorner = UnityEngine.UI.GridLayoutGroup.Corner.UpperLeft;
//             gridLayout.childAlignment = TextAnchor.UpperLeft;
//         }
        
//         // Kiểm tra Vertical Layout Group
//         var verticalLayout = shopSlotsContainer.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
//         if (verticalLayout != null)
//         {
//             verticalLayout.childAlignment = TextAnchor.UpperCenter;
//             verticalLayout.reverseArrangement = false; // ✅ ĐẢM BẢO KHÔNG BỊ REVERSE
//         }
        
//         // Kiểm tra Horizontal Layout Group
//         var horizontalLayout = shopSlotsContainer.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
//         if (horizontalLayout != null)
//         {
//             horizontalLayout.childAlignment = TextAnchor.MiddleLeft;
//             horizontalLayout.reverseArrangement = false; // ✅ ĐẢM BẢO KHÔNG BỊ REVERSE
//         }
        
//         Debug.Log($"Shop_UI: Layout settings checked for {shopSlotsContainer.name}");
//     }

//     /// <summary>
//     /// Tạo một shop slot mới
//     /// </summary>
//     private void CreateShopSlot(ShopItem shopItem, int index)
//     {
//         if (shopSlotPrefab == null || shopSlotsContainer == null)
//         {
//             Debug.LogError("Shop_UI: Missing shopSlotPrefab or shopSlotsContainer!");
//             return;
//         }

//         GameObject slotObj = Instantiate(shopSlotPrefab, shopSlotsContainer);
        
//         // ✅ ĐẶT SLOT THEO THỨ TỰ CHỈ ĐỊNH
//         slotObj.transform.SetSiblingIndex(index);
        
//         ShopSlot_UI slotUI = slotObj.GetComponent<ShopSlot_UI>();
        
//         if (slotUI != null)
//         {
//             slotUI.Initialize(shopItem, shopManager, shopManager.player);
            
//             // ✅ CHỈ SUBSCRIBE BUY EVENT VÌ SHOP CHỈ DÀNH CHO MUA
//             slotUI.OnBuyClicked += OnSlotBuyClicked;
//             // Xóa sell events vì shop chỉ dành cho mua
//             // slotUI.OnSellClicked += OnSlotSellClicked;
//             // slotUI.OnSellAllClicked += OnSlotSellAllClicked;
            
//             shopSlots.Add(slotUI);
//         }
//         else
//         {
//             Debug.LogError("Shop_UI: shopSlotPrefab doesn't have ShopSlot_UI component!");
//             Destroy(slotObj);
//         }
//     }

//     /// <summary>
//     /// Clear tất cả shop slots (bao gồm cả static slots)
//     /// </summary>
//     private void ClearShopSlots()
//     {
//         // ✅ XÓA TẤT CẢ DYNAMIC SLOTS TRONG LIST
//         foreach (var slot in shopSlots)
//         {
//             if (slot != null)
//             {
//                 // ✅ CHỈ UNSUBSCRIBE BUY EVENT
//                 slot.OnBuyClicked -= OnSlotBuyClicked;
//                 // Không cần unsubscribe sell events vì shop chỉ dành cho mua
//                 // slot.OnSellClicked -= OnSlotSellClicked;
//                 // slot.OnSellAllClicked -= OnSlotSellAllClicked;
                
//                 Destroy(slot.gameObject);
//             }
//         }
//         shopSlots.Clear();
        
//         // ✅ XÓA TẤT CẢ STATIC SLOTS TRONG CONTAINER
//         if (shopSlotsContainer != null)
//         {
//             // Lấy tất cả children và xóa
//             Transform[] children = new Transform[shopSlotsContainer.childCount];
//             for (int i = 0; i < shopSlotsContainer.childCount; i++)
//             {
//                 children[i] = shopSlotsContainer.GetChild(i);
//             }
            
//             foreach (Transform child in children)
//             {
//                 // Kiểm tra xem có phải shop slot không
//                 if (child.name.Contains("Shop_Slot"))
//                 {
//                     Debug.Log($"Shop_UI: Destroying static slot: {child.name}");
//                     Destroy(child.gameObject);
//                 }
//             }
//         }
//     }

//     /// <summary>
//     /// Refresh tất cả shop slots
//     /// </summary>
//     private void RefreshShopSlots()
//     {
//         foreach (var slot in shopSlots)
//         {
//             if (slot != null)
//                 slot.Refresh();
//         }
//     }

//     /// <summary>
//     /// Handle buy button click - CHỈ CHO MUA
//     /// </summary>
//     private void OnSlotBuyClicked(ShopSlot_UI slot)
//     {
//         if (slot != null)
//         {
//             slot.BuyItem(1);
//             Debug.Log($"Shop_UI: Player mua {slot.GetShopItem()?.itemName}");
//         }
//     }

//     // ✅ DISABLED SELL EVENT HANDLERS VÌ SHOP CHỈ DÀNH CHO MUA
//     // ✅ BÁN ITEMS SẼ ĐƯỢC XỬ LÝ TỪ INVENTORY/PLAYER_SCROLL UI
//     /*
//     /// <summary>
//     /// Handle sell button click - DISABLED cho shop
//     /// </summary>
//     private void OnSlotSellClicked(ShopSlot_UI slot)
//     {
//         if (slot != null)
//         {
//             slot.SellItem(1);
//         }
//     }

//     /// <summary>
//     /// Handle sell all button click - DISABLED cho shop
//     /// </summary>
//     private void OnSlotSellAllClicked(ShopSlot_UI slot)
//     {
//         if (slot != null)
//         {
//             slot.SellAllItems();
//         }
//     }
//     */

//     public void ToggleShop()
//     {
//         if (!shopPanel.activeSelf)
//         {
//             OpenShop();
//         }
//         else
//         {
//             CloseShop();
//         }
//     }

//     public void OpenShop()
//     {
//         shopPanel.SetActive(true);
//         RefreshShopSlots();
//     }

//     public void CloseShop()
//     {
//         shopPanel.SetActive(false);
//     }

//     /// <summary>
//     /// Refresh shop (có thể dùng để reset stock)
//     /// </summary>
//     public void RefreshShop()
//     {
//         if (shopManager != null)
//         {
//             shopManager.RefreshShopStock();
//         }
//     }

//     /// <summary>
//     /// Force refresh shop UI
//     /// </summary>
//     public void ForceRefresh()
//     {
//         InitializeShopSlots();
//     }
// }
