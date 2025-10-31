using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimento")]
    public float speed = 6.0f;

    [Header("Visuale (Mouse Look)")]
    [Tooltip("La camera che è FIGLIA del giocatore (di solito 'Main Camera')")]
    public Transform playerCamera;

    [Tooltip("Sensibilità del mouse")]
    public float mouseSensitivity = 100f;

    // Variabile privata per memorizzare la rotazione su/giù (pitch)
    private float xRotation = 0f;

    void Start()
    {
        // Questo è importante: blocca il cursore al centro
        // dello schermo e lo nasconde.
        // Premi 'Esc' per sbloccarlo mentre provi il gioco.
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // --- MOVIMENTO (Tuo codice originale) ---
        // Input da tastiera (W, A, S, D)
        float HorizontalInput = Input.GetAxis("Horizontal");
        float VerticalInput = Input.GetAxis("Vertical");

        // Calcola il vettore di movimento
        Vector3 movement = new Vector3(HorizontalInput, 0.0f, VerticalInput);

        // Applica il movimento relativo alla direzione in cui il giocatore sta guardando
        // (Space.Self è la chiave qui)
        transform.Translate(movement * Time.deltaTime * speed, Space.Self);


        // --- VISUALE (Nuovo codice per il Mouse) ---

        // Input dal mouse
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // --- Rotazione Sinistra/Destra (Yaw) ---
        // Ruotiamo l'INTERO corpo del giocatore (questo transform)
        // sull'asse Y (verticale)
        transform.Rotate(Vector3.up * mouseX);

        // --- Rotazione Su/Giù (Pitch) ---
        // Sottraiamo mouseY perché di default è invertito
        xRotation -= mouseY;

        // Blocchiamo la rotazione verticale per evitare che il giocatore
        // faccia "giri della morte" (clamp tra -90 e 90 gradi)
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Applichiamo la rotazione SU/GIÙ solo alla CAMERA,
        // non all'intero corpo del giocatore.
        if (playerCamera != null)
        {
            // Usiamo localRotation per ruotare la camera rispetto al genitore (il player)
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
        else
        {
            Debug.LogWarning("PlayerMovement: Camera non assegnata!");
        }
    }
}
