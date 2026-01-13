using UnityEngine;

/// <summary>
/// Data structure to hold kill/death statistics for a player or enemy.
/// Used by MatchManager to track leaderboard data.
/// </summary>
[System.Serializable]
public class PlayerStats
{
    public string entityName;           // Display name (e.g., "Player", "Bot_01")
    public GameObject entityObject;     // Reference to the GameObject
    public int kills;                   // Total kills by this entity
    public int deaths;                  // Total deaths of this entity
    public bool isPlayer;               // True if human player, false if bot

    // Constructor
    public PlayerStats(string name, GameObject obj, bool player = false)
    {
        entityName = name;
        entityObject = obj;
        kills = 0;
        deaths = 0;
        isPlayer = player;
    }

    // K/D Ratio (avoid division by zero)
    public float GetKDRatio()
    {
        if (deaths == 0)
            return kills; // If no deaths, K/D = kills
        return (float)kills / deaths;
    }

    // Score calculation (example: 100 points per kill, -50 per death)
    public int GetScore()
    {
        return (kills * 100) - (deaths * 50);
    }

    // String representation for debugging
    public override string ToString()
    {
        return $"{entityName}: {kills} kills / {deaths} deaths (K/D: {GetKDRatio():F2})";
    }
}
