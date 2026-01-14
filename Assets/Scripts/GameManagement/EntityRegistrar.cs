using UnityEngine;
using System.Collections;

/// <summary>
/// Automatically registers all enemies in the scene with MatchManager at the start.
/// This ensures all entities appear in the leaderboard from the beginning with 0 kills/deaths.
/// </summary>
public class EntityRegistrar : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Tag used to identify enemy GameObjects in the scene")]
    public string enemyTag = "Enemy";

    [Tooltip("Delay before registering entities (to ensure MatchManager is initialized)")]
    public float registrationDelay = 0.5f;

    [Tooltip("Prefix for auto-generated enemy names (e.g., 'Bot_01', 'Bot_02')")]
    public string enemyNamePrefix = "Bot_";

    private void Start()
    {
        StartCoroutine(RegisterAllEntitiesDelayed());
    }

    /// <summary>
    /// Wait a moment for MatchManager to initialize, then register all entities
    /// </summary>
    private IEnumerator RegisterAllEntitiesDelayed()
    {
        yield return new WaitForSeconds(registrationDelay);

        if (MatchManager.Instance == null)
        {
            Debug.LogError("[EntityRegistrar] MatchManager not found! Cannot register entities.");
            yield break;
        }

        RegisterAllEnemies();
    }

    /// <summary>
    /// Find and register all enemies in the scene
    /// </summary>
    private void RegisterAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        if (enemies.Length == 0)
        {
            Debug.LogWarning($"[EntityRegistrar] No enemies found with tag '{enemyTag}'!");
            return;
        }

        Debug.Log($"[EntityRegistrar] Found {enemies.Length} enemies. Registering...");

        for (int i = 0; i < enemies.Length; i++)
        {
            GameObject enemy = enemies[i];
            string enemyName = $"{enemyNamePrefix}{(i + 1):00}"; // "Bot_01", "Bot_02", etc.

            // Register with MatchManager
            MatchManager.Instance.RegisterEntity(enemy, enemyName, isPlayer: false);
        }

        Debug.Log($"[EntityRegistrar] Successfully registered {enemies.Length} enemies!");
    }

    /// <summary>
    /// Manually trigger entity registration (useful for debugging)
    /// </summary>
    [ContextMenu("Force Register All Enemies")]
    public void ForceRegisterAllEnemies()
    {
        RegisterAllEnemies();
    }
}
