using UnityEngine;
using TMPro;

/// <summary>
/// Represents a single row in the leaderboard.
/// This script is attached to the LeaderboardRow prefab.
/// </summary>
public class LeaderboardRow : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI rankText;        // "#1", "#2", etc.
    public TextMeshProUGUI playerNameText;  // "Player", "Bot_01", etc.
    public TextMeshProUGUI kdText;          // "10/2" (kills/deaths)
    public TextMeshProUGUI scoreText;       // "800"

    [Header("Visual Feedback")]
    public Color playerHighlightColor = new Color(1f, 0.84f, 0f, 0.3f); // Gold highlight for player
    public UnityEngine.UI.Image backgroundImage; // Optional: background image to colorize

    /// <summary>
    /// Populate this row with player stats
    /// </summary>
    public void SetData(int rank, PlayerStats stats)
    {
        if (stats == null)
        {
            Debug.LogWarning("[LeaderboardRow] Tried to set data with null stats!");
            return;
        }

        // Set rank
        if (rankText != null)
            rankText.text = $"#{rank}";

        // Set player name
        if (playerNameText != null)
        {
            string playerTag = stats.isPlayer ? " [YOU]" : "";
            playerNameText.text = stats.entityName + playerTag;
        }

        // Set K/D (kills/deaths)
        if (kdText != null)
            kdText.text = $"{stats.kills}/{stats.deaths}";

        // Set score
        if (scoreText != null)
            scoreText.text = stats.GetScore().ToString();

        // Highlight player row
        if (stats.isPlayer && backgroundImage != null)
        {
            backgroundImage.color = playerHighlightColor;
        }
    }

    /// <summary>
    /// Alternative: Set data with individual parameters (without PlayerStats object)
    /// </summary>
    public void SetData(int rank, string playerName, int kills, int deaths, int score, bool isPlayer = false)
    {
        if (rankText != null)
            rankText.text = $"#{rank}";

        if (playerNameText != null)
        {
            string playerTag = isPlayer ? " [YOU]" : "";
            playerNameText.text = playerName + playerTag;
        }

        if (kdText != null)
            kdText.text = $"{kills}/{deaths}";

        if (scoreText != null)
            scoreText.text = score.ToString();

        if (isPlayer && backgroundImage != null)
        {
            backgroundImage.color = playerHighlightColor;
        }
    }
}
