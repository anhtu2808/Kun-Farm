// PlayerInteraction.cs
// Đặt trong Assets/Scripts/Player/
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerInteraction : MonoBehaviour
{
    // === CÁC BIẾN SERIALIZED FIELD (Kéo vào từ Inspector) ===
    [SerializeField] private Tilemap interactableMap; // Kéo Tilemap tương tác của bạn vào đây
    [SerializeField] private TileManager tileManager; // Kéo GameManager (chứa TileManager) vào đây
    [SerializeField] private Animator playerAnimator; // Kéo Animator của Player vào đây

    // --- BIẾN CHO TỪNG LOẠI CROP DATA CỤ THỂ ĐỂ TEST ---
    [SerializeField] private CropData grapeCropDataAsset; // Kéo GrapeCropData Asset của bạn vào trường này
    // Nếu có loại cây khác để test, thêm ở đây:
    // [SerializeField] private CropData tomatoCropDataAsset;
    // ---------------------------------------------------

    // === BIẾN NỘI BỘ ===
    public string currentTool = "Shovel"; // Công cụ hiện tại: "Shovel", "Seeds", "Hand"
    private CropData selectedCropData; // CropData của loại hạt giống đang được chọn (được gán khi đổi công cụ)

    void Update()
    {
        // === LOGIC ĐỂ ĐỔI CÔNG CỤ BẰNG PHÍM (CHO MỤC ĐÍCH TEST) ===
        // Phím 1: Xẻng
        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeTool("Shovel", null);

        // Phím 2: Hạt giống (Cây nho)
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (grapeCropDataAsset != null)
            {
                ChangeTool("Seeds", grapeCropDataAsset); // Gán CropData cụ thể cho cây nho
            }
            else
            {
                Debug.LogError("grapeCropDataAsset chưa được gán trong Inspector của PlayerInteraction! Không thể chọn hạt giống.");
                ChangeTool("Seeds", null); // Vẫn đổi công cụ nhưng không có hạt giống
            }
        }
        // Phím 3: Thu hoạch (tay)
        if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeTool("Hand", null);

        // === LOGIC TƯƠNG TÁC CHÍNH (Sử dụng chuột trái HOẶC phím Space) ===

        // 1. Tương tác bằng chuột trái
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int clickedCellPos = interactableMap.WorldToCell(mouseWorldPos);

            if (tileManager.IsInteractable(clickedCellPos))
            {
                HandleInteraction(clickedCellPos);
            }
            else
            {
                Debug.Log("Không thể tương tác với ô này (chuột).");
            }
        }

        // 2. Tương tác bằng phím Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Lấy vị trí ô mà người chơi đang đứng
            Vector3Int playerCellPos = interactableMap.WorldToCell(transform.position);
            Vector3Int interactionCellPos = playerCellPos; // Hoặc thêm logic hướng player ở đây

            if (tileManager.IsInteractable(interactionCellPos))
            {
                HandleInteraction(interactionCellPos);
            }
            else
            {
                Debug.Log("Không thể tương tác với ô này (Space).");
            }
        }
    }

    void HandleInteraction(Vector3Int cellPosition)
    {
        TileState currentTileState = tileManager.GetTileState(cellPosition);

        if (currentTool == "Shovel") // Logic khi người chơi cầm xẻng
        {
            if (currentTileState == TileState.Undug)
            {
                Debug.Log("Đào đất tại: " + cellPosition);
                tileManager.SetTileState(cellPosition, TileState.Dug);
                // TODO: Thêm animation đào của người chơi hoặc hiệu ứng âm thanh/hình ảnh
            }
            else if (currentTileState == TileState.Dug)
            {
                Debug.Log("Đất này đã đào rồi.");
            }
            else if (currentTileState == TileState.Planted)
            {
                Debug.Log("Đất đang có cây, không thể đào.");
            }
            else if (currentTileState == TileState.Harvested)
            {
                Debug.Log("Đất đang có cây đã trưởng thành, không thể đào.");
            }
        }
        else if (currentTool == "Seeds") // Logic khi người chơi cầm hạt giống
        {
            if (currentTileState == TileState.Dug)
            {
                if (selectedCropData != null)
                {
                    // === SỬ DỤNG selectedCropData.cropPrefab TRỰC TIẾP ===
                    if (selectedCropData.cropPrefab == null)
                    {
                        Debug.LogError($"CropData '{selectedCropData.cropName}' thiếu Prefab cây! Không thể trồng.");
                        return;
                    }

                    Debug.Log("Trồng cây " + selectedCropData.cropName + " tại: " + cellPosition);

                    Vector3 worldPos = interactableMap.GetCellCenterWorld(cellPosition);
                    // Instantiate Prefab cây từ CropData
                    GameObject newPlant = Instantiate(selectedCropData.cropPrefab, worldPos, Quaternion.identity);
                    newPlant.name = selectedCropData.cropName + " Plant";

                    CropGrower cropGrowerScript = newPlant.GetComponent<CropGrower>();
                    if (cropGrowerScript != null)
                    {
                        // CropGrower.Awake() trên Prefab sẽ tự động lấy CropData được gán trên Prefab.
                        // Dòng này (cropGrowerScript.cropData = selectedCropData;) không cần thiết nếu đã gán trên Prefab.
                    }
                    else
                    {
                        Debug.LogError($"Prefab '{selectedCropData.cropPrefab.name}' thiếu component CropGrower!");
                        Destroy(newPlant); // Hủy cây vừa tạo nếu không có CropGrower
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
            else if (currentTileState == TileState.Planted)
            {
                Debug.Log("Đất này đã có cây đang phát triển.");
            }
            else if (currentTileState == TileState.Harvested)
            {
                Debug.Log("Đất này đã có cây trưởng thành. Thu hoạch đi!");
            }
            else // currentTileState == TileState.Undug
            {
                Debug.Log("Bạn cần đào đất trước khi trồng cây.");
            }
        }
        else if (currentTool == "Hand") // Logic thu hoạch
        {
            if (currentTileState == TileState.Harvested)
            {
                GameObject plantToHarvest = tileManager.GetPlantAt(cellPosition);
                if (plantToHarvest != null)
                {
                    CropGrower cropGrower = plantToHarvest.GetComponent<CropGrower>();
                    if (cropGrower != null && cropGrower.isMature)
                    {
                        Debug.Log("Thu hoạch cây tại: " + cellPosition);
                        cropGrower.Harvest();

                        tileManager.DeregisterPlant(cellPosition); // Dòng này sẽ phá hủy GameObject cây thực tế
                        tileManager.SetTileState(cellPosition, TileState.Dug);
                    }
                    else
                    {
                        Debug.Log("Cây chưa trưởng thành hoặc không phải cây thu hoạch được.");
                    }
                }
                else
                {
                    Debug.Log("Không có cây nào ở đây để thu hoạch.");
                }
            }
            else
            {
                Debug.Log("Không có cây nào sẵn sàng thu hoạch ở ô này (Trạng thái: " + currentTileState + ").");
            }
        }
    }

    public void ChangeTool(string toolName, CropData crop = null)
    {
        currentTool = toolName;
        selectedCropData = crop; // Gán CropData của loại hạt giống được chọn

        if (playerAnimator != null)
        {
            if (currentTool == "Shovel")
            {
                playerAnimator.SetInteger("ToolIndex", 1);
            }
            else if (currentTool == "Seeds")
            {
                playerAnimator.SetInteger("ToolIndex", 2);
            }
            else if (currentTool == "Hand")
            {
                playerAnimator.SetInteger("ToolIndex", 3);
            }
            else
            {
                playerAnimator.SetInteger("ToolIndex", 0);
            }
        }
        Debug.Log($"Đã đổi công cụ sang: {currentTool}" + (crop != null ? $" (Loại cây: {crop.cropName})" : ""));
    }
}