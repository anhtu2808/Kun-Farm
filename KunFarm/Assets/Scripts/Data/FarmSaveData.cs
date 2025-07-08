using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FarmSaveData
{
    public int userId;
    public List<TileStateData> tileStates = new List<TileStateData>();
    public List<PlantData> plants = new List<PlantData>();
    
    // Thêm chicken và egg data - sử dụng JSON để compatibility với API
    public string chickensStateJson = "[]";
    public string eggsStateJson = "[]";

    public static FarmSaveData FromTileManager(TileManager tileManager, int userId)
    {
        var farmData = new FarmSaveData { userId = userId };
        
        // Collect tile states (only save tiles that are not in default state)
        if (tileManager != null)
        {
            var allTileStates = tileManager.GetAllTileStates();
            foreach (var kvp in allTileStates)
            {
                // Only save tiles that have been modified (not Undug)
                if (kvp.Value != TileState.Undug)
                {
                    farmData.tileStates.Add(new TileStateData
                    {
                        x = kvp.Key.x,
                        y = kvp.Key.y,
                        z = kvp.Key.z,
                        state = (int)kvp.Value
                    });
                }
            }

            // Collect plant data
            var allPlants = tileManager.GetAllPlants(); // We'll need to add this method
            foreach (var kvp in allPlants)
            {
                var plant = kvp.Value;
                var cropGrower = plant.GetComponent<CropGrower>();
                if (cropGrower != null && cropGrower.cropData != null && !string.IsNullOrEmpty(cropGrower.cropData.name))
                {
                    farmData.plants.Add(new PlantData
                    {
                        x = kvp.Key.x,
                        y = kvp.Key.y,
                        z = kvp.Key.z,
                        cropType = cropGrower.cropData.name, // Use asset name
                        currentStage = cropGrower.GetCurrentStage(), // We'll need to add this method
                        timer = cropGrower.GetTimer(), // We'll need to add this method
                        isMature = cropGrower.isMature
                    });
                    Debug.Log($"[FarmSaveData] Saving plant: cropType='{cropGrower.cropData.name}', stage={cropGrower.GetCurrentStage()}, pos=({kvp.Key.x},{kvp.Key.y},{kvp.Key.z})");
                }
                else
                {
                    Debug.LogWarning($"[FarmSaveData] Skipping plant at ({kvp.Key.x},{kvp.Key.y},{kvp.Key.z}) - cropGrower={cropGrower != null}, cropData={cropGrower?.cropData != null}, cropName='{cropGrower?.cropData?.name}'");
                }
            }
        }

        // Collect chicken data từ ChickenManager
        if (ChickenManager.Instance != null)
        {
            try
            {
                // Lấy chicken states và serialize thành JSON
                var chickenStates = new List<ChickenSaveState>();
                var allChickens = ChickenManager.Instance.GetAllChickens();
                
                foreach (var chicken in allChickens)
                {
                    if (chicken != null && chicken.gameObject != null)
                    {
                        var chickenData = chicken.GetComponent<ChickenData>();
                        string chickenId = chickenData != null ? chickenData.chickenId : chicken.name;
                        var chickenState = ChickenManager.Instance.GetChickenState(chickenId);
                        
                        if (chickenState != null)
                        {
                            chickenStates.Add(new ChickenSaveState
                            {
                                chickenId = chickenState.chickenId,
                                positionX = chickenState.position.x,
                                positionY = chickenState.position.y,
                                positionZ = chickenState.position.z,
                                isFed = chickenState.isFed,
                                feedEndTime = chickenState.feedEndTime,
                                totalEggsLaid = chickenState.totalEggsLaid,
                                currentEggLayInterval = chickenState.currentEggLayInterval,
                                baseEggLayInterval = chickenState.baseEggLayInterval,
                                isMoving = chickenState.isMoving,
                                speedMultiplier = chickenState.speedMultiplier,
                                isBoosted = chickenState.isBoosted,
                                eggLayTimer = chicken.GetEggLayTimer()
                            });
                        }
                    }
                }
                
                farmData.chickensStateJson = JsonUtility.ToJson(new ChickenStatesWrapper { chickens = chickenStates });
                Debug.Log($"[FarmSaveData] Saving {chickenStates.Count} chickens to farm state");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[FarmSaveData] Error collecting chicken data: {e.Message}");
                farmData.chickensStateJson = "[]";
            }
        }
        else
        {
            Debug.Log("[FarmSaveData] ChickenManager not found, saving empty chicken state");
            farmData.chickensStateJson = "[]";
        }

        // Collect egg data từ scene
        try
        {
            var eggStates = new List<EggSaveState>();
            var allEggs = Object.FindObjectsOfType<Collectable>();
            
            foreach (var egg in allEggs)
            {
                if (egg != null && egg.type == CollectableType.EGG)
                {
                    eggStates.Add(new EggSaveState
                    {
                        eggId = egg.name,
                        positionX = egg.transform.position.x,
                        positionY = egg.transform.position.y,
                        positionZ = egg.transform.position.z,
                        hatchTime = egg.HatchTime,
                        hatchTimer = egg.HatchTimer
                    });
                }
            }
            
            farmData.eggsStateJson = JsonUtility.ToJson(new EggStatesWrapper { eggs = eggStates });
            Debug.Log($"[FarmSaveData] Saving {eggStates.Count} eggs to farm state");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FarmSaveData] Error collecting egg data: {e.Message}");
            farmData.eggsStateJson = "[]";
        }

        return farmData;
    }
}

[System.Serializable]
public class TileStateData
{
    public int x;
    public int y;
    public int z;
    public int state; // 0=Undug, 1=Dug, 2=Planted, 3=Harvested
}

[System.Serializable]
public class PlantData
{
    public int x;
    public int y;
    public int z;
    public string cropType;
    public int currentStage;
    public float timer;
    public bool isMature;
}

// Chicken save data structures
[System.Serializable]
public class ChickenSaveState
{
    public string chickenId;
    public float positionX;
    public float positionY;
    public float positionZ;
    public bool isFed;
    public float feedEndTime;
    public int totalEggsLaid;
    public float currentEggLayInterval;
    public float baseEggLayInterval;
    public bool isMoving;
    public float speedMultiplier;
    public bool isBoosted;
    public float eggLayTimer;
}

[System.Serializable]
public class ChickenStatesWrapper
{
    public List<ChickenSaveState> chickens;
}

// Egg save data structures - only save what actually exists
[System.Serializable]
public class EggSaveState
{
    public string eggId;
    public float positionX;
    public float positionY;
    public float positionZ;
    public float hatchTime;
    public float hatchTimer;
}

[System.Serializable]
public class EggStatesWrapper
{
    public List<EggSaveState> eggs;
} 