using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SimpleNotificationPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float displayDuration = 5f;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeOutDuration = 0.5f;

    public static SimpleNotificationPopup Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            gameObject.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowNotification(string message)
    {
        // Dừng coroutine trước đó nếu có
        StopAllCoroutines();

        // Thiết lập nội dung
        messageText.text = message;

        // Reset trạng thái alpha
        canvasGroup.alpha = 1f;

        // Hiển thị popup
        gameObject.SetActive(true);

        // Bắt đầu đếm ngược để ẩn popup
        StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        // Đợi theo thời gian cấu hình
        yield return new WaitForSeconds(displayDuration - fadeOutDuration);

        // Thực hiện hiệu ứng fade out
        float elapsedTime = 0;
        while (elapsedTime < fadeOutDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Đảm bảo alpha = 0 và ẩn GameObject
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    // Phương thức tĩnh để dễ sử dụng từ bất kỳ đâu
    public static void Show(string message)
    {
        if (Instance != null)
        {
            Instance.ShowNotification(message);
        }
    }
}