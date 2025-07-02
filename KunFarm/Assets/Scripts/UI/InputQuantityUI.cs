using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputQuantityUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject popupPanel; // Panel chính để bật/tắt
    [SerializeField] private TMP_InputField quantityInputField;
    [SerializeField] private TMP_InputField priceInputField;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private int defaultQuantity = 1;
    [SerializeField] private int maxQuantity = 999;
    
    [Header("Animation")]
    // [SerializeField] private bool useAnimation = false; // Unused field - commented out to avoid warning
    
    private bool isShowing = false;

    // Delegate for callback events
    public delegate void OnQuantityConfirmedDelegate(int quantity, int price);
    public delegate void OnCancelledDelegate();

    // Events that can be subscribed to
    private OnQuantityConfirmedDelegate onQuantityConfirmed;
    private OnCancelledDelegate onCancelled;

    private void Awake()
    {
        // Auto-find popup panel if not assigned
        if (popupPanel == null)
        {
            // Tìm child object có tên chứa "Panel" hoặc lấy child đầu tiên
            Transform panelTransform = transform.Find("Panel");
            if (panelTransform == null && transform.childCount > 0)
                panelTransform = transform.GetChild(0);
            
            if (panelTransform != null)
                popupPanel = panelTransform.gameObject;
            else
                popupPanel = gameObject; // Fallback to self
        }

        // Initialize UI elements if not set
        if (quantityInputField == null)
            quantityInputField = GetComponentInChildren<TMP_InputField>();

        if (confirmButton == null)
            confirmButton = transform.Find("ConfirmButton")?.GetComponent<Button>();

        if (cancelButton == null)
            cancelButton = transform.Find("CancelButton")?.GetComponent<Button>();

        // Add listeners to buttons
        confirmButton?.onClick.AddListener(ConfirmQuantity);
        cancelButton?.onClick.AddListener(CancelInput);
        
        // Đảm bảo popup ẩn ban đầu
        Hide();
    }

    private void Start()
    {
        // Set up input validation once
        if (quantityInputField != null)
            quantityInputField.onValueChanged.AddListener(ValidateInput);
    }
    
    private void Update()
    {
        // Close popup with ESC key
        if (isShowing && Input.GetKeyDown(KeyCode.Escape))
        {
            CancelInput();
        }
    }

    private void ValidateInput(string inputQuantity)
    {
        // Ensure input is valid and within range
        if (string.IsNullOrEmpty(inputQuantity))
        {
            quantityInputField.text = defaultQuantity.ToString();
            return;
        }

        if (int.TryParse(inputQuantity, out int quantity))
        {
            if (quantity <= 0)
                quantityInputField.text = "1";
            else if (quantity > maxQuantity)
                quantityInputField.text = maxQuantity.ToString();
        }
        else
        {
            quantityInputField.text = defaultQuantity.ToString();
        }
    }

    private void ConfirmQuantity()
    {
        if (!isShowing) return; // Tránh double-click
        
        if (int.TryParse(quantityInputField.text, out int quantity) && int.TryParse(priceInputField.text, out int price))
        {
            // Trigger the confirm event
            onQuantityConfirmed?.Invoke(quantity, price);
        }
        else
        {
            onQuantityConfirmed?.Invoke(defaultQuantity, 10);
        }

        // Hide panel after confirming
        Hide();
    }

    private void CancelInput()
    {
        if (!isShowing) return; // Tránh double-click
        
        // Trigger the cancel event
        onCancelled?.Invoke();

        // Hide panel after cancelling
        Hide();
    }

    // Public methods to register callbacks
    public void SetConfirmCallback(OnQuantityConfirmedDelegate callback)
    {
        onQuantityConfirmed = callback;
    }

    public void SetCancelCallback(OnCancelledDelegate callback)
    {
        onCancelled = callback;
    }

    // Hide the popup
    public void Hide()
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
            isShowing = false;
        }
    }
    
    // Show the panel with default settings
    public void Show()
    {
        if (isShowing) return; // Tránh hiện nhiều lần
        
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
            isShowing = true;
            
            // Reset input values
            if (quantityInputField != null)
            {
                quantityInputField.text = defaultQuantity.ToString();
                quantityInputField.Select();
                quantityInputField.ActivateInputField();
            }
        }
    }

    // Show the panel with custom default quantity
    public void Show(int defaultValue, int maxValue = -1)
    {
        if (isShowing) return; // Tránh hiện nhiều lần
        
        defaultQuantity = defaultValue;
        if (maxValue > 0)
            maxQuantity = maxValue;

        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
            isShowing = true;
            
            // Set custom values
            if (quantityInputField != null)
            {
                quantityInputField.text = defaultQuantity.ToString();
                quantityInputField.Select();
                quantityInputField.ActivateInputField();
            }
        }
    }
    
    // Check if popup is currently showing
    public bool IsShowing()
    {
        return isShowing && popupPanel != null && popupPanel.activeSelf;
    }
    
    // Force close popup without triggering callbacks (emergency close)
    public void ForceClose()
    {
        Hide();
        // Clear callbacks
        onQuantityConfirmed = null;
        onCancelled = null;
    }
}
