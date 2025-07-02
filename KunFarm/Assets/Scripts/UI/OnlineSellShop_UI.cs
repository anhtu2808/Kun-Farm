// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using System.Collections;

// /// <summary>
// /// Main controller cho OnlineSellShop UI
// /// Quản lý PlayerSell_Scroll và SellShop_Scroll
// /// </summary>
// public class OnlineSellShop_UI : MonoBehaviour
// {
//     [Header("UI References")]
//     public GameObject shopPanel;
//     public Button closeButton;
//     public Button refreshButton;

//     [Header("Player Sell Section")]
//     public PlayerSellScroll_UI playerSellScroll;

//     [Header("Sell Shop Section")]
//     public SellShopScroll_UI sellShopScroll;

//     [Header("Info Panel")]
//     public TextMeshProUGUI totalEarningsText;
//     public TextMeshProUGUI itemsSoldText;

//     [Header("System References")]
//     public ShopManager shopManager;
//     public Player player;
//     [Header("Hotkeys")]
//     public KeyCode toggleKey = KeyCode.O;
//     public KeyCode closeKey = KeyCode.Escape;

//     // Data tracking
//     private int totalEarnings = 0;
//     private int totalItemsSold = 0;

//     // Property để kiểm tra shop có đang mở không
//     public bool IsOpen => shopPanel.activeSelf;

//     private void Awake()
//     {
//         // Auto-find references nếu chưa assign
//         if (shopManager == null)
//             shopManager = FindObjectOfType<ShopManager>();

//         if (player == null)
//             player = FindObjectOfType<Player>();
//     }

//     private void Start()
//     {
//         // Đảm bảo shop luôn tắt khi bắt đầu
//         if (shopPanel != null)
//             shopPanel.SetActive(false);

//         // Setup buttons
//         if (closeButton != null)
//             closeButton.onClick.AddListener(CloseShop);

//         if (refreshButton != null)
//             refreshButton.onClick.AddListener(RefreshShop);

//         // Subscribe to events
//         if (shopManager != null)
//             shopManager.OnShopUpdated += OnShopUpdated;

//         // Initialize sub-components
//         InitializeComponents();
//     }

//     private void Update()
//     {
//         if (Input.GetKeyDown(toggleKey))
//             ToggleShop();

//         if (Input.GetKeyDown(closeKey) && IsOpen)
//             CloseShop();
//     }

//     private void OnDestroy()
//     {
//         // Unsubscribe from events
//         if (shopManager != null)
//             shopManager.OnShopUpdated -= OnShopUpdated;
//     }

//     /// <summary>
//     /// Khởi tạo các component con
//     /// </summary>
//     private void InitializeComponents()
//     {
//         // Initialize PlayerSell_Scroll
//         if (playerSellScroll != null)
//         {
//             playerSellScroll.Initialize(shopManager, player);
//             playerSellScroll.OnItemSold += OnPlayerItemSold;
//         }

//         // Initialize SellShop_Scroll
//         if (sellShopScroll != null)
//         {
//             sellShopScroll.Initialize(shopManager, player);
//         }
//     }

//     /// <summary>
//     /// Mở OnlineSellShop
//     /// </summary>
//     public void OpenShop()
//     {
//         if (shopPanel != null)
//         {
//             shopPanel.SetActive(true);
//             RefreshAll();
//         }
//     }

//     /// <summary>
//     /// Đóng OnlineSellShop
//     /// </summary>
//     public void CloseShop()
//     {
//         if (shopPanel != null)
//         {
//             shopPanel.SetActive(false);
//         }
//     }

//     /// <summary>
//     /// Toggle shop open/close
//     /// </summary>
//     public void ToggleShop()
//     {
//         if (IsOpen)
//             CloseShop();
//         else
//             OpenShop();
//     }

//     /// <summary>
//     /// Refresh toàn bộ shop
//     /// </summary>
//     public void RefreshShop()
//     {
//         RefreshAll();
//     }

//     /// <summary>
//     /// Refresh tất cả components
//     /// </summary>
//     private void RefreshAll()
//     {
//         if (playerSellScroll != null)
//             playerSellScroll.RefreshDisplay();

//         if (sellShopScroll != null)
//             sellShopScroll.RefreshDisplay();

//         UpdateInfoPanel();
//     }

//     /// <summary>
//     /// Handle khi shop được update
//     /// </summary>
//     private void OnShopUpdated()
//     {
//         // Prevent multiple rapid refreshes
//         if (gameObject.activeInHierarchy)
//         {
//             StartCoroutine(RefreshAllWithDelay());
//         }
//     }

//     /// <summary>
//     /// Refresh all with small delay to avoid race conditions
//     /// </summary>
//     private System.Collections.IEnumerator RefreshAllWithDelay()
//     {
//         yield return new WaitForEndOfFrame();
//         RefreshAll();
//     }

//     /// <summary>
//     /// Handle khi player bán item
//     /// </summary>
//     private void OnPlayerItemSold(CollectableType itemType, int quantity, int earnings)
//     {
//         totalEarnings += earnings;
//         totalItemsSold += quantity;

//         // Add to sell shop history
//         if (sellShopScroll != null)
//         {
//             sellShopScroll.AddSoldItem(itemType, quantity, earnings);
//         }

//         UpdateInfoPanel();

//         Debug.Log($"OnlineSellShop: Player sold {quantity}x {itemType} for {earnings}G");
//     }

//     /// <summary>
//     /// Cập nhật info panel
//     /// </summary>
//     private void UpdateInfoPanel()
//     {
//         if (totalEarningsText != null)
//         {
//             totalEarningsText.text = $"Tổng thu nhập: {totalEarnings}G";
//         }

//         if (itemsSoldText != null)
//         {
//             itemsSoldText.text = $"Đã bán: {totalItemsSold} items";
//         }
//     }

//     /// <summary>
//     /// Reset shop data (có thể dùng để reset hàng ngày)
//     /// </summary>
//     public void ResetShopData()
//     {
//         totalEarnings = 0;
//         totalItemsSold = 0;

//         if (sellShopScroll != null)
//             sellShopScroll.ClearHistory();

//         UpdateInfoPanel();

//         Debug.Log("OnlineSellShop: Shop data has been reset");
//     }

//     /// <summary>
//     /// Getter cho total earnings
//     /// </summary>
//     public int GetTotalEarnings()
//     {
//         return totalEarnings;
//     }

//     /// <summary>
//     /// Getter cho total items sold
//     /// </summary>
//     public int GetTotalItemsSold()
//     {
//         return totalItemsSold;
//     }
// }