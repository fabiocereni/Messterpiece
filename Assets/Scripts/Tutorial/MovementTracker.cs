using UnityEngine;

/// <summary>
/// Traccia il movimento del player (WASD) per completare la sezione Movement.
/// Completa quando il player si è mosso in tutte e 4 le direzioni.
/// </summary>
public class MovementTracker : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float minMoveDistance = 2f; // Distanza minima da percorrere per ogni direzione

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    // Tracking direzioni
    private Vector3 startPosition;
    private bool movedForward = false;
    private bool movedBackward = false;
    private bool movedLeft = false;
    private bool movedRight = false;

    private bool sectionCompleted = false;

    // Tracking distanze
    private float maxForwardDistance = 0f;
    private float maxBackwardDistance = 0f;
    private float maxLeftDistance = 0f;
    private float maxRightDistance = 0f;

    void Start()
    {
        startPosition = transform.position;
        Debug.Log("[MovementTracker] Tracciamento movimento iniziato");
    }

    void Update()
    {
        if (sectionCompleted) return;

        TrackMovement();
        CheckCompletion();
    }

    void TrackMovement()
    {
        Vector3 currentPosition = transform.position;
        Vector3 displacement = currentPosition - startPosition;

        // Proietta lo spostamento sugli assi locali del player
        Transform cam = Camera.main.transform;
        Vector3 forward = cam.forward;
        Vector3 right = cam.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // Calcola distanze nelle 4 direzioni
        float forwardDist = Vector3.Dot(displacement, forward);
        float rightDist = Vector3.Dot(displacement, right);

        // Aggiorna massime distanze
        if (forwardDist > maxForwardDistance)
        {
            maxForwardDistance = forwardDist;
            if (maxForwardDistance >= minMoveDistance && !movedForward)
            {
                movedForward = true;
                Debug.Log("[MovementTracker] Movimento AVANTI completato");
            }
        }

        if (-forwardDist > maxBackwardDistance)
        {
            maxBackwardDistance = -forwardDist;
            if (maxBackwardDistance >= minMoveDistance && !movedBackward)
            {
                movedBackward = true;
                Debug.Log("[MovementTracker] Movimento INDIETRO completato");
            }
        }

        if (rightDist > maxRightDistance)
        {
            maxRightDistance = rightDist;
            if (maxRightDistance >= minMoveDistance && !movedRight)
            {
                movedRight = true;
                Debug.Log("[MovementTracker] Movimento DESTRA completato");
            }
        }

        if (-rightDist > maxLeftDistance)
        {
            maxLeftDistance = -rightDist;
            if (maxLeftDistance >= minMoveDistance && !movedLeft)
            {
                movedLeft = true;
                Debug.Log("[MovementTracker] Movimento SINISTRA completato");
            }
        }
    }

    void CheckCompletion()
    {
        if (movedForward && movedBackward && movedLeft && movedRight)
        {
            CompleteSection();
        }
    }

    void CompleteSection()
    {
        if (sectionCompleted) return;

        sectionCompleted = true;
        Debug.Log("[MovementTracker] Sezione Movement completata!");

        // Notifica il TutorialManager
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.CompleteSection("Movement");
        }
    }

    void OnGUI()
    {
        if (!showDebugInfo || sectionCompleted) return;

        GUILayout.BeginArea(new Rect(10, 100, 300, 150));
        GUILayout.Label("=== MOVEMENT TRACKER ===");
        GUILayout.Label($"Forward: {(movedForward ? "✓" : "✗")} ({maxForwardDistance:F1}m / {minMoveDistance}m)");
        GUILayout.Label($"Backward: {(movedBackward ? "✓" : "✗")} ({maxBackwardDistance:F1}m / {minMoveDistance}m)");
        GUILayout.Label($"Left: {(movedLeft ? "✓" : "✗")} ({maxLeftDistance:F1}m / {minMoveDistance}m)");
        GUILayout.Label($"Right: {(movedRight ? "✓" : "✗")} ({maxRightDistance:F1}m / {minMoveDistance}m)");
        GUILayout.EndArea();
    }
}
