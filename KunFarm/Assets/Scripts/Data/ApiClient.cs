using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;

public class ApiClient : MonoBehaviour
{
    public static ApiClient Instance { get; private set; }

    [SerializeField] private string baseUrl = "http://localhost:5270"; // đổi khi build
    private string bearerToken;                                        // token hiện tại

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    /* ------------ Public API ------------ */

    public void SetToken(string token) => bearerToken = token;

    public IEnumerator PostJson(
        string path,
        string json,
        Action<string> onSuccess,
        Action<string> onError)
    {
        using UnityWebRequest req = BuildRequest("POST", path, json);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(req.downloadHandler.text);
        else
            onError?.Invoke(req.error);
    }

    public IEnumerator Get(
        string path,
        Action<string> onSuccess,
        Action<string> onError)
    {
        using UnityWebRequest req = BuildRequest("GET", path);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(req.downloadHandler.text);
        else
            onError?.Invoke(req.error);
    }

    /* ------------ Helpers ------------ */

    private UnityWebRequest BuildRequest(string method, string path, string jsonBody = null)
    {
        var req = new UnityWebRequest(baseUrl + path, method);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        // Token luôn tự động gắn nếu có
        if (!string.IsNullOrEmpty(bearerToken))
            req.SetRequestHeader("Authorization", $"Bearer {bearerToken}");

        if (jsonBody != null)
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        }

        return req;
    }
}
