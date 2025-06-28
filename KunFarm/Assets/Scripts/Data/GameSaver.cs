using UnityEngine;
using System.Collections;
using System;

public class GameSaver : MonoBehaviour
{
    public static GameSaver Instance { get; private set; }

    // chỉ còn path vì baseUrl nằm trong ApiClient
    [SerializeField] private string savePath = "/game/save";

    [Serializable]
    private class ApiResponse
    {
        public int code;
        public string message;
        public bool data; // For save game, data is just a boolean
    }

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

    public void SaveGame(PlayerSaveData data)
    {
        if (data == null)
        {
            return;
        }

        if (ApiClient.Instance == null)
        {
            return;
        }

        // Chuyển data → JSON
        string json = JsonUtility.ToJson(data);

        // Gọi PostJson của ApiClient và chờ coroutine
        StartCoroutine(ApiClient.Instance.PostJson(
            savePath,
            json,
            onSuccess: HandleSaveSuccess,
            onError: HandleSaveError
        ));
    }

    private void HandleSaveSuccess(string responseJson)
    {
        try
        {
            if (string.IsNullOrEmpty(responseJson))
            {
                return;
            }

            var response = JsonUtility.FromJson<ApiResponse>(responseJson);
            
            if (response == null)
            {
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameSaver] Error parsing save response: {e.Message}");
            return;
        }
    }

    private void HandleSaveError(string error)
    {
        // Silent fail
    }
}
