using UnityEngine;
using System.Collections;
using TMPro;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Spawn Points")]
    [Tooltip("Lista degli spawn point disponibili")]
    public Transform[] spawnPoints;
    
    [Header("Auto Spawn Points")]
    [Tooltip("Cerca automaticamente spawn point con questo tag")]
    public string spawnPointTag = "SpawnPoint";
    [Tooltip("Usa spawn point automatici se l'array è vuoto")]
    public bool useAutoSpawnPoints = true;
    
    [Header("Respawn Settings")]

    
    [Tooltip("Salute dopo il respawn")]
    public float respawnHealth = 100f;
    
    [Tooltip("Munizioni dopo il respawn")]
    public int respawnAmmo = 30;
    
    [Header("Spawn Control")]
    [Tooltip("Disabilita il giocatore durante il countdown morte")]
    public bool disablePlayerOnDeath = true;
    
    [Tooltip("Resetta la velocità del Rigidbody al respawn")]
    public bool resetVelocity = true;
    

    
    [Header("Visual Effects")]
    [Tooltip("Effetto visivo durante il respawn")]
    public GameObject respawnEffect;

    [Tooltip("Suono del respawn")]
    public AudioClip respawnSound;

    [Header("Death Camera Settings")]
    [Tooltip("GameObject contenente PlayerVisuals (mesh del player)")]
    public GameObject playerVisuals;

    [Tooltip("Camera del player (da disattivare alla morte)")]
    public Camera playerCamera;

    [Tooltip("Drone Camera da attivare quando il player muore")]
    public Camera droneCamera;
    

    

    
    [Header("Debug")]
    [Tooltip("Mostra log di debug")]
    public bool showDebugLogs = true;
    
    // Componenti
    private AudioSource audioSource;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private PlayerLook playerLook;
    private Gun playerGun;
    private AmmoDisplay ammoDisplay;
    private DeathScreenUI deathScreenUI;
    
    // Stato
    private bool isRespawning = false;
    private bool isDead = false;
    private Transform originalSpawnPoint;
    
    // solo per testing
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("Test Morte manuale attivato");
            RespawnPlayer(gameObject);
        }
    }

    private void Awake()
    {
        // Ottieni i componenti necessari
        GetRequiredComponents();
        
        // Trova DeathScreenUI
        deathScreenUI = FindObjectOfType<DeathScreenUI>();
        
        // Salva lo spawn point originale
        if (transform != null)
        {
            originalSpawnPoint = transform;
        }
    }
    
    // Ottiene i componenti necessari
    private void GetRequiredComponents()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerMovement = GetComponent<PlayerMovement>();
        playerLook = GetComponent<PlayerLook>();
        playerGun = GetComponent<Gun>();

        // Cerca AmmoDisplay nella scena
        ammoDisplay = FindObjectOfType<AmmoDisplay>();

        // Audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Auto-find PlayerVisuals se non assegnato
        if (playerVisuals == null)
        {
            Transform visualsTransform = transform.Find("PlayerVisuals");
            if (visualsTransform != null)
            {
                playerVisuals = visualsTransform.gameObject;
                if (showDebugLogs)
                    Debug.Log("[PlayerRespawn] PlayerVisuals trovato automaticamente");
            }
        }

        // Auto-find PlayerCamera se non assegnata
        if (playerCamera == null)
        {
            Transform cameraHolder = transform.Find("CameraHolder");
            if (cameraHolder != null)
            {
                playerCamera = cameraHolder.GetComponentInChildren<Camera>();
                if (playerCamera != null && showDebugLogs)
                    Debug.Log("[PlayerRespawn] PlayerCamera trovata automaticamente");
            }
        }

        // Auto-find DroneCamera se non assegnata
        if (droneCamera == null)
        {
            GameObject droneCamObj = GameObject.Find("DroneCamera");
            if (droneCamObj != null)
            {
                droneCamera = droneCamObj.GetComponent<Camera>();
                if (droneCamera != null && showDebugLogs)
                    Debug.Log("[PlayerRespawn] DroneCamera trovata automaticamente");
            }
        }

        if (showDebugLogs)
            Debug.Log("[PlayerRespawn] Componenti ottenuti");
    }
    
    public void RespawnPlayer(GameObject player)
    {
        if (isRespawning)
        {
            if (showDebugLogs)
                Debug.LogWarning("[PlayerRespawn] Respawn già in corso");
            return;
        }
        
        if (showDebugLogs)
            Debug.Log("[PlayerRespawn] Inizio respawn del giocatore");
        
        StartCoroutine(RespawnCoroutine(player));
    }
    
    private System.Collections.IEnumerator RespawnCoroutine(GameObject player)
    {
        isRespawning = true;
        isDead = true;
        
        // Disabilita controlli giocatore
        if (disablePlayerOnDeath)
        {
            DisablePlayerControls();
        }
        
        // Mostra schermata di morte (DeathScreenUI gestisce il countdown)
        ShowDeathScreen();
        
        // Resetta velocity per evitare animazioni strane
        if (resetVelocity)
        {
            ResetPlayerVelocity(player);
        }
        
        // Aspetta che DeathScreenUI finisca il countdown e chiami CompleteRespawn()
        yield break; // Esce dalla coroutine e aspetta il callback
        
        // Trova spawn point
        Transform spawnPoint = GetBestSpawnPoint();
        if (spawnPoint == null)
        {
            Debug.LogError("[PlayerRespawn] Nessuno spawn point disponibile!");
            isRespawning = false;
            isDead = false;
            yield break;
        }
        
        // Teletrasporta giocatore
        TeleportPlayerToSpawn(player, spawnPoint);
        
        // Ripristina salute
        RestorePlayerHealth();
        
        // Ripristina munizioni
        RestorePlayerAmmo();
        
        // Effetti visivi e sonori
        PlayRespawnEffects();
        
        // Riabilita controlli
        EnablePlayerControls(spawnPoint);
        
        isRespawning = false;
        isDead = false;
        
        if (showDebugLogs)
            Debug.Log($"[PlayerRespawn] Giocatore respawnato con successo a {spawnPoint.name}");
    }
    
    private System.Collections.IEnumerator CompleteRespawnCoroutine()
    {
        // Trova spawn point
        Transform spawnPoint = GetBestSpawnPoint();
        if (spawnPoint == null)
        {
            Debug.LogError("[PlayerRespawn] Nessuno spawn point disponibile!");
            isRespawning = false;
            isDead = false;
            yield break;
        }
        
        // Teletrasporta giocatore
        TeleportPlayerToSpawn(gameObject, spawnPoint);
        
        // Ripristina salute
        RestorePlayerHealth();
        
        // Ripristina munizioni
        RestorePlayerAmmo();
        
        // Effetti visivi e sonori
        PlayRespawnEffects();
        
        // Riabilita controlli
        EnablePlayerControls(spawnPoint);
        
        isRespawning = false;
        isDead = false;
        
        if (showDebugLogs)
            Debug.Log($"[PlayerRespawn] Giocatore respawnato con successo a {spawnPoint.name}");
    }

    private Transform GetBestSpawnPoint()
    {
        // Prima controlla se ci sono spawn point manuali
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // Scegli spawn point casuale
            int randomIndex = Random.Range(0, spawnPoints.Length);
            Transform selectedSpawn = spawnPoints[randomIndex];
            
            if (showDebugLogs)
                Debug.Log($"[PlayerRespawn] Spawn point manuale selezionato: {selectedSpawn.name}");
            
            return selectedSpawn;
        }
        
        // Poi controlla spawn point automatici con tag
        if (useAutoSpawnPoints && !string.IsNullOrEmpty(spawnPointTag))
        {
            GameObject[] spawnObjects = GameObject.FindGameObjectsWithTag(spawnPointTag);
            if (spawnObjects.Length > 0)
            {
                int randomIndex = Random.Range(0, spawnObjects.Length);
                Transform selectedSpawn = spawnObjects[randomIndex].transform;
                
                if (showDebugLogs)
                    Debug.Log($"[PlayerRespawn] Spawn point automatico selezionato: {selectedSpawn.name}");
                
                return selectedSpawn;
            }
        }
        
        // Fallback alla posizione originale
        if (originalSpawnPoint != null)
        {
            if (showDebugLogs)
                Debug.Log("[PlayerRespawn] Uso spawn point originale");
            return originalSpawnPoint;
        }
        
        if (showDebugLogs)
            Debug.LogWarning("[PlayerRespawn] Nessuno spawn point trovato, uso posizione corrente");
        return transform;
    }
    
    private void TeleportPlayerToSpawn(GameObject player, Transform spawnPoint)
    {
        // Resetta velocity del rigidbody
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        // Teletrasporta
        player.transform.position = spawnPoint.position;
        player.transform.rotation = spawnPoint.rotation;
    }

    private void RestorePlayerHealth()
    {
        if (playerHealth != null)
        {
            playerHealth.RestoreHealth(respawnHealth);
            
            if (showDebugLogs)
                Debug.Log($"[PlayerRespawn] Salute ripristinata a {respawnHealth}");
        }
    }
    
    private void RestorePlayerAmmo()
    {
        if (playerGun != null)
        {
            // Resetta direttamente le munizioni al massimo
            var currentAmmoField = typeof(Gun).GetField("currentAmmo", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (currentAmmoField != null)
            {
                var maxAmmoField = typeof(Gun).GetField("maxAmmo", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                
                if (maxAmmoField != null)
                {
                    int maxAmmo = (int)maxAmmoField.GetValue(playerGun);
                    currentAmmoField.SetValue(playerGun, maxAmmo);
                    
                    if (showDebugLogs)
                        Debug.Log($"[PlayerRespawn] Munizioni ripristinate a {maxAmmo}");
                }
            }
        }
        
    }
    
    private void DisablePlayerControls()
    {
        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerLook != null)
            playerLook.enabled = false;

        if (playerGun != null)
            playerGun.enabled = false;

        // Nascondi il player e cambia camera
        HidePlayerAndSwitchToDroneCamera();
    }
    
    private void EnablePlayerControls(Transform spawnPoint)
    {
        // Mostra il player e cambia camera
        ShowPlayerAndSwitchToPlayerCamera();

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (playerLook != null)
        {
            playerLook.enabled = true;
            // Resetta la rotazione della camera per matchare lo spawn point
            ResetPlayerRotation(spawnPoint);
        }

        if (playerGun != null)
            playerGun.enabled = true;
    }
    
    // Riproduce effetti visivi e sonori del respawn
    private void PlayRespawnEffects()
    {
        // Effetto visivo
        if (respawnEffect != null)
        {
            GameObject effect = Instantiate(respawnEffect, transform.position, Quaternion.identity);
            Destroy(effect, 3f); // Distruggi dopo 3 secondi
        }
        
        // Suono
        if (respawnSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(respawnSound);
        }
    }
    
    // Completa il respawn dopo che DeathScreenUI ha finito il countdown
    public void CompleteRespawn()
    {
        StartCoroutine(CompleteRespawnCoroutine());
    }
    
    // Aggiunge uno spawn point alla lista
    public void AddSpawnPoint(Transform spawnPoint)
    {
        if (spawnPoint == null) return;
        
        // Crea o espandi l'array
        if (spawnPoints == null)
        {
            spawnPoints = new Transform[1];
            spawnPoints[0] = spawnPoint;
        }
        else
        {
            System.Array.Resize(ref spawnPoints, spawnPoints.Length + 1);
            spawnPoints[spawnPoints.Length - 1] = spawnPoint;
        }
        
        if (showDebugLogs)
            Debug.Log($"[PlayerRespawn] Aggiunto spawn point: {spawnPoint.name}");
    }
    
    // Rimuovi uno spawn point dalla lista
    public void RemoveSpawnPoint(Transform spawnPoint)
    {
        if (spawnPoints == null || spawnPoint == null) return;
        
        int index = System.Array.IndexOf(spawnPoints, spawnPoint);
        if (index >= 0)
        {
            // Sposta gli elementi per rimuovere l'elemento
            for (int i = index; i < spawnPoints.Length - 1; i++)
            {
                spawnPoints[i] = spawnPoints[i + 1];
            }
            
            System.Array.Resize(ref spawnPoints, spawnPoints.Length - 1);
            
            if (showDebugLogs)
                Debug.Log($"[PlayerRespawn] Rimosso spawn point: {spawnPoint.name}");
        }
    }
    
    // Resetta la rotazione del giocatore e della camera per matchare lo spawn point
    private void ResetPlayerRotation(Transform spawnPoint)
    {
        if (playerLook != null && spawnPoint != null)
        {
            // Usa il nuovo metodo per resettare la rotazione verso la direzione dello spawn point
            playerLook.ResetRotation(spawnPoint.forward);
        }
    }
    
    // Controlla se il respawn è in corso
    public bool IsRespawning()
    {
        return isRespawning;
    }
    
    // Controlla se il giocatore è morto
    public bool IsDead()
    {
        return isDead;
    }
    
    // Mostra la schermata di morte
    private void ShowDeathScreen()
    {
        // Usa DeathScreenUI per gestire tutto
        if (deathScreenUI != null)
        {
            deathScreenUI.ShowDeathScreen();
        }
    }
    
    // Resetta la velocity del giocatore per evitare animazioni strane
    private IEnumerator ResetPlayerVelocity(GameObject player)
    {
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
        }
        
        // Resetta anche animator se presente
        Animator animator = player.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("IsDead", true);
            yield return new WaitForSeconds(0.1f); // Breve delay
            animator.SetBool("IsDead", false);
        }
    }
    
    // Nasconde il player e attiva la DroneCamera
    private void HidePlayerAndSwitchToDroneCamera()
    {
        // Nascondi PlayerVisuals
        if (playerVisuals != null)
        {
            playerVisuals.SetActive(false);
            if (showDebugLogs)
                Debug.Log("[PlayerRespawn] PlayerVisuals nascosto");
        }
        else if (showDebugLogs)
        {
            Debug.LogWarning("[PlayerRespawn] PlayerVisuals non assegnato! Assegnalo nell'Inspector.");
        }

        // Disattiva PlayerCamera
        if (playerCamera != null)
        {
            playerCamera.enabled = false;
            if (showDebugLogs)
                Debug.Log("[PlayerRespawn] PlayerCamera disattivata");
        }
        else if (showDebugLogs)
        {
            Debug.LogWarning("[PlayerRespawn] PlayerCamera non assegnata! Assegnala nell'Inspector.");
        }

        // Attiva DroneCamera
        if (droneCamera != null)
        {
            droneCamera.enabled = true;
            if (showDebugLogs)
                Debug.Log("[PlayerRespawn] DroneCamera attivata");
        }
        else if (showDebugLogs)
        {
            Debug.LogWarning("[PlayerRespawn] DroneCamera non trovata! Creala nella scena o assegnala nell'Inspector.");
        }
    }

    // Mostra il player e riattiva la PlayerCamera
    private void ShowPlayerAndSwitchToPlayerCamera()
    {
        // Mostra PlayerVisuals
        if (playerVisuals != null)
        {
            playerVisuals.SetActive(true);
            if (showDebugLogs)
                Debug.Log("[PlayerRespawn] PlayerVisuals mostrato");
        }

        // Attiva PlayerCamera
        if (playerCamera != null)
        {
            playerCamera.enabled = true;
            if (showDebugLogs)
                Debug.Log("[PlayerRespawn] PlayerCamera riattivata");
        }

        // Disattiva DroneCamera
        if (droneCamera != null)
        {
            droneCamera.enabled = false;
            if (showDebugLogs)
                Debug.Log("[PlayerRespawn] DroneCamera disattivata");
        }
    }

    // Metodi per debug
    [ContextMenu("Test Respawn")]
    public void DebugRespawn()
    {
        if (gameObject != null)
            RespawnPlayer(gameObject);
    }
    
    [ContextMenu("Log Spawn Points")]
    public void DebugLogSpawnPoints()
    {
        if (spawnPoints != null)
        {
            Debug.Log($"[PlayerRespawn] Spawn points: {spawnPoints.Length}");
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                Debug.Log($"  {i + 1}. {spawnPoints[i].name} at {spawnPoints[i].position}");
            }
        }
        else
        {
            Debug.Log("[PlayerRespawn] Nessuno spawn point configurato");
        }
    }
}