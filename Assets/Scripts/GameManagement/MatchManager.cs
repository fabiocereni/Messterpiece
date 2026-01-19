using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    public static MatchManager Instance { get; private set; }

    [Header("Match Settings")]
    [Tooltip("Duration of the match in seconds (0 = infinite)")]
    public float matchDuration = 600f; // 10 minutes

    [Header("Player Reference")]
    [Tooltip("Drag the Player GameObject here to track player stats separately")]
    public GameObject playerObject;

    private Dictionary<GameObject, PlayerStats> statsTable = new Dictionary<GameObject, PlayerStats>();

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
        if (Instance != null && Instance != this)
        {
            Debug.LogError("[MatchManager] Multiple MatchManagers detected! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Debug.Log("[MatchManager] MatchManager initialized (scene-based)");
    }

    private void Start()
    {
        if (MatchFlowManager.Instance != null)
        {
            MatchFlowManager.Instance.OnWarmupComplete += StartMatch;
        }
        else
        {
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

    // Start the match (called after warmup completes)
    public void StartMatch()
    {
        matchActive = true;
        matchTimer = 0f;
        Debug.Log("[MatchManager] Match started!");
    }

    // End the match and trigger leaderboard
    public void EndMatch()
    {
        if (!matchActive) return;

        matchActive = false;
        Debug.Log("[MatchManager] Match ended!");

        // Print final leaderboard
        PrintLeaderboard();

        // Fire event for UI to show end screen
        OnMatchEnd?.Invoke();

    }

    // Register a new entity (player or bot) for stats tracking
    // Call this when spawning enemies or at scene start for player
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

        Debug.Log($"[MatchManager] Registered: {displayName} (IsPlayer: {isPlayer})");
    }

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

            Debug.Log($"[MatchManager] KILL: {killerStats.entityName} killed {victimStats.entityName} | K/D: {killerStats.kills}/{killerStats.deaths}");
        }
        else if (killer == victim)
        {
            // Suicide
            Debug.Log($"[MatchManager] SUICIDE: {victimStats.entityName} killed themselves");
        }
        else
        {
            // Environment kill (fell off map, etc.)
            Debug.Log($"[MatchManager] ENVIRONMENT: {victimStats.entityName} died to environment");
        }

        // Fire event for UI updates, achievements, etc.
        OnKillRegistered?.Invoke(killerStats, victimStats);
    }

    // Get stats for a specific entity
    public PlayerStats GetStats(GameObject entity)
    {
        if (statsTable.ContainsKey(entity))
            return statsTable[entity];

        Debug.LogWarning($"[MatchManager] No stats found for {entity.name}");
        return null;
    }

    // Get all stats sorted by score (descending)
    public List<PlayerStats> GetLeaderboard()
    {
        return statsTable.Values
            .OrderByDescending(stat => stat.GetScore())
            .ToList();
    }

    // Get player stats specifically
    public PlayerStats GetPlayerStats()
    {
        if (playerObject != null && statsTable.ContainsKey(playerObject))
            return statsTable[playerObject];

        Debug.LogWarning("[MatchManager] Player not registered or found!");
        return null;
    }

    // Print leaderboard to console (for debugging)
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

    public void ResetStats()
    {
        foreach (var stats in statsTable.Values)
        {
            stats.kills = 0;
            stats.deaths = 0;
        }

        matchTimer = 0f;
        Debug.Log("[MatchManager] Stats reset");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            Debug.Log("[MatchManager] MatchManager destroyed (scene unloaded)");
        }
    }

    [ContextMenu("End Match Now")]
    public void DebugEndMatch()
    {
        EndMatch();
    }
}
