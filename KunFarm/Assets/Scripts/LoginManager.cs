using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoginManager : MonoBehaviour
{
    [Header("UI References")]
   public TMP_InputField usernameInput;   // thay InputField
    public TMP_InputField passwordInput;   // thay InputField
    public Button         loginButton;
    public TMP_Text       errorText; 

    [Header("Settings")]
    public string    loginUrl = "http://localhost:5270/auth/login";

    [Serializable]
    private class LoginRequest
    {
        public string usernameOrEmail;
        public string password;
    }

    [Serializable]
    private class UserData
    {
        public int    id;
        public string username;
        public string email;
        public string displayName;
        public string lastLoginAt;
        public string role;
    }

    [Serializable]
    private class LoginResponse
    {
        public bool     success;
        public string   message;
        public UserData user;
        public string   token;
    }

    void Start()
    {
        errorText.text = "";
        loginButton.onClick.AddListener(OnLogin);
    }

    void OnLogin()
    {
        errorText.text = "";
        StartCoroutine(LoginRoutine());
    }

    IEnumerator LoginRoutine()
    {
        // 1) Build request payload
        var reqData = new LoginRequest {
            usernameOrEmail = usernameInput.text,
            password        = passwordInput.text
        };
        string jsonPayload = JsonUtility.ToJson(reqData);

        // 2) Create & send UnityWebRequest
        using var req = new UnityWebRequest(loginUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        req.uploadHandler   = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        // 3) Network or server‐side error?
        if (req.result != UnityWebRequest.Result.Success)
        {
            errorText.text = "Network error: " + req.error;
            yield break;
        }

        // 4) Parse JSON
        var resp = JsonUtility.FromJson<LoginResponse>(req.downloadHandler.text);

        // 5) Check your success flag
        if (!resp.success)
        {
            errorText.text = resp.message;
            yield break;
        }

        // 6) Store token & proceed
        PlayerPrefs.SetString("JWT_TOKEN", resp.token);
        PlayerPrefs.Save();

        // optionally cache user info:
        // PlayerPrefs.SetString("PLAYER_NAME", resp.user.username);

        // 7) Load your main scene
        SceneManager.LoadScene("MainScene");
    }
}
