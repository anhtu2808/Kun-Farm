using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class ExitUI : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button saveExitButton;

    private bool isPanelActive = false;

    private void Start()
    {
        // Ensure panel is initially hidden
        settingsPanel.SetActive(false);

        // Add listeners to buttons
        settingsButton.onClick.AddListener(ToggleSettingsPanel);
        saveExitButton.onClick.AddListener(SaveAndExit);
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

    private void SaveAndExit()
    {
        // Save game data here
        SaveGameData();

        // Hide panel first
        settingsPanel.SetActive(false);
        isPanelActive = false;

        // Resume normal time scale
        Time.timeScale = 1f;

        // Return to main menu or exit game
        StartCoroutine(ExitGame());
    }

    private void SaveGameData()
    {
        // Implement your save logic here
        Debug.Log("Game data saved!");

        // Example: PlayerPrefs.Save();
    }

    private IEnumerator ExitGame()
    {
        // Add any transition effects here
        yield return new WaitForSeconds(0.5f);

        // Load main menu scene or exit game
        // Example: SceneManager.LoadScene("MainMenu");
        // Or for quitting: Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}