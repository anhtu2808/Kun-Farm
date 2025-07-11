using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using System.Text.RegularExpressions;

public class RegisterManager : MonoBehaviour
{
    /* ---------- UI ---------- */
    [Header("UI References")]
    public TMP_InputField nameInput;        // Will be mapped to displayName
    public TMP_InputField emailInput;
    public TMP_InputField userNameInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;
    public Button registerButton;
    public Button backButton;

    /* ---------- Settings ---------- */
    [Header("Settings")]
    // API URL will be loaded from ApiClient.BaseUrl
    [SerializeField] private bool showDebug = true;

    /* ---------- DTO ---------- */
    [System.Serializable]
    public class RegisterRequest
    {
        public string username;
        public string email;
        public string password;
        public string displayName;
    }

    [System.Serializable]
    public class UserDto
    {
        public int id;
        public string username;
        public string email;
        public string displayName;
        public string createdAt;
        public string role;
    }

    [System.Serializable]
    public class RegisterData
    {
        public string token;
        public UserDto user;
    }

    [System.Serializable]
    public class ApiResponse
    {
        public int code;
        public string message;
        public RegisterData data;
    }

    /* ---------- Unity ---------- */
    void Start()
    {
        // Setup register button
        if (registerButton != null)
        {
            registerButton.onClick.AddListener(OnRegister);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBack);
        }

        // Ensure ApiClient exists
        if (ApiClient.Instance == null)
        {
            GameObject apiClientGO = new GameObject("ApiClient");
            apiClientGO.AddComponent<ApiClient>();
            DontDestroyOnLoad(apiClientGO);
        }
    }

    public void OnRegister()
    {
        // Validate all inputs
        if (!ValidateInputs())
        {
            return;
        }

        StartCoroutine(RegisterRoutine());
    }

    public void OnBack()
    {
        SceneManager.LoadScene("LoginScene");
    }


    bool ValidateInputs()
    {
        // Check if UI components are assigned
        if (nameInput == null || emailInput == null || userNameInput == null ||
            passwordInput == null || confirmPasswordInput == null)
        {
            SimpleNotificationPopup.Show("UI components not properly assigned!");
            return false;
        }

        // Check if all fields are filled
        if (string.IsNullOrEmpty(nameInput.text.Trim()))
        {
            SimpleNotificationPopup.Show("Please enter your name!");
            return false;
        }

        if (string.IsNullOrEmpty(emailInput.text.Trim()))
        {
            SimpleNotificationPopup.Show("Please enter your email!");
            return false;
        }

        if (string.IsNullOrEmpty(userNameInput.text.Trim()))
        {
            SimpleNotificationPopup.Show("Please enter a username!");
            return false;
        }

        if (string.IsNullOrEmpty(passwordInput.text))
        {
            SimpleNotificationPopup.Show("Please enter a password!");
            return false;
        }

        if (string.IsNullOrEmpty(confirmPasswordInput.text))
        {
            SimpleNotificationPopup.Show("Please confirm your password!");
            return false;
        }

        // Validate email format
        if (!IsValidEmail(emailInput.text.Trim()))
        {
            SimpleNotificationPopup.Show("Please enter a valid email address!");
            return false;
        }

        // Validate username length
        if (userNameInput.text.Trim().Length < 3)
        {
            SimpleNotificationPopup.Show("Username must be at least 3 characters long!");
            return false;
        }

        // Validate password length
        if (passwordInput.text.Length < 6)
        {
            SimpleNotificationPopup.Show("Password must be at least 6 characters long!");
            return false;
        }

        // Check password confirmation
        if (passwordInput.text != confirmPasswordInput.text)
        {
            SimpleNotificationPopup.Show("Passwords do not match!");
            return false;
        }

        return true;
    }

    bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        // Simple email regex pattern
        string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        return Regex.IsMatch(email, pattern);
    }

    IEnumerator RegisterRoutine()
    {
        if (registerButton != null)
            registerButton.interactable = false;

        // Build payload
        var registerRequest = new RegisterRequest();
        registerRequest.username = userNameInput.text.Trim();
        registerRequest.email = emailInput.text.Trim();
        registerRequest.password = passwordInput.text;
        registerRequest.displayName = nameInput.text.Trim();

        string jsonPayload = JsonUtility.ToJson(registerRequest);

        string apiUrl = $"{ApiClient.BaseUrl}/auth/register";
        if (showDebug)
            Debug.Log("[Register] POST " + apiUrl + " Payload: " + jsonPayload);

        // Send request
        UnityWebRequest req = new UnityWebRequest(apiUrl, "POST");
        req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonPayload));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        yield return req.SendWebRequest();

        // Handle response
        bool success = false;
        string errorMessage = "";

        try
        {
            if (req.result != UnityWebRequest.Result.Success)
            {
                if (req.responseCode == 400)
                {
                    errorMessage = "Invalid registration data. Please check your inputs.";
                }
                else if (req.responseCode == 409)
                {
                    errorMessage = "Username or email already exists!";
                }
                else if (req.responseCode >= 500)
                {
                    errorMessage = "Server error. Please try again later.";
                }
                else
                {
                    errorMessage = "Registration failed. Please try again.";
                }
            }
            else
            {
                string rawJson = req.downloadHandler.text;
                if (showDebug)
                    Debug.Log("[Register] HTTP " + req.responseCode + " Response: " + rawJson);

                if (string.IsNullOrEmpty(rawJson))
                {
                    errorMessage = "Empty response from server";
                }
                else
                {
                    ApiResponse resp = JsonUtility.FromJson<ApiResponse>(rawJson);

                    if (resp != null && req.responseCode == 200 && resp.code == 200)
                    {
                        if (resp.data != null && resp.data.user != null)
                        {
                            if (showDebug)
                                Debug.Log("Registration successful! Welcome " + resp.data.user.displayName);

                            SimpleNotificationPopup.Show("Registration successful! Welcome " + resp.data.user.displayName + "!");

                            if (!string.IsNullOrEmpty(resp.data.token))
                            {
                                PlayerPrefs.SetString("JWT_TOKEN", resp.data.token);
                                PlayerPrefs.SetInt("PLAYER_ID", resp.data.user.id);
                                PlayerPrefs.SetString("PLAYER_NAME", resp.data.user.username ?? "");
                                PlayerPrefs.SetString("PLAYER_DISPLAYNAME", resp.data.user.displayName ?? "");
                                PlayerPrefs.Save();

                                if (ApiClient.Instance != null)
                                {
                                    ApiClient.Instance.SetToken(resp.data.token);
                                }

                                success = true;
                            }
                            else
                            {
                                success = true;
                                errorMessage = "login_redirect"; // Special flag for login redirect
                            }
                        }
                        else
                        {
                            errorMessage = "Invalid response: missing user data";
                        }
                    }
                    else
                    {
                        string respErrorMsg = resp != null ? resp.message : "Registration failed";
                        if (showDebug)
                            Debug.LogWarning("[Register] Failed: " + respErrorMsg);

                        if (respErrorMsg.ToLower().Contains("username"))
                        {
                            errorMessage = "Username already exists! Please choose a different username.";
                        }
                        else if (respErrorMsg.ToLower().Contains("email"))
                        {
                            errorMessage = "Email already exists! Please use a different email address.";
                        }
                        else
                        {
                            errorMessage = respErrorMsg;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (showDebug)
                Debug.LogError("Registration Exception: " + ex.Message);
            errorMessage = "An unexpected error occurred. Please try again.";
        }
        finally
        {
            req.Dispose();

            if (registerButton != null)
                registerButton.interactable = true;
        }

        // Handle final result outside try-catch
        if (success)
        {
            if (errorMessage == "login_redirect")
            {
                yield return new WaitForSeconds(2f);
                SceneManager.LoadScene("LoginScene");
            }
            else
            {
                yield return new WaitForSeconds(2f);
                SceneManager.LoadScene("MainScene");
            }
        }
        else if (!string.IsNullOrEmpty(errorMessage))
        {
            SimpleNotificationPopup.Show(errorMessage);
        }
    }

    /* ---------- Public Methods ---------- */
    public void GoToLogin()
    {
        SceneManager.LoadScene("LoginScene");
    }

    public void ClearAllFields()
    {
        if (nameInput != null) nameInput.text = "";
        if (emailInput != null) emailInput.text = "";
        if (userNameInput != null) userNameInput.text = "";
        if (passwordInput != null) passwordInput.text = "";
        if (confirmPasswordInput != null) confirmPasswordInput.text = "";
    }
}