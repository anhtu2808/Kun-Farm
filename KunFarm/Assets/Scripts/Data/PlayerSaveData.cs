using UnityEngine;

[System.Serializable]
public class PlayerSaveData
{
    public int userId; // Unique identifier for the player - must be positive
    public int money;
    public float posX;
    public float posY;
    public float posZ;

    public static PlayerSaveData FromPlayer(GameObject player, int money, int userId)
    {
        Vector3 pos = player.transform.position;
        return new PlayerSaveData
        {
            userId = userId,
            money = money,
            posX = pos.x,
            posY = pos.y,
            posZ = pos.z
        };
    }
}
