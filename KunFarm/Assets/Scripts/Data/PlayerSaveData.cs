using UnityEngine;

[System.Serializable]
public class PlayerSaveData
{
    public int userId; // Unique identifier for the player - must be positive
    public int money;
    public float posX;
    public float posY;
    public float posZ;
    public float health;
    public float hunger;

    public static PlayerSaveData FromPlayer(GameObject player, int money, int userId)
    {
        Vector3 pos = player.transform.position;
        
        // Get health and hunger from PlayerStats
        float health = 100f; // Default values
        float hunger = 100f;
        
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            // Try to find PlayerStats in scene if not on player object
            playerStats = Object.FindObjectOfType<PlayerStats>();
        }
        
        if (playerStats != null)
        {
            health = playerStats.Health;
            hunger = playerStats.Hunger;
        }
        
        return new PlayerSaveData
        {
            userId = userId,
            money = money,
            posX = pos.x,
            posY = pos.y,
            posZ = pos.z,
            health = health,
            hunger = hunger
        };
    }
}
