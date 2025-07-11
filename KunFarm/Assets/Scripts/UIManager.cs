using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public Inventory inventory;
    public OnlSellShopManager sellShopManager;
    public OnlBuyShopManager buyShopManager;
    public ShopManager regularShopManager;
    
    [Header("Debug")]
    public bool showDebug = true;
    
    public static UIManager Instance { get; private set; }
    
    public enum UIType
    {
        None,
        Inventory,
        SellShop,
        BuyShop,
        RegularShop
    }
    
    private UIType currentlyOpenUI = UIType.None;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Auto-find components if not assigned
        if (inventory == null)
            inventory = FindObjectOfType<Inventory>();
        if (sellShopManager == null)
            sellShopManager = FindObjectOfType<OnlSellShopManager>();
        if (buyShopManager == null)
            buyShopManager = FindObjectOfType<OnlBuyShopManager>();
        if (regularShopManager == null)
            regularShopManager = FindObjectOfType<ShopManager>();
    }
    
    void Update()
    {
        HandleInput();
    }
    
    private void HandleInput()
    {
        // Tab - Inventory
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleUI(UIType.Inventory);
        }
        // O - Online Sell Shop
        else if (Input.GetKeyDown(KeyCode.O))
        {
            ToggleUI(UIType.SellShop);
        }
        // P - Online Buy Shop
        else if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleUI(UIType.BuyShop);
        }
        // B - Regular Shop
        else if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleUI(UIType.RegularShop);
        }
        // ESC - Close all UIs
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseAllUIs();
        }
    }
    
    public void ToggleUI(UIType uiType)
    {
        if (currentlyOpenUI == uiType)
        {
            // Same UI is open, close it
            CloseAllUIs();
        }
        else
        {
            // Different UI, close current and open new
            CloseAllUIs();
            OpenUI(uiType);
        }
    }
    
    public void OpenUI(UIType uiType)
    {
        // Close current UI first
        if (currentlyOpenUI != UIType.None)
        {
            CloseAllUIs();
        }
        
        // Open the requested UI
        switch (uiType)
        {
            case UIType.Inventory:
                if (inventory != null)
                {
                    inventory.inventoryPanel.SetActive(true);
                    inventory.Refresh();
                    currentlyOpenUI = UIType.Inventory;
                    if (showDebug) Debug.Log("[UIManager] Opened Inventory");
                }
                break;
                
            case UIType.SellShop:
                if (sellShopManager != null)
                {
                    sellShopManager.OpenShop();
                    currentlyOpenUI = UIType.SellShop;
                    if (showDebug) Debug.Log("[UIManager] Opened Sell Shop");
                }
                break;
                
            case UIType.BuyShop:
                if (buyShopManager != null)
                {
                    buyShopManager.OpenShop();
                    currentlyOpenUI = UIType.BuyShop;
                    if (showDebug) Debug.Log("[UIManager] Opened Buy Shop");
                }
                break;
                
            case UIType.RegularShop:
                if (regularShopManager != null)
                {
                    regularShopManager.OpenShop();
                    currentlyOpenUI = UIType.RegularShop;
                    if (showDebug) Debug.Log("[UIManager] Opened Regular Shop");
                }
                break;
        }
    }
    
    public void CloseAllUIs()
    {
        // Close Inventory
        if (inventory != null && inventory.inventoryPanel.activeSelf)
        {
            inventory.inventoryPanel.SetActive(false);
        }
        
        // Close Sell Shop
        if (sellShopManager != null)
        {
            sellShopManager.CloseShop();
        }
        
        // Close Buy Shop
        if (buyShopManager != null)
        {
            buyShopManager.CloseShop();
        }
        
        // Close Regular Shop
        if (regularShopManager != null)
        {
            regularShopManager.CloseShop();
        }
        
        currentlyOpenUI = UIType.None;
        if (showDebug) Debug.Log("[UIManager] Closed all UIs");
    }
    
    // Public methods for external scripts
    public void OpenInventory() => OpenUI(UIType.Inventory);
    public void OpenSellShop() => OpenUI(UIType.SellShop);
    public void OpenBuyShop() => OpenUI(UIType.BuyShop);
    public void OpenRegularShop() => OpenUI(UIType.RegularShop);
    
    public bool IsAnyUIOpen() => currentlyOpenUI != UIType.None;
    public bool IsInventoryOpen() => currentlyOpenUI == UIType.Inventory;
    public bool IsSellShopOpen() => currentlyOpenUI == UIType.SellShop;
    public bool IsBuyShopOpen() => currentlyOpenUI == UIType.BuyShop;
    public bool IsRegularShopOpen() => currentlyOpenUI == UIType.RegularShop;
} 