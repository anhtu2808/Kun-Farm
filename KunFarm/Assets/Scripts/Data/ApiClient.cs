using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;

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

    /* ------------ Marketplace API ------------ */

    /// <summary>
    /// Tạo marketplace item mới
    /// </summary>
    public async System.Threading.Tasks.Task<ApiResponse<MarketplaceItemResponse>> CreateMarketplaceItem(CreateMarketplaceItemRequest request)
    {
        var json = JsonUtility.ToJson(request);
        var response = await PostJsonAsync("/api/marketplace/create", json);
        
        if (response.Success)
        {
            var itemResponse = JsonUtility.FromJson<MarketplaceItemResponse>(response.Data);
            return new ApiResponse<MarketplaceItemResponse> { Success = true, Data = itemResponse };
        }
        
        return new ApiResponse<MarketplaceItemResponse> { Success = false, Message = response.Message };
    }

    /// <summary>
    /// Mua marketplace item
    /// </summary>
    public async System.Threading.Tasks.Task<ApiResponse<MarketplaceItemResponse>> BuyMarketplaceItem(BuyMarketplaceItemRequest request)
    {
        var json = JsonUtility.ToJson(request);
        var response = await PostJsonAsync("/api/marketplace/buy", json);
        
        if (response.Success)
        {
            var itemResponse = JsonUtility.FromJson<MarketplaceItemResponse>(response.Data);
            return new ApiResponse<MarketplaceItemResponse> { Success = true, Data = itemResponse };
        }
        
        return new ApiResponse<MarketplaceItemResponse> { Success = false, Message = response.Message };
    }

    /// <summary>
    /// Lấy danh sách items đang hoạt động
    /// </summary>
    public async System.Threading.Tasks.Task<ApiResponse<List<MarketplaceItemResponse>>> GetActiveMarketplaceItems()
    {
        var response = await GetAsync("/api/marketplace/items");
        
        if (response.Success)
        {
            var itemsResponse = JsonUtility.FromJson<MarketplaceItemsResponse>(response.Data);
            return new ApiResponse<List<MarketplaceItemResponse>> { Success = true, Data = itemsResponse.Items };
        }
        
        return new ApiResponse<List<MarketplaceItemResponse>> { Success = false, Message = response.Message };
    }

    /// <summary>
    /// Tìm kiếm marketplace items
    /// </summary>
    public async System.Threading.Tasks.Task<ApiResponse<List<MarketplaceItemResponse>>> SearchMarketplaceItems(string searchTerm)
    {
        var response = await GetAsync($"/api/marketplace/search?term={UnityWebRequest.EscapeURL(searchTerm)}");
        
        if (response.Success)
        {
            var itemsResponse = JsonUtility.FromJson<MarketplaceItemsResponse>(response.Data);
            return new ApiResponse<List<MarketplaceItemResponse>> { Success = true, Data = itemsResponse.Items };
        }
        
        return new ApiResponse<List<MarketplaceItemResponse>> { Success = false, Message = response.Message };
    }

    /// <summary>
    /// Lấy lịch sử giao dịch của user
    /// </summary>
    public async System.Threading.Tasks.Task<ApiResponse<List<MarketplaceTransactionResponse>>> GetUserMarketplaceTransactions()
    {
        var response = await GetAsync("/api/marketplace/transactions");
        
        if (response.Success)
        {
            var transactionsResponse = JsonUtility.FromJson<MarketplaceTransactionsResponse>(response.Data);
            return new ApiResponse<List<MarketplaceTransactionResponse>> { Success = true, Data = transactionsResponse.Transactions };
        }
        
        return new ApiResponse<List<MarketplaceTransactionResponse>> { Success = false, Message = response.Message };
    }

    /// <summary>
    /// Hủy marketplace item
    /// </summary>
    public async System.Threading.Tasks.Task<ApiResponse<bool>> CancelMarketplaceItem(int itemId)
    {
        var response = await PostJsonAsync($"/api/marketplace/cancel/{itemId}", "");
        
        return new ApiResponse<bool> { Success = response.Success, Data = response.Success, Message = response.Message };
    }

    /* ------------ Async Helpers ------------ */

    private async System.Threading.Tasks.Task<ApiResponse<string>> PostJsonAsync(string path, string json)
    {
        var tcs = new System.Threading.Tasks.TaskCompletionSource<ApiResponse<string>>();
        
        StartCoroutine(PostJson(path, json, 
            result => tcs.SetResult(new ApiResponse<string> { Success = true, Data = result }),
            error => tcs.SetResult(new ApiResponse<string> { Success = false, Message = error })));
        
        return await tcs.Task;
    }

    private async System.Threading.Tasks.Task<ApiResponse<string>> GetAsync(string path)
    {
        var tcs = new System.Threading.Tasks.TaskCompletionSource<ApiResponse<string>>();
        
        StartCoroutine(Get(path, 
            result => tcs.SetResult(new ApiResponse<string> { Success = true, Data = result }),
            error => tcs.SetResult(new ApiResponse<string> { Success = false, Message = error })));
        
        return await tcs.Task;
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

/* ------------ Response Wrapper Classes ------------ */

[System.Serializable]
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public string Message { get; set; }
}

[System.Serializable]
public class MarketplaceItemsResponse
{
    public List<MarketplaceItemResponse> Items;
}

[System.Serializable]
public class MarketplaceTransactionsResponse
{
    public List<MarketplaceTransactionResponse> Transactions;
}
