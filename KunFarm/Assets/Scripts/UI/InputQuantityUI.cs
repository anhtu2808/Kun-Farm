using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputQuantityUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField quantityInputField;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private int defaultQuantity = 1;
    [SerializeField] private int maxQuantity = 999;

    // Delegate for callback events
    public delegate void OnQuantityConfirmedDelegate(int quantity);
    public delegate void OnCancelledDelegate();

    // Events that can be subscribed to
    private OnQuantityConfirmedDelegate onQuantityConfirmed;
    private OnCancelledDelegate onCancelled;

    private void Awake()
    {
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
    }

    private void OnEnable()
    {
        // Initialize input field with default value
        quantityInputField.text = defaultQuantity.ToString();
        quantityInputField.Select();
        quantityInputField.ActivateInputField();

        // Set up input validation
        quantityInputField.onValueChanged.AddListener(ValidateInput);
    }

    private void ValidateInput(string input)
    {
        // Ensure input is valid and within range
        if (string.IsNullOrEmpty(input))
        {
            quantityInputField.text = defaultQuantity.ToString();
            return;
        }

        if (int.TryParse(input, out int quantity))
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
        if (int.TryParse(quantityInputField.text, out int quantity))
        {
            // Trigger the confirm event
            onQuantityConfirmed?.Invoke(quantity);
        }
        else
        {
            onQuantityConfirmed?.Invoke(defaultQuantity);
        }

        // Hide panel after confirming
        gameObject.SetActive(false);
    }

    private void CancelInput()
    {
        // Trigger the cancel event
        onCancelled?.Invoke();

        // Hide panel after cancelling
        gameObject.SetActive(false);
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

    // Show the panel with default settings
    public void Show()
    {
        gameObject.SetActive(true);
    }

    // Show the panel with custom default quantity
    public void Show(int defaultValue, int maxValue = -1)
    {
        defaultQuantity = defaultValue;
        if (maxValue > 0)
            maxQuantity = maxValue;

        gameObject.SetActive(true);
    }
}
