using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The main leaderboard panel (parent of all UI elements)")]
    public GameObject leaderboardPanel;

    [Tooltip("Container where LeaderboardRow prefabs will be spawned (usually inside a Scroll View)")]
    public Transform rowContainer;

    [Tooltip("Prefab for a single leaderboard row")]
    public GameObject leaderboardRowPrefab;

    [Header("Settings")]
    [Tooltip("Should the leaderboard update in real-time while visible?")]
    public bool updateInRealTime = true;

    [Tooltip("Update interval in seconds (if real-time enabled)")]
    public float updateInterval = 1f;

    [Header("Input")]
    [Tooltip("Key to hold for showing leaderboard")]
    public KeyCode leaderboardKey = KeyCode.Tab;

    private List<LeaderboardRow> spawnedRows = new List<LeaderboardRow>();
    private float updateTimer = 0f;
    private bool isVisible = false;

    private void Start()
    {
        Debug.Log("[LeaderboardUI] 🎬 LeaderboardUI Start() called");

        if (leaderboardPanel == null)
        {
            Debug.LogError("[LeaderboardUI] Leaderboard Panel reference is missing! Assign it in the Inspector.");
        }
        else
        {
            Debug.Log($"[LeaderboardUI] Leaderboard Panel found: {leaderboardPanel.name}");
            // Hide leaderboard at start
            leaderboardPanel.SetActive(false);
            Debug.Log("[LeaderboardUI] Leaderboard hidden at start");
        }

        if (rowContainer == null)
        {
            Debug.LogError("[LeaderboardUI] Row Container reference is missing! Assign the 'Content' GameObject from the Scroll View.");
        }
        else
        {
            Debug.Log($"[LeaderboardUI] Row Container found: {rowContainer.name}");
        }

        if (leaderboardRowPrefab == null)
        {
            Debug.LogError("[LeaderboardUI] Leaderboard Row Prefab reference is missing! Assign the prefab in the Inspector.");
        }
        else
        {
            Debug.Log($"[LeaderboardUI] Leaderboard Row Prefab found: {leaderboardRowPrefab.name}");
        }

        Debug.Log($"[LeaderboardUI] Leaderboard Key configured: {leaderboardKey}");
    }

    private void Update()
    {
        // Toggle leaderboard visibility with TAB (hold to show, release to hide)
        if (Input.GetKeyDown(leaderboardKey))
        {
            ShowLeaderboard();
        }
        else if (Input.GetKeyUp(leaderboardKey))
        {
            HideLeaderboard();
        }

        // Real-time updates while visible
        if (isVisible && updateInRealTime)
        {
            updateTimer += Time.deltaTime;
            if (updateTimer >= updateInterval)
            {
                RefreshLeaderboard();
                updateTimer = 0f;
            }
        }
    }

    // Show the leaderboard panel and populate with current data
    public void ShowLeaderboard()
    {
        Debug.Log("[LeaderboardUI] ShowLeaderboard() called");

        if (leaderboardPanel == null)
        {
            Debug.LogError("[LeaderboardUI] Cannot show leaderboard - Panel reference is null!");
            return;
        }

        Debug.Log($"[LeaderboardUI] Activating panel: {leaderboardPanel.name}");
        leaderboardPanel.SetActive(true);
        isVisible = true;

        Debug.Log("[LeaderboardUI] Leaderboard panel activated! IsVisible: " + isVisible);

        // Populate with latest data
        RefreshLeaderboard();
    }

    public void HideLeaderboard()
    {
        Debug.Log("[LeaderboardUI] HideLeaderboard() called");

        if (leaderboardPanel == null)
        {
            Debug.LogError("[LeaderboardUI] Cannot hide leaderboard - Panel reference is null!");
            return;
        }

        leaderboardPanel.SetActive(false);
        isVisible = false;

        Debug.Log("[LeaderboardUI] Leaderboard panel hidden! IsVisible: " + isVisible);
    }

    public void ToggleLeaderboard()
    {
        if (isVisible)
            HideLeaderboard();
        else
            ShowLeaderboard();
    }

    public void RefreshLeaderboard()
    {
        Debug.Log("[LeaderboardUI] RefreshLeaderboard() called");

        // Check if MatchManager exists
        if (MatchManager.Instance == null)
        {
            Debug.LogWarning("[LeaderboardUI] MatchManager not found! Cannot refresh leaderboard.");
            return;
        }

        Debug.Log("[LeaderboardUI] MatchManager found");

        // Get sorted leaderboard data
        List<PlayerStats> leaderboard = MatchManager.Instance.GetLeaderboard();

        Debug.Log($"[LeaderboardUI] Retrieved {leaderboard.Count} entries from MatchManager");

        // Clear old rows
        ClearRows();

        // Spawn new rows
        int rank = 1;
        foreach (PlayerStats stats in leaderboard)
        {
            Debug.Log($"[LeaderboardUI] Spawning row #{rank}: {stats.entityName} ({stats.kills}/{stats.deaths})");
            SpawnRow(rank, stats);
            rank++;
        }

        Debug.Log($"[LeaderboardUI] Leaderboard refreshed with {rank - 1} rows");
    }

    private void SpawnRow(int rank, PlayerStats stats)
    {
        if (leaderboardRowPrefab == null || rowContainer == null)
            return;

        // Instantiate row prefab
        GameObject rowObj = Instantiate(leaderboardRowPrefab, rowContainer);
        LeaderboardRow row = rowObj.GetComponent<LeaderboardRow>();

        if (row != null)
        {
            // Populate row with data
            row.SetData(rank, stats);
            spawnedRows.Add(row);
        }
        else
        {
            Debug.LogError("[LeaderboardUI] LeaderboardRow component not found on prefab!");
        }
    }

    // Clear all spawned rows
    private void ClearRows()
    {
        foreach (LeaderboardRow row in spawnedRows)
        {
            if (row != null)
                Destroy(row.gameObject);
        }
        spawnedRows.Clear();
    }

    public void ForceUpdate()
    {
        if (isVisible)
        {
            RefreshLeaderboard();
        }
    }

    private void OnDestroy()
    {
        ClearRows();
    }

    private void OnEnable()
    {
        if (MatchManager.Instance != null)
        {
            MatchManager.Instance.OnKillRegistered += OnKillHappened;
        }
    }

    private void OnDisable()
    {
        if (MatchManager.Instance != null)
        {
            MatchManager.Instance.OnKillRegistered -= OnKillHappened;
        }
    }

    private void OnKillHappened(PlayerStats killer, PlayerStats victim)
    {
        if (isVisible)
        {
            RefreshLeaderboard();
        }
    }
}
