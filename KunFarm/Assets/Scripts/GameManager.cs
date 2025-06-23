using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour
{

    [Header("Player Data")]
    [SerializeField] private int currency = 0; // Private với SerializeField để bảo mật
    public int score = 0;

    [Header("UI References")]
    public TMP_Text currencyText;
    public TMP_Text scoreText;

    [Header("Currency Settings")] // THÊM MỚI
    public int startingCurrency = 0;
    public int maxCurrency = 999999;
    public bool enableCurrencyEvents = true;

    [Header("Settings")]
    public bool autoSave = true;
    public bool showDebugInfo = false;

    // THÊM EVENTS CHO TEAM KHÁC SỬ DỤNG
    public static event Action<int> OnCurrencyChanged;
    public static event Action<int, int> OnCurrencySpent; // (amount spent, remaining)
    public static event Action<int, int> OnCurrencyAdded; // (amount added, total)
    public static event Action<string> OnCurrencyError;

    public static GameManager instance;

    public ItemManager itemManager;
    public TileManager tileManager;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("Multiple instances of GameManager detected. Destroying the new instance.");
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(this.gameObject);
        itemManager = GetComponent<ItemManager>();
        tileManager = GetComponent<TileManager>();

        // Load dữ liệu khi khởi động
        LoadPlayerData();
    }

    private void Start()
    {
        // Cập nhật UI lần đầu
        UpdateUI();

        // THÊM: Notify initial currency value
        if (enableCurrencyEvents)
            OnCurrencyChanged?.Invoke(currency);
    }

    #region Currency System - IMPROVED

    public bool AddCurrency(int amount)
    {
        // THÊM: Validation
        if (amount <= 0)
        {
            if (showDebugInfo) Debug.LogWarning("Cannot add negative or zero currency!");
            OnCurrencyError?.Invoke("Invalid currency amount");
            return false;
        }

        int previousCurrency = currency;
        currency = Mathf.Min(currency + amount, maxCurrency); // THÊM: Max limit
        int actualAdded = currency - previousCurrency;

        if (showDebugInfo) Debug.Log($"Added {actualAdded} currency. Total: {currency}");

        // THÊM: Events
        if (enableCurrencyEvents)
        {
            OnCurrencyChanged?.Invoke(currency);
            OnCurrencyAdded?.Invoke(actualAdded, currency);
        }

        UpdateUI();
        if (autoSave) SavePlayerData();
        return actualAdded > 0;
    }

    public bool SpendCurrency(int amount)
    {
        // THÊM: Validation
        if (amount <= 0)
        {
            if (showDebugInfo) Debug.LogWarning("Cannot spend negative or zero currency!");
            OnCurrencyError?.Invoke("Invalid spend amount");
            return false;
        }

        if (currency >= amount)
        {
            currency -= amount;
            if (showDebugInfo) Debug.Log($"Spent {amount} currency. Remaining: {currency}");

            // THÊM: Events
            if (enableCurrencyEvents)
            {
                OnCurrencyChanged?.Invoke(currency);
                OnCurrencySpent?.Invoke(amount, currency);
            }

            UpdateUI();
            if (autoSave) SavePlayerData();
            return true;
        }
        else
        {
            if (showDebugInfo) Debug.Log($"Not enough currency! Need: {amount}, Have: {currency}");
            OnCurrencyError?.Invoke($"Not enough coins! Need {amount}, have {currency}");
            return false;
        }
    }

    // THÊM: Method mới cho Shop system
    public bool TryPurchase(int cost, string itemName = "item")
    {
        if (SpendCurrency(cost))
        {
            if (showDebugInfo) Debug.Log($"Successfully purchased {itemName} for {cost} coins!");
            return true;
        }
        return false;
    }

    // THÊM: Set currency trực tiếp (admin/debug)
    public void SetCurrency(int amount)
    {
        int previousCurrency = currency;
        currency = Mathf.Clamp(amount, 0, maxCurrency);

        if (showDebugInfo) Debug.Log($"Currency set from {previousCurrency} to {currency}");

        if (enableCurrencyEvents)
            OnCurrencyChanged?.Invoke(currency);

        UpdateUI();
        if (autoSave) SavePlayerData();
    }

    public void AddScore(int points)
    {
        score += points;
        if (showDebugInfo) Debug.Log("Added " + points + " score. Total: " + score);
        UpdateUI();
        if (autoSave) SavePlayerData();
    }

    #endregion

    #region UI Management - IMPROVED

    private void UpdateUI()
    {
        if (currencyText != null)
        {
            currencyText.text = "Coins: " + currency.ToString();
        }
        else if (showDebugInfo)
        {
            Debug.LogWarning("CurrencyText reference is missing!");
        }

        if (scoreText != null)
            scoreText.text = "Score: " + score.ToString();
    }

    public void SetCurrencyUI(TMP_Text uiText)
    {
        currencyText = uiText;
        UpdateUI();
    }

    public void SetScoreUI(TMP_Text uiText)
    {
        scoreText = uiText;
        UpdateUI();
    }

    #endregion

    #region Save/Load System - IMPROVED

    public void SavePlayerData()
    {
        PlayerPrefs.SetInt("Currency", currency);
        PlayerPrefs.SetInt("Score", score);
        PlayerPrefs.Save();
        if (showDebugInfo) Debug.Log("Player data saved!");
    }

    public void LoadPlayerData()
    {
        currency = PlayerPrefs.GetInt("Currency", startingCurrency); // THÊM: startingCurrency default
        score = PlayerPrefs.GetInt("Score", 0);
        if (showDebugInfo) Debug.Log($"Player data loaded! Currency: {currency}, Score: {score}");
    }

    public void ResetPlayerData()
    {
        currency = startingCurrency; // THÊM: Reset về starting amount
        score = 0;
        SavePlayerData();

        if (enableCurrencyEvents)
            OnCurrencyChanged?.Invoke(currency);

        UpdateUI();
        if (showDebugInfo) Debug.Log("Player data reset!");
    }

    #endregion

    #region Utility Methods - IMPROVED

    public int GetCurrency()
    {
        return currency;
    }

    public int GetScore()
    {
        return score;
    }

    public bool HasEnoughCurrency(int amount)
    {
        return currency >= amount;
    }

    // THÊM: Currency info cho debugging
    public string GetCurrencyInfo()
    {
        return $"Currency: {currency}/{maxCurrency}";
    }

    #endregion

    #region Debug Methods - THÊM MỚI

    [ContextMenu("Add 10 Coins")]
    public void Debug_Add10Coins() => AddCurrency(10);

    [ContextMenu("Add 100 Coins")]
    public void Debug_Add100Coins() => AddCurrency(100);

    [ContextMenu("Add 1000 Coins")]
    public void Debug_Add1000Coins() => AddCurrency(1000);

    [ContextMenu("Spend 50 Coins")]
    public void Debug_Spend50Coins() => SpendCurrency(50);

    [ContextMenu("Reset Currency")]
    public void Debug_ResetCurrency() => SetCurrency(startingCurrency);

    [ContextMenu("Print Currency Info")]
    public void Debug_PrintInfo() => Debug.Log(GetCurrencyInfo());

    #endregion
}