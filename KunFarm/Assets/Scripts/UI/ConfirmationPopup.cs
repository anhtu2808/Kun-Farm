using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class ConfirmationPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private GameObject popupPanel;

    private Action onConfirm;
    private Action onCancel;

    private static ConfirmationPopup instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        confirmButton.onClick.AddListener(OnConfirmClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);

        // Ẩn popup khi bắt đầu
        popupPanel.SetActive(false);
    }

    private void OnConfirmClicked()
    {
        popupPanel.SetActive(false);
        onConfirm?.Invoke();
    }

    private void OnCancelClicked()
    {
        popupPanel.SetActive(false);
        onCancel?.Invoke();
    }

    // Phương thức để hiển thị popup với thông báo và các callback
    public static void Show(string message, Action confirmAction, Action cancelAction = null)
    {
        if (instance == null)
        {
            Debug.LogError("Confirmation Popup instance không tồn tại!");
            return;
        }

        instance.messageText.text = message;
        instance.onConfirm = confirmAction;
        instance.onCancel = cancelAction;
        instance.popupPanel.SetActive(true);
    }
}