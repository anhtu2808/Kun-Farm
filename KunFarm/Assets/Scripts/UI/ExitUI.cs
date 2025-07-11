using UnityEngine;
using UnityEngine.UI;

public class ExitUI : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button settingsButton;

    private bool isPanelActive = false;

    private void Start()
    {
        // Ensure panel is initially hidden
        settingsPanel.SetActive(false);

        // Add listeners to buttons
        settingsButton.onClick.AddListener(ToggleSettingsPanel);
    }

    private void Update()
    {
        // ESC key handling disabled to avoid conflict with SaveAndExitManager
        // ESC is now handled by SaveAndExitManager which can call settings via button
    }

    public void ToggleSettingsPanel()
    {
        isPanelActive = !isPanelActive;
        settingsPanel.SetActive(isPanelActive);

        // Optional: Pause the game when panel is active
        Time.timeScale = isPanelActive ? 0f : 1f;
    }


}