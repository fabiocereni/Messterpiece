using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Launcher : MonoBehaviour
{
    public int fireRateSeconds = 5;

    // This has to be set from Inspector (or can be loaded at runtime from Resource path)
    public GameObject shot;
    public float shootSpeed = 1.0f;
    public Transform shotSpawn;
    public bool automaticShoot = false;
    public float pitchRotationSpeed = 10f;
    public float pitchMin = -45f, pitchMax = 45f;
    public bool invertPitchAxis = false;
    
    [Header("Camera Settings")]
    public CameraBehavior cameraBehavior;
    
    
    //valori modificabili direttamente dall'inspector per posizione il launcher. questi valori identificano dove si troverà il launcher rispetto allo schermo
    [Header("Camera UI Position")]
    [Range(0f, 1f)] public float screenX = 0.85f;  //"di quando farlo spostare verso destra (0 = tutto a sx / 1 = tutto a dx)
    [Range(0f, 1f)] public float screenY = 0.25f;  //"di quanto farlo spostare verso l'altro
    public float distanceFromCamera = 1.0f;         // Distanza in unità dal piano della telecamera per posizionare il launcher in profondità rispetto alla camera


    private float _cooldown = 0f;
    private float _cooldownTimer = 0f;
    private bool _canShoot = false;
    private uint _shotCounter = 0;
    private float _launcherPitch;

    // Start is called before the first frame update
    void Start()
    {
        _cooldown = 1f / fireRateSeconds;
        var localAngle = transform.localEulerAngles.x;
        // angles are unsigned values from 0 to 360 with localEulerAngles, needed for simpler clamp in Update
        // pitch is the rotation around the x axis
        _launcherPitch = localAngle > 180f ? (360f - localAngle) : localAngle;
    }


    // Update is called once per frame
    void Update()
    {
        // handle fire rate
        _cooldownTimer -= Time.deltaTime;

        _canShoot = false;
        
        // Seleziona la camera attiva in base alla modalità (prima o terza persona)
        Camera activeCamera = cameraBehavior.isFirstPerson
            ? cameraBehavior.firstPersonCamera
            : cameraBehavior.thirdPersonCamera;

// Se la camera attiva esiste, aggiorna la rotazione e la posizione del launcher
        if (activeCamera != null)
        {
            transform.rotation = activeCamera.transform.rotation * Quaternion.Euler(_launcherPitch, 0f, 0f);

            //per calcolare la posizione sullo schermo
            Vector3 screenPos = new Vector3(
                Screen.width * screenX,   // offset orizzontale normalizzato (0-1)
                Screen.height * screenY,  // offset verticale normalizzato (0-1)
                distanceFromCamera        // distanza dalla camera
            );

            //convertire da screen a world
            transform.position = cameraBehavior.firstPersonCamera.ScreenToWorldPoint(screenPos);
        }

        if (_cooldownTimer <= 0f)
        {
            _canShoot = true;
        }

        // Input has been put outside cooldown check so we can do something else if character cannot shoot, for example display something or reproduce a sound
        if (
            Input.GetMouseButtonDown(0)
            || automaticShoot && Input.GetMouseButton(0)
        )
        {
            if (shot && _canShoot)
            {
                // reset cooldown
                _cooldownTimer = _cooldown;
                // create shot instance
                Shot shotInstance = Instantiate(shot, shotSpawn.position, Quaternion.LookRotation(transform.forward, transform.up)).GetComponent<Shot>();
                // assign your values to the Shot component
                shotInstance.speed = shootSpeed;
                shotInstance.direction = transform.forward;
                shotInstance.gameObject.name = $"SHOT_{_shotCounter:D3}";
                _shotCounter++;
            }
        }
        
        var launcherPitchDelta = Input.GetAxis("Mouse ScrollWheel");
        
        _launcherPitch += launcherPitchDelta * pitchRotationSpeed * (invertPitchAxis ? -1 : 1);
        _launcherPitch = Mathf.Clamp(_launcherPitch, pitchMin, pitchMax);

        
    }
}