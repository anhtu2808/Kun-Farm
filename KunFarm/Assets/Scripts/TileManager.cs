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
                // Khi trồng cây, có thể ẩn Tile đã đào hoặc giữ lại tùy ý.
                // Ở đây, ta vẫn giữ Tile đã đào dưới cây.
                // Nếu muốn ẩn, có thể set lại hiddenInteractableTile:
                // interactableMap.SetTile(cellPosition, hiddenInteractableTile);
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
}