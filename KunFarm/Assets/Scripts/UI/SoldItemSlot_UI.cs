using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Individual sold item slot UI cho SellShop_Scroll
/// </summary>
public class SoldItemSlot_UI : MonoBehaviour
{
    [Header("UI References")]
    public Image itemIcon;
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI earningsText;

    [Header("Visual States")]
    public GameObject highlightObject;
    public Color normalColor = Color.white;
    public Color recentColor = Color.green; // For recently sold items
    public Color oldColor = Color.gray; // For older items

    // Data
    private CollectableType currentItemType = CollectableType.NONE;
    private string currentItemName = "";
    private int currentQuantity = 0;
    private int currentEarnings = 0;
    private System.DateTime sellTime;

    private void Awake()
    {
        SetEmpty();
    }

    /// <summary>
    /// Set sold item data cho slot
    /// </summary>
    public void SetSoldItem(CollectableType itemType, Sprite icon, int quantity, int earnings, System.DateTime time)
    {
        currentItemType = itemType;
        currentQuantity = quantity;
        currentEarnings = earnings;
        sellTime = time;

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

        // Update earnings
        if (earningsText != null)
        {
            earningsText.text = $"+{earnings}G";
            earningsText.color = Color.green;
            earningsText.gameObject.SetActive(true);
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
        currentItemName = "";
        currentQuantity = 0;
        currentEarnings = 0;
        sellTime = System.DateTime.MinValue;

        // Hide all UI elements
        if (itemIcon != null)
            itemIcon.gameObject.SetActive(false);
        if (quantityText != null)
            quantityText.gameObject.SetActive(false);
        if (earningsText != null)
            earningsText.gameObject.SetActive(false);

        // Hide slot
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Format time since item was sold
    /// </summary>
    private string FormatTimeSinceSold(System.DateTime sellTime)
    {
        System.TimeSpan timeSpan = System.DateTime.Now - sellTime;

        if (timeSpan.TotalMinutes < 1)
        {
            return "Vừa xong";
        }
        else if (timeSpan.TotalMinutes < 60)
        {
            return $"{(int)timeSpan.TotalMinutes} phút trước";
        }
        else if (timeSpan.TotalHours < 24)
        {
            return $"{(int)timeSpan.TotalHours} giờ trước";
        }
        else
        {
            return $"{(int)timeSpan.TotalDays} ngày trước";
        }
    }

    /// <summary>
    /// Update visual state based on time
    /// </summary>
    private void UpdateVisualState()
    {
        if (highlightObject != null)
        {
            // Highlight recent items (sold within last 5 minutes)
            System.TimeSpan timeSinceSold = System.DateTime.Now - sellTime;
            bool isRecent = timeSinceSold.TotalMinutes < 5;
            highlightObject.SetActive(isRecent);
        }

        // Update background color based on age
        Image background = GetComponent<Image>();
        if (background != null)
        {
            System.TimeSpan timeSinceSold = System.DateTime.Now - sellTime;

            if (currentItemType == CollectableType.NONE)
            {
                background.color = normalColor;
            }
            else if (timeSinceSold.TotalMinutes < 5)
            {
                background.color = recentColor;
            }
            else if (timeSinceSold.TotalHours < 1)
            {
                background.color = Color.Lerp(recentColor, normalColor, (float)(timeSinceSold.TotalMinutes / 60.0));
            }
            else
            {
                background.color = oldColor;
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
    /// Get current item name
    /// </summary>
    public string GetItemName()
    {
        return currentItemName;
    }

    /// <summary>
    /// Get current quantity
    /// </summary>
    public int GetQuantity()
    {
        return currentQuantity;
    }

    /// <summary>
    /// Get current earnings
    /// </summary>
    public int GetEarnings()
    {
        return currentEarnings;
    }

    /// <summary>
    /// Get sell time
    /// </summary>
    public System.DateTime GetSellTime()
    {
        return sellTime;
    }

    /// <summary>
    /// Check if slot is empty
    /// </summary>
    public bool IsEmpty()
    {
        return currentItemType == CollectableType.NONE;
    }

    /// <summary>
    /// Check if item was sold recently (within 5 minutes)
    /// </summary>
    public bool IsRecentlySold()
    {
        if (IsEmpty()) return false;

        System.TimeSpan timeSinceSold = System.DateTime.Now - sellTime;
        return timeSinceSold.TotalMinutes < 5;
    }

    /// <summary>
    /// Set highlight state
    /// </summary>
    public void SetHighlight(bool highlighted)
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(highlighted);
        }
    }

    /// <summary>
    /// Animate new item appearance
    /// </summary>
    public void AnimateNewItem()
    {
        StartCoroutine(AnimateNewItemCoroutine());
    }

    private System.Collections.IEnumerator AnimateNewItemCoroutine()
    {
        // Start with scale 0 and fade in
        Vector3 originalScale = transform.localScale;
        Color originalColor = GetComponent<Image>()?.color ?? Color.white;

        transform.localScale = Vector3.zero;

        Image bg = GetComponent<Image>();
        if (bg != null)
        {
            Color newColor = bg.color;
            newColor.a = 0f;
            bg.color = newColor;
        }

        // Animate in
        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;

            // Ease out bounce effect
            float scaleProgress = 1f - Mathf.Pow(1f - progress, 3f);
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, scaleProgress);

            // Fade in
            if (bg != null)
            {
                Color newColor = bg.color;
                newColor.a = Mathf.Lerp(0f, originalColor.a, progress);
                bg.color = newColor;
            }

            yield return null;
        }

        // Ensure final state
        transform.localScale = originalScale;
        if (bg != null)
        {
            bg.color = originalColor;
        }
    }

    /// <summary>
    /// Get formatted time string
    /// </summary>
    public string GetFormattedTime()
    {
        return FormatTimeSinceSold(sellTime);
    }

    /// <summary>
    /// Get time since sold as TimeSpan
    /// </summary>
    public System.TimeSpan GetTimeSinceSold()
    {
        return System.DateTime.Now - sellTime;
    }
}