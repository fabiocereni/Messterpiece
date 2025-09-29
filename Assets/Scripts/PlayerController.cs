using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    private float mouseSensitivity = 20f;
    private CharacterController controller;
    private Camera cam;
    private float xRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
{
    // Movimento (WASD) --> ad ogni frame se premo un taso mi muovo di 1 float
    float h = 0f;
    float v = 0f;
    if (Keyboard.current.aKey.isPressed) h = -1f;
    if (Keyboard.current.dKey.isPressed) h = 1f;
    if (Keyboard.current.wKey.isPressed) v = 1f;
    if (Keyboard.current.sKey.isPressed) v = -1f;

    // calcolo il vettore per il movimento con la sua velocità
    Vector3 move = transform.right * h + transform.forward * v;
    controller.SimpleMove(move * speed);

    // calcolo la rotazione del mouse
    float mouseX = Mouse.current.delta.x.ReadValue() * mouseSensitivity * Time.deltaTime;
    float mouseY = Mouse.current.delta.y.ReadValue() * mouseSensitivity * Time.deltaTime;
    transform.Rotate(Vector3.up * mouseX);

    // aggiorno la rotazione verticale della camera in base al movimento del mouse
    // mouseY viene dal Mouse.current.delta.y.ReadValue() --> cioè quanto si muove il mouse sull’asse verticale
    // -= serve a invertire il movimento --> altrimenti muovendo il mouse in alto la camera guarderebbe in basso, direi che è sbagliato
    xRotation -= mouseY;
    // impedisce che la camera possa girare oltre un certo angolo --> il range è da -80° a +80°
    // NOTA: senza clamp il giocatore potrebbe ruotare la testa di 360° all’indietro
    xRotation = Mathf.Clamp(xRotation, -80f, 80f);
    // applica la nuova inclinazione alla camera --> così il giocatore guarda in alto o in basso
    cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
}

}
