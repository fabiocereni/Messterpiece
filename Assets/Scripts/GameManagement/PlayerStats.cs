using UnityEngine;

[System.Serializable]
public class PlayerStats
{
    public string entityName;
    public GameObject entityObject;
    public int kills; 
    public int deaths;
    public bool isPlayer;

    public PlayerStats(string name, GameObject obj, bool player = false)
    {
        entityName = name;
        entityObject = obj;
        kills = 0;
        deaths = 0;
        isPlayer = player;
    }

    public float GetKDRatio()
    {
        if (deaths == 0)
            return kills; // If no deaths, K/D = kills
        return (float)kills / deaths;
    }

    public int GetScore()
    {
        return (kills * 100) - (deaths * 50);
    }

    public override string ToString()
    {
        return $"{entityName}: {kills} kills / {deaths} deaths (K/D: {GetKDRatio():F2})";
    }
}
