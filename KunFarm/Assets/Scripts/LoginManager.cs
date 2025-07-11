using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoginManager : MonoBehaviour
{
    /* ---------- UI ---------- */
    [Header("UI References")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button         loginButton;
    public Button         registerButton;
    // public TMP_Text       errorText; // Không sử dụng nữa, thay bằng notification

    /* ---------- Settings ---------- */
    [Header("Settings")]
    // API URL will be loaded from ApiClient.BaseUrl
    [SerializeField] private bool showDebug = true;

    /* ---------- DTO ---------- */
    [Serializable]
    private class LoginRequest
    {
        public string usernameOrEmail;
        public string password;
    }

    [Serializable]
    private class UserDto
    {
        public int    id;
        public string username;
        public string email;
        public string displayName;
        public string lastLoginAt;
        public string role;
    }

    [Serializable]
    private class AuthData
    {
        public string  token;
        public UserDto user;
    }

    [Serializable]
    private class ApiResponse   // lớp bọc ngoài
    {
        public int       code;
        public string    message;
        public AuthData  data;
    }

    /* ---------- Unity ---------- */
    void Start()
    {
        // Setup login button
        if (loginButton != null) 
        loginButton.onClick.AddListener(OnLogin);
        
        // Setup register button
        if (registerButton != null)
            registerButton.onClick.AddListener(GoToRegister);
            
        // Ensure ApiClient exists
        if (ApiClient.Instance == null)
        {
            GameObject apiClientGO = new GameObject("ApiClient");
            apiClientGO.AddComponent<ApiClient>();
            DontDestroyOnLoad(apiClientGO);
        }
    }

    void OnLogin()
    {
        // Validate inputs
        if (usernameInput == null || passwordInput == null)
        {
            SimpleNotificationPopup.Show("UI components not properly assigned!");
            return;
        }
        
        if (string.IsNullOrEmpty(usernameInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            SimpleNotificationPopup.Show("Please enter both username and password!");
            return;
        }
        
        StartCoroutine(LoginRoutine());
    }

    IEnumerator LoginRoutine()
    {
        // Disable login button during request
        if (loginButton != null) loginButton.interactable = false;

        try
    {
        /* 1) Build payload */
            var rq = new LoginRequest { 
                usernameOrEmail = usernameInput.text.Trim(), 
                password = passwordInput.text 
            };
        string jsonPayload = JsonUtility.ToJson(rq);

        string apiUrl = $"{ApiClient.BaseUrl}/auth/login";
        if (showDebug) Debug.Log($"[Login] POST {apiUrl}\nPayload: {jsonPayload}");

        /* 2) Send request */
        using var req = new UnityWebRequest(apiUrl, "POST");
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonPayload));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        /* 3) Network error? */
        if (req.result != UnityWebRequest.Result.Success)
        {
                SimpleNotificationPopup.Show("Username or password is incorrect!");
            yield break;
        }

        /* 4) Parse JSON */
        string rawJson = req.downloadHandler.text;
        if (showDebug) Debug.Log($"[Login] HTTP {req.responseCode}\nResponse: {rawJson}");

            if (string.IsNullOrEmpty(rawJson))
            {
                SimpleNotificationPopup.Show("Empty response from server");
                yield break;
            }

            ApiResponse resp = null;
            try
            {
                resp = JsonUtility.FromJson<ApiResponse>(rawJson);
            }
            catch (Exception ex)
            {
                if (showDebug) Debug.LogError($"JSON Parse Error: {ex.Message}");
                SimpleNotificationPopup.Show("Failed to parse server response");
                yield break;
            }

            /* 5) Kiểm tra response */
            if (resp == null)
            {
                SimpleNotificationPopup.Show("Failed to parse server response");
                yield break;
            }

            // Check for success
            if (req.responseCode == 200 && resp.code == 200)
            {
                if (resp.data == null)
                {
                    SimpleNotificationPopup.Show("Invalid response: missing data");
                    yield break;
                }

                if (resp.data.user == null)
        {
                    SimpleNotificationPopup.Show("Invalid response: missing user data");
                    yield break;
                }

                if (string.IsNullOrEmpty(resp.data.token))
                {
                    SimpleNotificationPopup.Show("Invalid response: missing token");
                    yield break;
                }

                // Save login data
            PlayerPrefs.SetString("JWT_TOKEN", resp.data.token);
                PlayerPrefs.SetInt("PLAYER_ID", resp.data.user.id);
                PlayerPrefs.SetString("PLAYER_NAME", resp.data.user.username ?? "");
                PlayerPrefs.SetString("PLAYER_DISPLAYNAME", resp.data.user.displayName ?? "");
            PlayerPrefs.Save();

                // Set token in ApiClient
                if (ApiClient.Instance != null)
                {
            ApiClient.Instance.SetToken(resp.data.token);
                }

            if (showDebug) Debug.Log($"✅ Login successful! Welcome {resp.data.user.displayName}");

                // Show success notification
                SimpleNotificationPopup.Show($"Welcome {resp.data.user.displayName}! Login successful!");

                // Wait a moment before loading scene to let user see the success message
                yield return new WaitForSeconds(1f);

                // Load main scene
            SceneManager.LoadScene("MainScene");
        }
        else
        {
                // Login failed
                string errorMsg = resp.message ?? "Login failed";
                if (showDebug) Debug.LogWarning($"[Login] Failed: {errorMsg}");
                SimpleNotificationPopup.Show(errorMsg);
            }
        }
        finally
        {
            // Re-enable login button
            if (loginButton != null) loginButton.interactable = true;
        }
    }

    void GoToRegister()
    {
        if (showDebug) Debug.Log("[Login] Navigating to RegisterScene");
        SceneManager.LoadScene("RegisterScene");
    }
}
