using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Questo script nasconde le mesh selezionate alla vista della camera,
/// ma continua a far proiettare loro le ombre (opzionale).
/// Utile per FPS dove non vuoi vedere la tua testa/corpo clippare nella camera.
/// </summary>
public class PlayerBodyVisibility : MonoBehaviour
{
    [Header("Visibility Settings")]
    [Tooltip("Inserisci qui le Mesh (Renderer) che vuoi nascondere al giocatore (es. Testa, Cappello, Corpo)")]
    public List<Renderer> meshesToHide = new List<Renderer>();

    [Tooltip("Se VERO, le mesh saranno completamente invisibili (niente ombre). Se FALSO, saranno invisibili ma proietteranno ombra (ShadowsOnly).")]
    public bool completelyInvisible = false;

    void Start()
    {
        UpdateVisibility();
    }

    /// <summary>
    /// Applica le impostazioni di visibilità alle mesh in lista
    /// </summary>
    public void UpdateVisibility()
    {
        foreach (Renderer meshRenderer in meshesToHide)
        {
            if (meshRenderer != null)
            {
                if (completelyInvisible)
                {
                    // Disabilita completamente il renderer (niente ombra, niente visuale)
                    meshRenderer.enabled = false;
                }
                else
                {
                    // La mesh esiste per le luci/ombre, ma è invisibile alla camera
                    meshRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                }
            }
        }
    }
}