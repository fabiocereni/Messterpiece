using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraSprintEffect : MonoBehaviour
{
    [Header("Riferimento Player")]
    private PlayerMovement playerMovement; // assegna il Player che ha lo script PlayerMovement

    [Header("FOV")]
    public float normalFOV = 60f;    // FOV normale
    public float sprintFOV = 75f;    // FOV quando sprinti
    public float smoothTime = 0.1f;  // velocità di interpolazione

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.fieldOfView = normalFOV;

        if (playerMovement == null)
            playerMovement = GetComponentInParent<PlayerMovement>();
    }

    void Update()
    {
        if (playerMovement == null)
            return;

        // Target FOV a seconda dello stato di sprint
        float targetFOV = playerMovement.getSprinting() ? sprintFOV : normalFOV;

        // Interpolazione fluida verso il target
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, smoothTime * Time.deltaTime * 60f);
        // Moltiplichiamo per 60 per renderlo indipendente dal frame rate
    }
}
