using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop_UI : MonoBehaviour
{
     public GameObject shopPanel;

    // Property để kiểm tra Shop có đang mở không
    public bool IsOpen => shopPanel.activeSelf;

    void Start()
    {
        // Đảm bảo shop luôn tắt khi bắt đầu
        shopPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleShop();
        }
    }

    public void ToggleShop()
    {
        if (!shopPanel.activeSelf)
        {
            shopPanel.SetActive(true);
            // TODO: Gọi Refresh shop item nếu có
        }
        else
        {
            shopPanel.SetActive(false);
        }
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
    }
}
