using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Represents a single row in the leaderboard.
/// This script is attached to the LeaderboardRow prefab.
/// Supports both TextMeshPro and Unity UI Text components.
/// </summary>
public class LeaderboardRow : MonoBehaviour
{
    [Header("TextMeshPro UI References (Optional)")]
    public TextMeshProUGUI rankTextTMP;        // "#1", "#2", etc.
    public TextMeshProUGUI playerNameTextTMP;  // "Player", "Bot_01", etc.
    public TextMeshProUGUI kdTextTMP;          // "10/2" (kills/deaths)
    public TextMeshProUGUI scoreTextTMP;       // "800"

    [Header("Unity UI Text References (Optional)")]
    public Text rankText;        // "#1", "#2", etc.
    public Text playerNameText;  // "Player", "Bot_01", etc.
    public Text kdText;          // "10/2" (kills/deaths)
    public Text scoreText;       // "800"

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

        // Set rank (try TMP first, then regular Text)
        SetText(rankTextTMP, rankText, $"#{rank}");

        // Set player name
        string playerTag = stats.isPlayer ? " [YOU]" : "";
        SetText(playerNameTextTMP, playerNameText, stats.entityName + playerTag);

        // Set K/D (kills/deaths)
        SetText(kdTextTMP, kdText, $"{stats.kills}/{stats.deaths}");

        // Set score
        SetText(scoreTextTMP, scoreText, stats.GetScore().ToString());

        // Highlight player row
        if (backgroundImage != null)
        {
            if (stats.isPlayer)
            {
                backgroundImage.color = playerHighlightColor;
            }
            else
            {
                // Transparent background for non-player rows
                backgroundImage.color = new Color(1f, 1f, 1f, 0f);
            }
        }
    }

    /// <summary>
    /// Helper method to set text on either TextMeshPro or Unity UI Text
    /// </summary>
    private void SetText(TextMeshProUGUI tmpText, Text uiText, string value)
    {
        if (tmpText != null)
        {
            tmpText.text = value;
        }
        else if (uiText != null)
        {
            uiText.text = value;
        }
        else
        {
            Debug.LogWarning($"[LeaderboardRow] Both TMP and UI Text are null! Cannot set value: '{value}'");
        }
    }

    /// <summary>
    /// Alternative: Set data with individual parameters (without PlayerStats object)
    /// </summary>
    public void SetData(int rank, string playerName, int kills, int deaths, int score, bool isPlayer = false)
    {
        SetText(rankTextTMP, rankText, $"#{rank}");

        string playerTag = isPlayer ? " [YOU]" : "";
        SetText(playerNameTextTMP, playerNameText, playerName + playerTag);

        SetText(kdTextTMP, kdText, $"{kills}/{deaths}");

        SetText(scoreTextTMP, scoreText, score.ToString());

        if (backgroundImage != null)
        {
            if (isPlayer)
            {
                backgroundImage.color = playerHighlightColor;
            }
            else
            {
                // Transparent background for non-player rows
                backgroundImage.color = new Color(1f, 1f, 1f, 0f);
            }
        }
    }
}
