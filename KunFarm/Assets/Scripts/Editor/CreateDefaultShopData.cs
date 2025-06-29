using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor script để tạo ShopData mặc định với các items cơ bản
/// </summary>
public static class CreateDefaultShopData
{
    [MenuItem("Assets/Create/Shop/Default Shop Data")]
    public static void CreateDefaultShopDataAsset()
    {
        // Tạo ShopData asset
        ShopData shopData = ScriptableObject.CreateInstance<ShopData>();
        
        // Thêm các shop items mặc định
        shopData.shopItems = new List<ShopItem>
        {
            // Seeds
            new ShopItem
            {
                collectableType = CollectableType.WHEATSEED,
                itemName = "Wheat Seed",
                canBuy = true,
                buyPrice = 10,
                canSell = true,
                sellPrice = 5,
                showInShop = true,
                stockLimit = 50,
                currentStock = 50
            },
            
            new ShopItem
            {
                collectableType = CollectableType.GRAPESEED,
                itemName = "Grape Seed",
                canBuy = true,
                buyPrice = 20,
                canSell = true,
                sellPrice = 10,
                showInShop = true,
                stockLimit = 30,
                currentStock = 30
            },
            
            new ShopItem
            {
                collectableType = CollectableType.APPLETREESEED,
                itemName = "Apple Tree Seed",
                canBuy = true,
                buyPrice = 50,
                canSell = true,
                sellPrice = 25,
                showInShop = true,
                stockLimit = 10,
                currentStock = 10
            },
            
            // Harvested Items
            new ShopItem
            {
                collectableType = CollectableType.WHEAT,
                itemName = "Wheat",
                canBuy = false,
                buyPrice = 0,
                canSell = true,
                sellPrice = 15,
                showInShop = false
            },
            
            new ShopItem
            {
                collectableType = CollectableType.GRAPE,
                itemName = "Grape",
                canBuy = false,
                buyPrice = 0,
                canSell = true,
                sellPrice = 25,
                showInShop = false
            },
            
            new ShopItem
            {
                collectableType = CollectableType.APPLETREE,
                itemName = "Apple",
                canBuy = false,
                buyPrice = 0,
                canSell = true,
                sellPrice = 60,
                showInShop = false
            },
            
            // Tools
            new ShopItem
            {
                collectableType = CollectableType.SHOVEL_TOOL,
                itemName = "Shovel",
                canBuy = true,
                buyPrice = 100,
                canSell = true,
                sellPrice = 50,
                showInShop = true,
                stockLimit = 5,
                currentStock = 5
            },
            
            // Special Items
            new ShopItem
            {
                collectableType = CollectableType.EGG,
                itemName = "Egg",
                canBuy = false,
                buyPrice = 0,
                canSell = true,
                sellPrice = 30,
                showInShop = false
            }
        };
        
        // Tạo file asset
        string path = "Assets/Resources/ShopData/DefaultShopData.asset";
        
        // Tạo thư mục nếu chưa tồn tại
        string directory = System.IO.Path.GetDirectoryName(path);
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
        
        AssetDatabase.CreateAsset(shopData, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Focus vào asset vừa tạo
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = shopData;
        
        Debug.Log($"Default ShopData created at: {path}");
        Debug.Log("Remember to assign item icons in the Inspector!");
    }
    
    [MenuItem("Tools/Shop/Setup Shop Icons")]
    public static void SetupShopIcons()
    {
        Debug.Log("To setup shop icons:");
        Debug.Log("1. Find your ShopData asset in Project window");
        Debug.Log("2. Select it and look at Inspector");
        Debug.Log("3. Assign itemIcon for each ShopItem from your sprites");
        Debug.Log("4. You can find item sprites in Assets/Prefabs/Collectable/ or similar folders");
    }
} 