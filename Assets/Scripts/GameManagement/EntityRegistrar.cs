using UnityEngine;
using System.Collections;

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

    [ContextMenu("Force Register All Enemies")]
    public void ForceRegisterAllEnemies()
    {
        RegisterAllEnemies();
    }
}
