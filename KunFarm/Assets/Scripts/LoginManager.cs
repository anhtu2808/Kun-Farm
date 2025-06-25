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
    public TMP_Text       errorText;

    /* ---------- Settings ---------- */
    [Header("Settings")]
    public string loginUrl = "http://localhost:5270/auth/login";
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
        // Clear error text
        if (errorText != null) errorText.text = "";
        
        // Setup login button
        if (loginButton != null) 
            loginButton.onClick.AddListener(OnLogin);
            
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
        if (errorText != null) errorText.text = "";
        
        // Validate inputs
        if (usernameInput == null || passwordInput == null)
        {
            if (errorText != null) errorText.text = "UI components not properly assigned!";
            return;
        }
        
        if (string.IsNullOrEmpty(usernameInput.text) || string.IsNullOrEmpty(passwordInput.text))
        {
            if (errorText != null) errorText.text = "Please enter both username and password!";
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

            if (showDebug) Debug.Log($"[Login] POST {loginUrl}\nPayload: {jsonPayload}");

            /* 2) Send request */
            using var req = new UnityWebRequest(loginUrl, "POST");
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonPayload));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            /* 3) Network error? */
            if (req.result != UnityWebRequest.Result.Success)
            {
                if (errorText != null) errorText.text = $"Network error: {req.error}";
                yield break;
            }

            /* 4) Parse JSON */
            string rawJson = req.downloadHandler.text;
            if (showDebug) Debug.Log($"[Login] HTTP {req.responseCode}\nResponse: {rawJson}");

            if (string.IsNullOrEmpty(rawJson))
            {
                if (errorText != null) errorText.text = "Empty response from server";
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
                if (errorText != null) errorText.text = "Failed to parse server response";
                yield break;
            }

            /* 5) Kiểm tra response */
            if (resp == null)
            {
                if (errorText != null) errorText.text = "Failed to parse server response";
                yield break;
            }

            // Check for success
            if (req.responseCode == 200 && resp.code == 200)
            {
                if (resp.data == null)
                {
                    if (errorText != null) errorText.text = "Invalid response: missing data";
                    yield break;
                }

                if (resp.data.user == null)
                {
                    if (errorText != null) errorText.text = "Invalid response: missing user data";
                    yield break;
                }

                if (string.IsNullOrEmpty(resp.data.token))
                {
                    if (errorText != null) errorText.text = "Invalid response: missing token";
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

                // Load main scene
                SceneManager.LoadScene("MainScene");
            }
            else
            {
                // Login failed
                string errorMsg = resp.message ?? "Login failed";
                if (showDebug) Debug.LogWarning($"[Login] Failed: {errorMsg}");
                if (errorText != null) errorText.text = errorMsg;
            }
        }
        finally
        {
            // Re-enable login button
            if (loginButton != null) loginButton.interactable = true;
        }
    }
}
