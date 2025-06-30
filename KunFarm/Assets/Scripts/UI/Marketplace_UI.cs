using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI chính cho marketplace online
/// </summary>
public class Marketplace_UI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject marketplacePanel;
    public Transform itemsContainer;
    public Button closeButton;
    public Button refreshButton;
    public Button listItemButton;
    
    [Header("Marketplace Slot Prefab")]
    public GameObject marketplaceSlotPrefab;
    
    [Header("Marketplace System")]
    public MarketplaceManager marketplaceManager;
    
    [Header("Tabs")]
    public Button browseTabButton;
    public Button myListingsTabButton;
    public Button transactionsTabButton;
    public GameObject browsePanel;
    public GameObject myListingsPanel;
    public GameObject transactionsPanel;
    
    [Header("Search")]
    public TMP_InputField searchInput;
    public Button searchButton;
    
    // Marketplace slots
    private List<MarketplaceSlot_UI> marketplaceSlots = new List<MarketplaceSlot_UI>();
    
    // Current view
    private MarketplaceView currentView = MarketplaceView.Browse;
    
    // Property để kiểm tra Marketplace có đang mở không
    public bool IsOpen => marketplacePanel.activeSelf;
    
    public enum MarketplaceView
    {
        Browse,
        MyListings,
        Transactions
    }

    void Start()
    {
        // Đảm bảo marketplace luôn tắt khi bắt đầu
        marketplacePanel.SetActive(false);
        
        // Setup buttons
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseMarketplace);
            
        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshMarketplace);
            
        if (listItemButton != null)
            listItemButton.onClick.AddListener(OpenListItemDialog);
        
        // Setup tab buttons
        if (browseTabButton != null)
            browseTabButton.onClick.AddListener(() => SwitchView(MarketplaceView.Browse));
            
        if (myListingsTabButton != null)
            myListingsTabButton.onClick.AddListener(() => SwitchView(MarketplaceView.MyListings));
            
        if (transactionsTabButton != null)
            transactionsTabButton.onClick.AddListener(() => SwitchView(MarketplaceView.Transactions));
        
        // Setup search
        if (searchButton != null)
            searchButton.onClick.AddListener(PerformSearch);
        
        // Tự động tìm MarketplaceManager nếu chưa assign
        if (marketplaceManager == null)
            marketplaceManager = FindObjectOfType<MarketplaceManager>();
            
        // Subscribe to marketplace events
        if (marketplaceManager != null)
        {
            marketplaceManager.OnItemsLoaded += OnItemsLoaded;
            marketplaceManager.OnError += OnMarketplaceError;
        }
            
        // Initialize marketplace slots
        InitializeMarketplaceSlots();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMarketplace();
        }
        
        // Escape key để đóng marketplace
        if (Input.GetKeyDown(KeyCode.Escape) && IsOpen)
        {
            CloseMarketplace();
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (marketplaceManager != null)
        {
            marketplaceManager.OnItemsLoaded -= OnItemsLoaded;
            marketplaceManager.OnError -= OnMarketplaceError;
        }
    }

    /// <summary>
    /// Initialize marketplace slots
    /// </summary>
    private void InitializeMarketplaceSlots()
    {
        if (marketplaceManager == null || itemsContainer == null)
        {
            Debug.LogWarning("Marketplace_UI: Missing references for marketplace initialization!");
            return;
        }

        ClearMarketplaceSlots();
        StartCoroutine(CreateSlotsAfterClear());
    }
    
    /// <summary>
    /// Tạo slots sau khi clear hoàn thành
    /// </summary>
    private System.Collections.IEnumerator CreateSlotsAfterClear()
    {
        yield return null;
        
        // Load items từ server
        _ = marketplaceManager.LoadActiveItems();
        
        Debug.Log("Marketplace_UI: Initialized marketplace slots");
    }

    /// <summary>
    /// Tạo một marketplace slot mới
    /// </summary>
    private void CreateMarketplaceSlot(MarketplaceItem item, int index)
    {
        if (marketplaceSlotPrefab == null || itemsContainer == null)
        {
            Debug.LogError("Marketplace_UI: Missing marketplaceSlotPrefab or itemsContainer!");
            return;
        }

        GameObject slotObj = Instantiate(marketplaceSlotPrefab, itemsContainer);
        slotObj.transform.SetSiblingIndex(index);
        
        MarketplaceSlot_UI slotUI = slotObj.GetComponent<MarketplaceSlot_UI>();
        
        if (slotUI != null)
        {
            slotUI.Initialize(item, marketplaceManager);
            slotUI.OnBuyClicked += OnSlotBuyClicked;
            
            marketplaceSlots.Add(slotUI);
        }
        else
        {
            Debug.LogError("Marketplace_UI: marketplaceSlotPrefab doesn't have MarketplaceSlot_UI component!");
            Destroy(slotObj);
        }
    }

    /// <summary>
    /// Clear tất cả marketplace slots
    /// </summary>
    private void ClearMarketplaceSlots()
    {
        foreach (var slot in marketplaceSlots)
        {
            if (slot != null)
            {
                slot.OnBuyClicked -= OnSlotBuyClicked;
                Destroy(slot.gameObject);
            }
        }
        marketplaceSlots.Clear();
        
        if (itemsContainer != null)
        {
            Transform[] children = new Transform[itemsContainer.childCount];
            for (int i = 0; i < itemsContainer.childCount; i++)
            {
                children[i] = itemsContainer.GetChild(i);
            }
            
            foreach (Transform child in children)
            {
                if (child.name.Contains("Marketplace_Slot"))
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// Handle buy button click
    /// </summary>
    private void OnSlotBuyClicked(MarketplaceSlot_UI slot)
    {
        if (slot != null)
        {
            slot.BuyItem(1);
            Debug.Log($"Marketplace_UI: Player mua {slot.GetMarketplaceItem()?.itemName}");
        }
    }

    /// <summary>
    /// Switch giữa các view
    /// </summary>
    private void SwitchView(MarketplaceView view)
    {
        currentView = view;
        
        // Update tab buttons
        browseTabButton.interactable = view != MarketplaceView.Browse;
        myListingsTabButton.interactable = view != MarketplaceView.MyListings;
        transactionsTabButton.interactable = view != MarketplaceView.Transactions;
        
        // Show/hide panels
        browsePanel.SetActive(view == MarketplaceView.Browse);
        myListingsPanel.SetActive(view == MarketplaceView.MyListings);
        transactionsPanel.SetActive(view == MarketplaceView.Transactions);
        
        // Load data based on view
        switch (view)
        {
            case MarketplaceView.Browse:
                _ = marketplaceManager.LoadActiveItems();
                break;
            case MarketplaceView.MyListings:
                // TODO: Load user's listings
                break;
            case MarketplaceView.Transactions:
                _ = marketplaceManager.LoadUserTransactions();
                break;
        }
    }

    /// <summary>
    /// Perform search
    /// </summary>
    private async void PerformSearch()
    {
        if (searchInput == null || string.IsNullOrEmpty(searchInput.text))
        {
            _ = marketplaceManager.LoadActiveItems();
            return;
        }
        
        var searchResults = await marketplaceManager.SearchItems(searchInput.text);
        DisplayItems(searchResults);
    }

    /// <summary>
    /// Display items in slots
    /// </summary>
    private void DisplayItems(List<MarketplaceItem> items)
    {
        ClearMarketplaceSlots();
        
        for (int i = 0; i < items.Count; i++)
        {
            CreateMarketplaceSlot(items[i], i);
        }
    }

    /// <summary>
    /// Handle items loaded event
    /// </summary>
    private void OnItemsLoaded(List<MarketplaceItem> items)
    {
        DisplayItems(items);
    }

    /// <summary>
    /// Handle marketplace error
    /// </summary>
    private void OnMarketplaceError(string error)
    {
        Debug.LogError($"Marketplace Error: {error}");
        // TODO: Show error UI
    }

    /// <summary>
    /// Open list item dialog
    /// </summary>
    private void OpenListItemDialog()
    {
        // TODO: Open dialog to list new item
        Debug.Log("Marketplace_UI: Open list item dialog");
    }

    public void ToggleMarketplace()
    {
        if (!marketplacePanel.activeSelf)
        {
            OpenMarketplace();
        }
        else
        {
            CloseMarketplace();
        }
    }

    public void OpenMarketplace()
    {
        marketplacePanel.SetActive(true);
        SwitchView(MarketplaceView.Browse);
    }

    public void CloseMarketplace()
    {
        marketplacePanel.SetActive(false);
    }

    /// <summary>
    /// Refresh marketplace
    /// </summary>
    public void RefreshMarketplace()
    {
        if (marketplaceManager != null)
        {
            switch (currentView)
            {
                case MarketplaceView.Browse:
                    _ = marketplaceManager.LoadActiveItems();
                    break;
                case MarketplaceView.MyListings:
                    // TODO: Refresh user listings
                    break;
                case MarketplaceView.Transactions:
                    _ = marketplaceManager.LoadUserTransactions();
                    break;
            }
        }
    }

    /// <summary>
    /// Force refresh marketplace UI
    /// </summary>
    public void ForceRefresh()
    {
        InitializeMarketplaceSlots();
    }
} 