using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ShopApiClient : MonoBehaviour
{
    public IEnumerator GetShopItems(System.Action<List<ShopItemDto>> onDone)
    {
        using (var req = UnityWebRequest.Get("http://ocalhost:7067/regular-shop"))
        {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                var json = "{\"items\":" + req.downloadHandler.text + "}";
                var list = JsonUtility.FromJson<ShopItemList>(json);
                onDone?.Invoke(list.items);
            }
            else
            {
                Debug.LogError(req.error);
                onDone?.Invoke(new List<ShopItemDto>());
            }
        }
    }
}

[System.Serializable]
public class ShopItemDto {
  public int id;
  public string collectableType;
  public string itemName;
  public string itemIconUrl;
  public bool canBuy;
  public int buyPrice;
  public bool canSell;
  public int sellPrice;
  public bool showInShop;
  public int stockLimit;
  public int currentStock;
}

[System.Serializable]
public class ShopItemList {
  public List<ShopItemDto> items;
}
