using UnityEngine;
using System;

/// <summary>
/// Quản lý tiền + tự gửi save mỗi khi thay đổi.
/// </summary>
[RequireComponent(typeof(Transform))]
public class Wallet : MonoBehaviour
{
    [Header("Runtime data")]
    [SerializeField] private int money = 1000;

    [Tooltip("Lấy từ PlayerPrefs sau login (PLAYER_ID)")]
    [SerializeField] private int userId = 0;

    public int Money => money;

    public event Action<int> OnMoneyChanged;

    /* ---------- LIFECYCLE ---------- */

    private void Awake()
    {
        // Tự lấy ID đã lưu khi login
        userId = PlayerPrefs.GetInt("PLAYER_ID", 0);
    }

    private void Start()
    {
        // Đảm bảo có ApiClient và GameSaver
        if (ApiClient.Instance == null)
        {
            Debug.LogWarning("[Wallet] ApiClient instance not found!");
        }
        
        if (GameSaver.Instance == null)
        {
            Debug.LogWarning("[Wallet] GameSaver instance not found!");
        }
    }

    /* ---------- PUBLIC API ---------- */

    public void Add(int amount)
    {
        if (amount <= 0) return;
        
        money += amount;
        OnMoneyChanged?.Invoke(money);
        SaveToServer();
    }

    public bool Spend(int amount)
    {
        if (amount <= 0 || money < amount) 
        {
            return false;
        }
        
        money -= amount;
        OnMoneyChanged?.Invoke(money);
        SaveToServer();
        return true;
    }

    public void SetMoney(int amount)
    {
        money = amount;
        OnMoneyChanged?.Invoke(money);
    }

    /* ---------- INTERNAL ---------- */

    private void SaveToServer()
    {
        if (userId <= 0)
        {
            return;
        }

        if (GameSaver.Instance == null)
        {
            return;
        }

        Vector3 p = transform.position;

        var data = new PlayerSaveData
        {
            userId = userId,
            money = money,
            posX = p.x,
            posY = p.y,
            posZ = p.z
        };

        GameSaver.Instance.SaveGame(data);
    }
}
