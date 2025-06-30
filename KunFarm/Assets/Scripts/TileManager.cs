using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Đảm bảo Enum này nằm ngoài bất kỳ class nào để dễ dàng truy cập


public class TileManager : MonoBehaviour
{
    [SerializeField] private Tilemap interactableMap; // Tilemap chứa các ô đất có thể tương tác
    [SerializeField] private TileBase hiddenInteractableTile; // Tile dùng để ẩn các ô tương tác ban đầu
    [SerializeField] private TileBase dugGroundTile;      // Tile dùng để hiển thị đất đã đào

    // Dictionary để lưu trữ trạng thái hiện tại của từng ô đất có thể tương tác
    private Dictionary<Vector3Int, TileState> tileStates;

    // Dictionary để lưu trữ GameObject cây được trồng trên mỗi ô đất
    // Dùng để quản lý và thu hoạch cây sau này
    private Dictionary<Vector3Int, GameObject> plantedTrees;

    void Awake()
    {
        tileStates = new Dictionary<Vector3Int, TileState>();
        plantedTrees = new Dictionary<Vector3Int, GameObject>();

        // Lặp qua tất cả các ô trong phạm vi của interactableMap
        foreach (var pos in interactableMap.cellBounds.allPositionsWithin)
        {
            TileBase tile = interactableMap.GetTile(pos);

            // Chỉ xử lý các ô được đánh dấu là "Interactable_Visible"
            if (tile != null && tile.name == "Interactable_Visible")
            {
                // Ẩn tile đi bằng cách đặt nó thành hiddenInteractableTile
                // (Hãy đảm bảo hiddenInteractableTile là một Tile trống hoặc trong suốt)
                interactableMap.SetTile(pos, hiddenInteractableTile);

                // Khởi tạo trạng thái của ô này là Undug (chưa đào)
                tileStates[pos] = TileState.Undug;
            }
        }
    }

    /// <summary>
    /// Kiểm tra xem một ô có thể tương tác được không (tức là nó nằm trên interactableMap).
    /// </summary>
    public bool IsInteractable(Vector3Int position)
    {
        // Kiểm tra xem vị trí có tồn tại trong dictionary các trạng thái ô đất không.
        // Điều này đảm bảo chỉ những ô ban đầu là "Interactable_Visible" mới có thể tương tác.
        return tileStates.ContainsKey(position);
    }

    /// <summary>
    /// Lấy trạng thái hiện tại của một ô đất.
    /// </summary>
    public TileState GetTileState(Vector3Int cellPosition)
    {
        if (tileStates.ContainsKey(cellPosition))
        {
            return tileStates[cellPosition];
        }
        // Trả về Undug nếu không phải là ô tương tác hoặc không có trong danh sách
        return TileState.Undug;
    }

    /// <summary>
    /// Đặt trạng thái mới cho một ô đất và cập nhật Tilemap nếu cần.
    /// </summary>
    public void SetTileState(Vector3Int cellPosition, TileState newState)
    {
        if (tileStates.ContainsKey(cellPosition))
        {
            tileStates[cellPosition] = newState;

            // Cập nhật hiển thị trên Tilemap dựa trên trạng thái mới
            if (newState == TileState.Dug)
            {
                // Thay đổi Tile trên interactableMap thành Tile "đã đào"
                interactableMap.SetTile(cellPosition, dugGroundTile);
            }
            else if (newState == TileState.Planted)
            {
                // Khi trồng cây, hiển thị đất đã đào dưới cây để player biết có thể tương tác
                interactableMap.SetTile(cellPosition, dugGroundTile);
            }
            else if (newState == TileState.Harvested)
            {
                // Khi cây đã trưởng thành và có thể thu hoạch, hiển thị đất đã đào
                interactableMap.SetTile(cellPosition, dugGroundTile);
            }
            else if (newState == TileState.Undug)
            {
                // Khi trở về trạng thái chưa đào, đặt lại Tile ban đầu (ẩn đi)
                interactableMap.SetTile(cellPosition, hiddenInteractableTile);
            }
        }
        else
        {
            Debug.LogWarning($"Attempted to set state for non-interactable tile at {cellPosition}");
        }
    }

    /// <summary>
    /// Lưu trữ GameObject của cây được trồng trên một ô cụ thể.
    /// </summary>
    public void RegisterPlant(Vector3Int cellPosition, GameObject plantObject)
    {
        if (IsInteractable(cellPosition))
        {
            plantedTrees[cellPosition] = plantObject;
        }
        else
        {
            Debug.LogWarning($"Attempted to register plant on a non-interactable tile at {cellPosition}");
        }
    }

    /// <summary>
    /// Lấy GameObject cây trên một ô cụ thể.
    /// </summary>
    public GameObject GetPlantAt(Vector3Int cellPosition)
    {
        if (plantedTrees.ContainsKey(cellPosition))
        {
            return plantedTrees[cellPosition];
        }
        return null;
    }

    /// <summary>
    /// Xóa GameObject cây khỏi một ô cụ thể (sau khi thu hoạch).
    /// </summary>
    public void DeregisterPlant(Vector3Int cellPosition)
    {
        if (plantedTrees.ContainsKey(cellPosition))
        {
            plantedTrees.Remove(cellPosition);
        }
    }

    /// <summary>
    /// Lấy Tilemap để convert cell position thành world position
    /// </summary>
    public Tilemap GetTilemap()
    {
        return interactableMap;
    }

    /// <summary>
    /// Lấy tất cả tile states để lưu vào server
    /// </summary>
    public Dictionary<Vector3Int, TileState> GetAllTileStates()
    {
        return new Dictionary<Vector3Int, TileState>(tileStates);
    }

    /// <summary>
    /// Lấy tất cả plants để lưu vào server
    /// </summary>
    public Dictionary<Vector3Int, GameObject> GetAllPlants()
    {
        return new Dictionary<Vector3Int, GameObject>(plantedTrees);
    }

    /// <summary>
    /// Restore tile states từ server data
    /// </summary>
    public void RestoreTileStates(List<TileStateData> tileStateDataList)
    {
        Debug.Log("=== RESTORE TILE STATES DEBUG START ===");
        Debug.Log($"tileStateDataList null: {tileStateDataList == null}");
        Debug.Log($"tileStateDataList count: {tileStateDataList?.Count ?? 0}");
        Debug.Log($"TileManager active: {gameObject.activeInHierarchy}");
        Debug.Log($"TileManager enabled: {enabled}");
        
        // Test GetTilemap()
        try 
        {
            var tilemap = GetTilemap();
            Debug.Log($"GetTilemap() success: {tilemap != null}");
            if (tilemap != null)
            {
                Debug.Log($"Tilemap name: {tilemap.name}");
                Debug.Log($"Tilemap active: {tilemap.gameObject.activeInHierarchy}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"GetTilemap() failed: {e.Message}");
            Debug.Log("=== RESTORE TILE STATES DEBUG END (EARLY EXIT) ===");
            return;
        }
        
        if (tileStateDataList == null || tileStateDataList.Count == 0)
        {
            Debug.LogWarning("No tile states to restore");
            Debug.Log("=== RESTORE TILE STATES DEBUG END (NO DATA) ===");
            return;
        }
        
        foreach (var tileData in tileStateDataList)
        {
            Vector3Int position = new Vector3Int(tileData.x, tileData.y, tileData.z);
            TileState state = (TileState)tileData.state;
            
            Debug.Log($"[TileManager] Restoring tile at ({tileData.x}, {tileData.y}, {tileData.z}) - state: {state}");
            
            if (IsInteractable(position))
            {
                SetTileState(position, state);
                Debug.Log($"[TileManager] ✅ Tile state set successfully");
            }
            else
            {
                Debug.LogWarning($"[TileManager] Position ({tileData.x}, {tileData.y}, {tileData.z}) is not interactable, skipping");
            }
        }
        
        Debug.Log("=== RESTORE TILE STATES DEBUG END ===");
    }

    /// <summary>
    /// Restore plants từ server data
    /// </summary>
    public void RestorePlants(List<PlantData> plantDataList)
    {
        Debug.Log($"[TileManager] Restoring {plantDataList.Count} plants from server data");
        
        foreach (var plantData in plantDataList)
        {
            Vector3Int position = new Vector3Int(plantData.x, plantData.y, plantData.z);
            Debug.Log($"[TileManager] Restoring plant at ({plantData.x}, {plantData.y}, {plantData.z}) - cropType: '{plantData.cropType}', stage: {plantData.currentStage}, mature: {plantData.isMature}");
            
            // Handle empty cropType with fallback or cleanup
            if (string.IsNullOrEmpty(plantData.cropType))
            {
                Debug.LogWarning($"[TileManager] Plant at ({plantData.x}, {plantData.y}, {plantData.z}) has empty cropType");
                
                // Option 1: Use default crop type
                // plantData.cropType = "Wheat"; // fallback to default
                
                // Option 2: Clean up the tile state (recommended)
                Vector3Int cleanupPosition = new Vector3Int(plantData.x, plantData.y, plantData.z);
                SetTileState(cleanupPosition, TileState.Dug); // Reset to dug state
                Debug.Log($"[TileManager] Reset tile at ({plantData.x}, {plantData.y}, {plantData.z}) to Dug state due to empty cropType");
                continue;
            }
            
            // Load CropData
            CropData cropData = Resources.Load<CropData>($"CropData/{plantData.cropType}");
            if (cropData == null)
            {
                Debug.LogError($"[TileManager] Failed to load CropData for '{plantData.cropType}' at position ({plantData.x}, {plantData.y}, {plantData.z})");
                continue;
            }
            
            if (cropData.cropPrefab == null)
            {
                Debug.LogError($"[TileManager] CropData '{plantData.cropType}' has null cropPrefab");
                continue;
            }
            
            // Instantiate plant
            Vector3 worldPos = GetTilemap().GetCellCenterWorld(position);
            GameObject newPlant = Object.Instantiate(cropData.cropPrefab, worldPos, Quaternion.identity);
            newPlant.name = cropData.cropName + " Plant";
            Debug.Log($"[TileManager] ✅ Created plant '{cropData.cropName}' at world position {worldPos}");

            CropGrower cropGrower = newPlant.GetComponent<CropGrower>();
            if (cropGrower != null)
            {
                cropGrower.SetTilePosition(position);
                cropGrower.RestoreFromSaveData(plantData);
                RegisterPlant(position, newPlant);
                
                // Set correct tile state based on plant maturity
                TileState correctState = plantData.isMature ? TileState.Harvested : TileState.Planted;
                SetTileState(position, correctState);
                Debug.Log($"[TileManager] ✅ Plant registered and tile state set to {correctState} (mature: {plantData.isMature})");
            }
            else
            {
                Debug.LogError($"[TileManager] CropGrower component not found on plant prefab for '{plantData.cropType}'");
                Object.Destroy(newPlant);
            }
        }
    }
}