using UnityEngine;

public class Move : MonoBehaviour
{
    [Header("Impostazioni Movimento")]
    public float moveSpeed = 5f; // Velocità di movimento

    void Update()
    {
        // --- 1. Leggi l'input (WASD o Frecce) ---
        float horizontalInput = Input.GetAxis("Horizontal"); // A/D o Freccia Sinistra/Destra
        float verticalInput = Input.GetAxis("Vertical");   // W/S o Freccia Su/Giù

        // --- 2. Calcola la direzione ---
        // Vector3.right è (1, 0, 0)
        // Vector3.forward è (0, 0, 1)
        Vector3 moveDirection = (transform.forward * verticalInput) + (transform.right * horizontalInput);

        // --- 3. Applica il movimento ---
        // Muove il GameObject in quella direzione
        // Time.deltaTime rende il movimento fluido e indipendente dai frame rate
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }
}