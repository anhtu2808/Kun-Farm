using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FarmSaveData
{
    public int userId;
    public List<TileStateData> tileStates = new List<TileStateData>();
    public List<PlantData> plants = new List<PlantData>();

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