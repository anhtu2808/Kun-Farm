using UnityEngine;
using TMPro;

public class MoneyUI : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;
    private Wallet wallet;

    void Awake()
    {
        // Tìm Wallet trên Player (hoặc trong scene)
        wallet = FindObjectOfType<Wallet>();       
    }

    void OnEnable()  => wallet.OnMoneyChanged += UpdateUI;
    void OnDisable() => wallet.OnMoneyChanged -= UpdateUI;

    void Start() => UpdateUI(wallet.Money);

    void UpdateUI(int value) => moneyText.text = $"{value} G";
}
