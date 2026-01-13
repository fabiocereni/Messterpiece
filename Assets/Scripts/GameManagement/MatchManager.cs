using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Scene-based Match Manager (NOT DontDestroyOnLoad!)
/// Handles match-specific logic: kills, deaths, leaderboard tracking.
/// Dies with the scene to ensure clean state for each new match.
/// </summary>
public class MatchManager : MonoBehaviour
{
    // Singleton pattern (within this scene only - NOT DontDestroyOnLoad)
    public static MatchManager Instance { get; private set; }

    [Header("Match Settings")]
    [Tooltip("Duration of the match in seconds (0 = infinite)")]
    public float matchDuration = 600f; // 10 minutes

    [Header("Player Reference")]
    [Tooltip("Drag the Player GameObject here to track player stats separately")]
    public GameObject playerObject;

    // Stats storage: Dictionary for fast lookup by GameObject
    private Dictionary<GameObject, PlayerStats> statsTable = new Dictionary<GameObject, PlayerStats>();

    // Events that other systems can subscribe to
    public event System.Action<PlayerStats, PlayerStats> OnKillRegistered; // (killer, victim)
    public event System.Action OnMatchEnd;

    // Match state
    private float matchTimer = 0f;
    private bool matchActive = false;

    // Properties
    public bool IsMatchActive => matchActive;
    public float RemainingTime => Mathf.Max(0, matchDuration - matchTimer);

    private void Awake()
    {
        // Singleton pattern (scene-local, NOT persistent)
        if (Instance != null && Instance != this)
        {
            Debug.LogError("[MatchManager] Multiple MatchManagers detected! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // DO NOT use DontDestroyOnLoad - we WANT this to die with the scene
        Debug.Log("[MatchManager] ✅ MatchManager initialized (scene-based)");
    }

    private void Start()
    {
        // Subscribe to warmup complete event
        if (MatchFlowManager.Instance != null)
        {
            MatchFlowManager.Instance.OnWarmupComplete += StartMatch;
        }
        else
        {
            // If no warmup, start immediately
            StartMatch();
        }

        // Register player if assigned
        if (playerObject != null)
        {
            RegisterEntity(playerObject, "Player", isPlayer: true);
        }
    }

    private void Update()
    {
        if (!matchActive) return;

        // Update match timer
        if (matchDuration > 0)
        {
            matchTimer += Time.deltaTime;

            if (matchTimer >= matchDuration)
            {
                EndMatch();
            }
        }
    }

    /// <summary>
    /// Start the match (called after warmup completes)
    /// </summary>
    public void StartMatch()
    {
        matchActive = true;
        matchTimer = 0f;
        Debug.Log("[MatchManager] 🎮 Match started!");
    }

    /// <summary>
    /// End the match and trigger leaderboard
    /// </summary>
    public void EndMatch()
    {
        if (!matchActive) return;

        matchActive = false;
        Debug.Log("[MatchManager] 🏁 Match ended!");

        // Print final leaderboard
        PrintLeaderboard();

        // Fire event for UI to show end screen
        OnMatchEnd?.Invoke();

        // TODO: Show end screen UI with stats
    }

    /// <summary>
    /// Register a new entity (player or bot) for stats tracking
    /// Call this when spawning enemies or at scene start for player
    /// </summary>
    public void RegisterEntity(GameObject entity, string displayName, bool isPlayer = false)
    {
        if (entity == null)
        {
            Debug.LogWarning("[MatchManager] Tried to register null entity!");
            return;
        }

        if (statsTable.ContainsKey(entity))
        {
            Debug.LogWarning($"[MatchManager] Entity {entity.name} already registered!");
            return;
        }

        PlayerStats stats = new PlayerStats(displayName, entity, isPlayer);
        statsTable.Add(entity, stats);

        Debug.Log($"[MatchManager] ✅ Registered: {displayName} (IsPlayer: {isPlayer})");
    }

    /// <summary>
    /// Main method: Register a kill event
    /// Call this when an entity kills another entity
    /// </summary>
    /// <param name="killer">The GameObject that performed the kill (can be null for suicide/environment)</param>
    /// <param name="victim">The GameObject that died</param>
    public void RegisterKill(GameObject killer, GameObject victim)
    {
        if (victim == null)
        {
            Debug.LogWarning("[MatchManager] RegisterKill called with null victim!");
            return;
        }

        // Get or create victim stats
        if (!statsTable.ContainsKey(victim))
        {
            // Auto-register victim if not already registered
            RegisterEntity(victim, victim.name, isPlayer: false);
        }

        PlayerStats victimStats = statsTable[victim];
        victimStats.deaths++;

        PlayerStats killerStats = null;

        // Handle killer stats (if killer exists and isn't the victim)
        if (killer != null && killer != victim)
        {
            // Get or create killer stats
            if (!statsTable.ContainsKey(killer))
            {
                // Auto-register killer if not already registered
                RegisterEntity(killer, killer.name, isPlayer: false);
            }

            killerStats = statsTable[killer];
            killerStats.kills++;

            Debug.Log($"[MatchManager] 💀 KILL: {killerStats.entityName} killed {victimStats.entityName} | K/D: {killerStats.kills}/{killerStats.deaths}");
        }
        else if (killer == victim)
        {
            // Suicide
            Debug.Log($"[MatchManager] 💀 SUICIDE: {victimStats.entityName} killed themselves");
        }
        else
        {
            // Environment kill (fell off map, etc.)
            Debug.Log($"[MatchManager] 💀 ENVIRONMENT: {victimStats.entityName} died to environment");
        }

        // Fire event for UI updates, achievements, etc.
        OnKillRegistered?.Invoke(killerStats, victimStats);
    }

    /// <summary>
    /// Get stats for a specific entity
    /// </summary>
    public PlayerStats GetStats(GameObject entity)
    {
        if (statsTable.ContainsKey(entity))
            return statsTable[entity];

        Debug.LogWarning($"[MatchManager] No stats found for {entity.name}");
        return null;
    }

    /// <summary>
    /// Get all stats sorted by score (descending)
    /// </summary>
    public List<PlayerStats> GetLeaderboard()
    {
        return statsTable.Values
            .OrderByDescending(stat => stat.GetScore())
            .ToList();
    }

    /// <summary>
    /// Get player stats specifically
    /// </summary>
    public PlayerStats GetPlayerStats()
    {
        if (playerObject != null && statsTable.ContainsKey(playerObject))
            return statsTable[playerObject];

        Debug.LogWarning("[MatchManager] Player not registered or found!");
        return null;
    }

    /// <summary>
    /// Print leaderboard to console (for debugging)
    /// </summary>
    public void PrintLeaderboard()
    {
        Debug.Log("═══════════════════════════════════════");
        Debug.Log("          MATCH LEADERBOARD");
        Debug.Log("═══════════════════════════════════════");

        List<PlayerStats> leaderboard = GetLeaderboard();

        int rank = 1;
        foreach (PlayerStats stats in leaderboard)
        {
            string playerTag = stats.isPlayer ? " [PLAYER]" : "";
            Debug.Log($"#{rank}: {stats.entityName}{playerTag} | Score: {stats.GetScore()} | K/D: {stats.kills}/{stats.deaths} ({stats.GetKDRatio():F2})");
            rank++;
        }

        Debug.Log("═══════════════════════════════════════");
    }

    /// <summary>
    /// Reset all stats (useful for restarting match without reloading scene)
    /// </summary>
    public void ResetStats()
    {
        foreach (var stats in statsTable.Values)
        {
            stats.kills = 0;
            stats.deaths = 0;
        }

        matchTimer = 0f;
        Debug.Log("[MatchManager] 🔄 Stats reset");
    }

    private void OnDestroy()
    {
        // Cleanup singleton reference when scene unloads
        if (Instance == this)
        {
            Instance = null;
            Debug.Log("[MatchManager] 🗑️ MatchManager destroyed (scene unloaded)");
        }
    }

    // Debug method to manually trigger match end (for testing)
    [ContextMenu("End Match Now")]
    public void DebugEndMatch()
    {
        EndMatch();
    }
}
