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

    // Data
    private CollectableType currentItemType = CollectableType.NONE;
    private int currentQuantity = 0;
    private int currentPrice = 0;
    private bool isSellable = false;

    // Events
    public System.Action<PlayerSellItemSlot_UI> OnSellClicked;

    private void Awake()
    {
        SetupButton();
        SetEmpty();
    }

    /// <summary>
    /// Setup sell button
    /// </summary>
    private void SetupButton()
    {
        if (sellButton != null)
        {
            sellButton.onClick.AddListener(() =>
            {
                // Only trigger if slot has valid item
                if (currentItemType != CollectableType.NONE && currentQuantity > 0)
                {
                    Debug.Log($"[PlayerSellItemSlot] Bán {currentQuantity} x {currentItemType} với giá {currentPrice}G");
                    OnSellClicked?.Invoke(this);
                }
                else
                {
                    Debug.LogWarning("[PlayerSellItemSlot] Attempted to sell empty slot");
                }
            });
        }
    }
    /// <summary>
    /// Set item data cho slot
    /// </summary>
    public void SetItem(CollectableType itemType, Sprite icon, int quantity, int price, bool sellable)
    {
        currentItemType = itemType;
        currentQuantity = quantity;
        currentPrice = price;
        isSellable = sellable;

        // Update icon
        if (itemIcon != null && icon != null)
        {
            itemIcon.sprite = icon;
            itemIcon.color = normalColor;
            itemIcon.gameObject.SetActive(true);
        }


        // Update quantity
        if (quantityText != null)
        {
            quantityText.text = $"x{quantity}";
            quantityText.gameObject.SetActive(true);
        }

        // Update price
        if (priceText != null)
        {
            if (sellable)
            {
                priceText.text = $"{price}G";
                priceText.color = Color.yellow;
            }
            else
            {
                priceText.text = "Không bán được";
                priceText.color = unsellableColor;
            }
            priceText.gameObject.SetActive(true);
        }

        // Update sell button
        if (sellButton != null)
        {
            sellButton.interactable = sellable;
            sellButton.gameObject.SetActive(true);
        }

        // Update visual state
        UpdateVisualState();

        // Show slot
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Set slot as empty
    /// </summary>
    public void SetEmpty()
    {
        currentItemType = CollectableType.NONE;
        currentQuantity = 0;
        currentPrice = 0;
        isSellable = false;

        // Hide all UI elements
        if (itemIcon != null)
            itemIcon.gameObject.SetActive(false);
        if (quantityText != null)
            quantityText.gameObject.SetActive(false);
        if (priceText != null)
            priceText.gameObject.SetActive(false);
        if (sellButton != null)
            sellButton.gameObject.SetActive(false);

        // Hide slot
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Update visual state based on sellability
    /// </summary>
    private void UpdateVisualState()
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(isSellable);
        }

        // Update background color if available
        Image background = GetComponent<Image>();
        if (background != null)
        {
            if (currentItemType == CollectableType.NONE)
            {
                background.color = normalColor;
            }
            else if (isSellable)
            {
                background.color = sellableColor;
            }
            else
            {
                background.color = unsellableColor;
            }
        }
    }

    /// <summary>
    /// Get current item type
    /// </summary>
    public CollectableType GetItemType()
    {
        return currentItemType;
    }

    /// <summary>
    /// Get current quantity
    /// </summary>
    public int GetQuantity()
    {
        return currentQuantity;
    }

    /// <summary>
    /// Get current price
    /// </summary>
    public int GetPrice()
    {
        return currentPrice;
    }

    /// <summary>
    /// Check if item is sellable
    /// </summary>
    public bool IsSellable()
    {
        return isSellable;
    }

    /// <summary>
    /// Check if slot is empty
    /// </summary>
    public bool IsEmpty()
    {
        return currentItemType == CollectableType.NONE;
    }

    /// <summary>
    /// Set highlight state
    /// </summary>
    public void SetHighlight(bool highlighted)
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(highlighted && isSellable);
        }
    }

    /// <summary>
    /// Animate sell action
    /// </summary>
    public void AnimateSell()
    {
        // Simple animation - scale down and fade out
        StartCoroutine(AnimateSellCoroutine());
    }

    private System.Collections.IEnumerator AnimateSellCoroutine()
    {
        Vector3 originalScale = transform.localScale;
        Color originalColor = GetComponent<Image>()?.color ?? Color.white;

        // Scale down
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            transform.localScale = Vector3.Lerp(originalScale, originalScale * 0.8f, progress);

            // Fade out
            Image bg = GetComponent<Image>();
            if (bg != null)
            {
                Color newColor = bg.color;
                newColor.a = Mathf.Lerp(1f, 0.5f, progress);
                bg.color = newColor;
            }

            yield return null;
        }

        // Reset
        transform.localScale = originalScale;
        Image background = GetComponent<Image>();
        if (background != null)
        {
            background.color = originalColor;
        }

        // Set empty
        SetEmpty();
    }
}