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
            
            // Load token from PlayerPrefs if exists
            string savedToken = PlayerPrefs.GetString("JWT_TOKEN", "");
            if (!string.IsNullOrEmpty(savedToken))
            {
                SetToken(savedToken);
                Debug.Log("[ApiClient] Token loaded from PlayerPrefs");
            }
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
        
        // Use unscaled time to prevent pausing issues
        var operation = req.SendWebRequest();
        while (!operation.isDone)
        {
            yield return null;
        }

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
        
        // Use unscaled time to prevent pausing issues
        var operation = req.SendWebRequest();
        while (!operation.isDone)
        {
            yield return null;
        }

        if (req.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(req.downloadHandler.text);
        else
            onError?.Invoke(req.error);
    }

    public void ResetGame(int userId, Action<bool> onComplete)
    {
        Debug.Log($"[ApiClient] Resetting game for user {userId}");
        
        StartCoroutine(PostJson($"/game/reset/{userId}", "", 
            response => {
                try 
                {
                    Debug.Log($"[ApiClient] Raw reset response: {response}");
                    var apiResponse = JsonUtility.FromJson<ApiResponseWrapper<bool>>(response);
                    bool success = apiResponse.success && apiResponse.data;
                    Debug.Log($"[ApiClient] Parsed reset response - Code: {apiResponse.code}, Success: {success}");
                    onComplete?.Invoke(success);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[ApiClient] ❌ Error parsing reset response: {e.Message}\nResponse: {response}");
                    onComplete?.Invoke(false);
                }
            },
            error => {
                Debug.LogError($"[ApiClient] ❌ Network error resetting game: {error}");
                onComplete?.Invoke(false);
            }));
    }

    // Farm State API methods
    public void SaveFarmState(FarmSaveData farmData, Action<bool> onComplete)
    {
        string json = JsonUtility.ToJson(farmData);
        Debug.Log($"[ApiClient] Saving farm state for user {farmData.userId}");
        
        StartCoroutine(PostJson("/game/farm/save", json, 
            response => {
                try 
                {
                    Debug.Log($"[ApiClient] Raw save response: {response}");
                    var apiResponse = JsonUtility.FromJson<ApiResponseWrapper<bool>>(response);
                    bool success = apiResponse.success && apiResponse.data;
                    Debug.Log($"[ApiClient] Parsed save response - Code: {apiResponse.code}, Data: {apiResponse.data}, Success: {success}");
                    onComplete?.Invoke(success);
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[ApiClient] ❌ Error parsing farm save response: {e.Message}\nResponse: {response}");
                    onComplete?.Invoke(false);
                }
            },
            error => {
                Debug.LogError($"[ApiClient] ❌ Network error saving farm state: {error}");
                onComplete?.Invoke(false);
            }));
    }

    public void LoadFarmState(int userId, Action<FarmSaveData> onComplete)
    {
        Debug.Log($"[ApiClient] Loading farm state for user {userId}");
        
        StartCoroutine(Get($"/game/farm/load/{userId}",
            response => {
                try
                {
                    Debug.Log($"[ApiClient] Raw load response: {response}");
                    var apiResponse = JsonUtility.FromJson<ApiResponseWrapper<FarmStateServerResponse>>(response);
                    
                    // Debug server response structure
                    Debug.Log($"[ApiClient] API Response - Success: {apiResponse.success}, Code: {apiResponse.code}");
                    if (apiResponse.data != null)
                    {
                        Debug.Log($"[ApiClient] Server data - UserId: {apiResponse.data.userId}");
                        Debug.Log($"[ApiClient] Server data - TileStates: {apiResponse.data.tileStates?.Length ?? 0}");
                        Debug.Log($"[ApiClient] Server data - Plants: {apiResponse.data.plants?.Length ?? 0}");
                        Debug.Log($"[ApiClient] Server data - ChickensStateJson length: {apiResponse.data.chickensStateJson?.Length ?? 0}");
                        Debug.Log($"[ApiClient] Server data - EggsStateJson length: {apiResponse.data.eggsStateJson?.Length ?? 0}");
                        
                        // Debug first few plants
                        if (apiResponse.data.plants != null && apiResponse.data.plants.Length > 0)
                        {
                            for (int i = 0; i < Math.Min(3, apiResponse.data.plants.Length); i++)
                            {
                                var plant = apiResponse.data.plants[i];
                                Debug.Log($"[ApiClient] Server plant {i}: ({plant.x}, {plant.y}, {plant.z}) cropType: '{plant.cropType}' stage: {plant.currentStage}");
                            }
                        }
                    }
                    
                    if (apiResponse.success && apiResponse.data != null)
                    {
                        // Helper function để clean JSON strings từ server
                        string CleanJsonString(string jsonString)
                        {
                            if (string.IsNullOrEmpty(jsonString) || 
                                jsonString.Trim().ToLower() == "null" ||
                                jsonString.Trim() == "{}" ||
                                string.IsNullOrWhiteSpace(jsonString))
                            {
                                return "[]";
                            }
                            return jsonString.Trim();
                        }
                        
                        // Convert server response to Unity format
                        var farmData = new FarmSaveData
                        {
                            userId = apiResponse.data.userId,
                            tileStates = new System.Collections.Generic.List<TileStateData>(),
                            plants = new System.Collections.Generic.List<PlantData>(),
                            chickensStateJson = CleanJsonString(apiResponse.data.chickensStateJson),
                            eggsStateJson = CleanJsonString(apiResponse.data.eggsStateJson)
                        };
                        
                        Debug.Log($"[ApiClient] Cleaned JSON - Chickens: '{farmData.chickensStateJson}', Eggs: '{farmData.eggsStateJson}'");

                        // Convert tile states
                        if (apiResponse.data.tileStates != null)
                        {
                            foreach (var tileState in apiResponse.data.tileStates)
                            {
                                farmData.tileStates.Add(new TileStateData
                                {
                                    x = tileState.x,
                                    y = tileState.y,
                                    z = tileState.z,
                                    state = tileState.state
                                });
                            }
                        }

                        // Convert plants with validation
                        if (apiResponse.data.plants != null)
                        {
                            foreach (var plant in apiResponse.data.plants)
                            {
                                // Validate plant data before adding
                                if (string.IsNullOrEmpty(plant.cropType))
                                {
                                    Debug.LogWarning($"[ApiClient] Skipping plant at ({plant.x}, {plant.y}, {plant.z}) - empty cropType from server");
                                    continue;
                                }
                                
                                farmData.plants.Add(new PlantData
                                {
                                    x = plant.x,
                                    y = plant.y,
                                    z = plant.z,
                                    cropType = plant.cropType,
                                    currentStage = plant.currentStage,
                                    timer = plant.timer,
                                    isMature = plant.isMature
                                });
                            }
                        }

                        Debug.Log($"[ApiClient] ✅ Farm load successful - {farmData.tileStates.Count} tiles, {farmData.plants.Count} plants, chickens: {(string.IsNullOrEmpty(farmData.chickensStateJson) ? 0 : farmData.chickensStateJson.Length)} chars, eggs: {(string.IsNullOrEmpty(farmData.eggsStateJson) ? 0 : farmData.eggsStateJson.Length)} chars");
                        onComplete?.Invoke(farmData);
                    }
                    else
                    {
                        Debug.LogWarning("[ApiClient] ⚠️ Failed to load farm state - invalid response or empty data");
                        onComplete?.Invoke(null);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[ApiClient] ❌ Error parsing farm load response: {e.Message}\nResponse: {response}");
                    onComplete?.Invoke(null);
                }
            },
            error => {
                Debug.LogError($"[ApiClient] ❌ Network error loading farm state: {error}");
                onComplete?.Invoke(null);
            }));
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

// Response wrapper classes for farm state API
[System.Serializable]
public class ApiResponseWrapper<T>
{
    public int code;
    public T data;
    public string message;
    
    // Helper property to check if request was successful
    public bool success => code == 200;
}

[System.Serializable]
public class FarmStateServerResponse
{
    public int userId;
    public ServerTileStateData[] tileStates;
    public ServerPlantData[] plants;
    public string chickensStateJson;
    public string eggsStateJson;
    public string lastSaved; // Server includes this field
}

[System.Serializable]
public class ServerTileStateData
{
    public int x;
    public int y;
    public int z;
    public int state;
}

[System.Serializable]
public class ServerPlantData
{
    public int x;
    public int y;
    public int z;
    public string cropType;
    public int currentStage;
    public float timer;
    public bool isMature;
}
