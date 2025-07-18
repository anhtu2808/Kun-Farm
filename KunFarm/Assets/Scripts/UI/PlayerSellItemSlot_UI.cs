using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Individual item slot UI cho PlayerSell_Scroll
/// </summary>
public class PlayerSellItemSlot_UI : MonoBehaviour
{
    [Header("UI References")]
    public Image itemIcon;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI priceText;
    public Button sellButton;

    [Header("Visual States")]
    public GameObject highlightObject;
    public Color normalColor = Color.white;
    public Color sellableColor = Color.green;
    public Color unsellableColor = Color.red;

    public InputQuantityUI inputQuantityUI;
    public Transform sellSlotContainer;
    public GameObject shopSellSlotPrefab;

    // Data
    private CollectableType currentItemType = CollectableType.NONE;
    private int currentQuantity = 0;
    private int currentPrice = 0;
    private bool isSellable = false;
    private Player player;
    private OnlSellShopManager shopManager;
    // Events
    public System.Action<PlayerSellItemSlot_UI> OnSellClicked;

    private void Awake()
    {
        player = FindObjectOfType<Player>();
        shopManager = FindObjectOfType<OnlSellShopManager>();

        if (inputQuantityUI == null)
            inputQuantityUI = FindObjectOfType<InputQuantityUI>();
        SetupButton();
    }

    public void Setup(CollectableType type, int quantity, Sprite icon, OnlSellShopManager manager)
    {
        currentItemType = type;
        currentQuantity = quantity;
        itemIcon.sprite = icon;
        itemIcon.gameObject.SetActive(true);
        quantityText.text = $"x{quantity}";

        isSellable = true;
        shopManager = manager;
        gameObject.SetActive(true);

        OnSellClicked = (slotUI) =>
        {
            // OnlSellShopManager sẽ tự trừ inventory và trigger refresh
            shopManager.SellItemOnline(currentItemType, quantity, currentPrice);
            
            // Không cần update UI manually vì inventory sẽ tự refresh
            // currentQuantity--;
            // quantityText.text = $"x{currentQuantity}";
            // if (currentQuantity <= 0) gameObject.SetActive(false);
        };
    }

    /// <summary>
    /// Setup sell button
    /// </summary>
    private void SetupButton()
    {
        Debug.Log("[PlayerSellItemSlot] Setting up sell button...");
        if (sellButton != null)
        {
            sellButton.onClick.AddListener(() =>
            {
                if (currentItemType != CollectableType.NONE && currentQuantity > 0)
                {
                    // Kiểm tra xem popup đã mở chưa
                    if (inputQuantityUI.IsShowing())
                    {
                        Debug.Log("[PlayerSellItemSlot] Popup already showing, ignoring click");
                        return;
                    }
                    
                    Debug.Log($"[SellItemSlot] Open input for {currentItemType}");
                    
                    // Gọi UI để nhập số lượng và giá
                    inputQuantityUI.SetConfirmCallback((quantity, price) =>
                    {
                        // Callback sau khi người dùng confirm
                        Debug.Log($"[PlayerSellItemSlot] Confirmed sell {quantity} x {currentItemType} at {price}G");

                        // OnlSellShopManager sẽ tự trừ inventory và trigger refresh
                        shopManager.SellItemOnline(currentItemType, quantity, price);
                        
                        // Không cần update UI manually vì inventory sẽ tự refresh
                        // currentQuantity -= quantity;
                        // quantityText.text = $"x{currentQuantity}";
                        // if (currentQuantity <= 0) gameObject.SetActive(false);
                    });

                    inputQuantityUI.SetCancelCallback(() =>
                    {
                        Debug.Log("[PlayerSellItemSlot] User canceled input.");
                    });

                    // Hiện UI nhập với giá trị mặc định
                    inputQuantityUI.Show(1, currentQuantity); // default=1, max=currentQuantity
                }
                else
                {
                    Debug.LogWarning("[PlayerSellItemSlot] Attempted to sell empty slot.");
                }
            });
        }
    }

    /// <summary>
    /// Gán dữ liệu cho slot UI
    /// </summary>
    public void SetSlot(CollectableType type, Sprite icon, int quantity, int price, bool canSell)
    {
        currentItemType = type;
        currentQuantity = quantity;
        currentPrice = price;
        isSellable = canSell;

        itemIcon.sprite = icon;
        quantityText.text = $"x{quantity}";
        priceText.text = $"{price}G";

        highlightObject.SetActive(canSell);
        itemIcon.color = canSell ? sellableColor : unsellableColor;
    }

}