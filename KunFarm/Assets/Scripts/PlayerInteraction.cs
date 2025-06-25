// PlayerInteraction.cs
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerInteraction : MonoBehaviour
{
    // ... (Các biến SerializedField và nội bộ khác của bạn) ...
    [SerializeField] private Tilemap interactableMap;
    [SerializeField] private TileManager tileManager;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private CropData grapeCropDataAsset; // Đảm bảo đã gán Asset này trong Inspector

    public string currentTool = "Shovel";
    private CropData selectedCropData;

    void Update()
    {
        // ... (Logic đổi công cụ bằng phím 1, 2, 3) ...
        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeTool("Shovel", null);
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (grapeCropDataAsset != null) ChangeTool("Seeds", grapeCropDataAsset);
            else Debug.LogError("grapeCropDataAsset chưa được gán trong Inspector của PlayerInteraction!");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeTool("Hand", null);

        // ... (Logic tương tác chuột và Space) ...
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int clickedCellPos = interactableMap.WorldToCell(mouseWorldPos);
            if (tileManager.IsInteractable(clickedCellPos)) HandleInteraction(clickedCellPos);
            else Debug.Log("Không thể tương tác với ô này (chuột).");
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3Int playerCellPos = interactableMap.WorldToCell(transform.position);
            if (tileManager.IsInteractable(playerCellPos)) HandleInteraction(playerCellPos);
            else Debug.Log("Không thể tương tác với ô này (Space).");
        }
    }

    void HandleInteraction(Vector3Int cellPosition)
    {
        TileState currentTileState = tileManager.GetTileState(cellPosition);
        Debug.Log($"Tương tác ô {cellPosition} với công cụ {currentTool}. Trạng thái ô: {currentTileState}");

        if (currentTool == "Shovel")
        {
            if (currentTileState == TileState.Undug)
            {
                Debug.Log("Đào đất tại: " + cellPosition);
                tileManager.SetTileState(cellPosition, TileState.Dug);
            }
            else
            {
                Debug.Log("Đất này không thể đào trong trạng thái hiện tại (" + currentTileState + ").");
            }
        }
        else if (currentTool == "Seeds")
        {
            if (currentTileState == TileState.Dug) // Vẫn chỉ trồng trên đất đã đào
            {
                if (selectedCropData != null)
                {
                    if (selectedCropData.cropPrefab == null)
                    {
                        Debug.LogError($"CropData '{selectedCropData.cropName}' thiếu Prefab cây! Không thể trồng.");
                        return;
                    }
                    Debug.Log("Trồng cây " + selectedCropData.cropName + " tại: " + cellPosition);

                    Vector3 worldPos = interactableMap.GetCellCenterWorld(cellPosition);
                    GameObject newPlant = Instantiate(selectedCropData.cropPrefab, worldPos, Quaternion.identity);
                    newPlant.name = selectedCropData.cropName + " Plant";

                    CropGrower cropGrowerScript = newPlant.GetComponent<CropGrower>();
                    if (cropGrowerScript != null)
                    {
                        cropGrowerScript.SetTilePosition(cellPosition); // Gán vị trí ô cho CropGrower
                    }
                    else
                    {
                        Debug.LogError($"Prefab '{selectedCropData.cropPrefab.name}' thiếu component CropGrower!");
                        Destroy(newPlant);
                        return;
                    }
                    tileManager.RegisterPlant(cellPosition, newPlant);
                    tileManager.SetTileState(cellPosition, TileState.Planted);
                }
                else
                {
                    Debug.Log("Chưa chọn loại cây giống để trồng.");
                }
            }
            else
            {
                Debug.Log("Bạn cần đào đất trước khi trồng cây. (Hiện tại: " + currentTileState + ")");
            }
        }
        else if (currentTool == "Hand") // Logic thu hoạch
        {
            // Luôn cố gắng lấy cây tại vị trí ô đó, bất kể trạng thái TileState
            // Vì có thể TileState chưa kịp cập nhật hoặc có lỗi đồng bộ nhỏ
            GameObject plantAtCell = tileManager.GetPlantAt(cellPosition);

            if (plantAtCell != null) // Nếu có cây tồn tại ở ô này
            {
                CropGrower cropGrower = plantAtCell.GetComponent<CropGrower>();
                if (cropGrower != null && cropGrower.isMature) // Và cây đã trưởng thành
                {
                    Debug.Log("Thu hoạch cây tại: " + cellPosition);
                    cropGrower.Harvest(); // Gọi hàm Harvest của cây

                    tileManager.DeregisterPlant(cellPosition); // Dòng này sẽ HỦY cây và xóa khỏi dictionary
                    tileManager.SetTileState(cellPosition, TileState.Dug); // Đặt lại trạng thái đất thành đã đào
                    Destroy(plantAtCell);
                }
                else
                {
                    Debug.Log("Cây chưa trưởng thành hoặc không phải cây thu hoạch được ở ô này.");
                }
            }
            else // Không có cây nào được đăng ký tại ô này
            {
                // Nếu tileManager.GetPlantAt() trả về null, có thể cây đã bị thu hoạch/hủy trước đó
                // hoặc chưa bao giờ được trồng.
                Debug.Log("Không có cây nào ở đây để thu hoạch. (Trạng thái ô: " + currentTileState + ")");

                // Thêm một kiểm tra phòng ngừa nếu trạng thái vẫn là Harvested nhưng cây đã không còn
                if (currentTileState == TileState.Harvested)
                {
                    Debug.LogWarning("Ô đất đang ở trạng thái Harvested nhưng không tìm thấy cây. Cập nhật lại trạng thái ô đất.");
                    tileManager.SetTileState(cellPosition, TileState.Dug); // Đặt lại về Dug
                }
            }
        }
    }

    public void ChangeTool(string toolName, CropData crop = null)
    {
        currentTool = toolName;
        selectedCropData = crop;

        if (playerAnimator != null)
        {
            if (currentTool == "Shovel") playerAnimator.SetInteger("ToolIndex", 1);
            else if (currentTool == "Seeds") playerAnimator.SetInteger("ToolIndex", 2);
            else if (currentTool == "Hand") playerAnimator.SetInteger("ToolIndex", 3);
            else playerAnimator.SetInteger("ToolIndex", 0);
        }
        Debug.Log($"Đã đổi công cụ sang: {currentTool}" + (crop != null ? $" (Loại cây: {crop.cropName})" : ""));
    }
}