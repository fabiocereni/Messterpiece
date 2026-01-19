using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerBodyVisibility : MonoBehaviour
{
    [Header("Visibility Settings")]
    [Tooltip("Inserisci qui le Mesh (Renderer) che vuoi nascondere in FPS (es. Testa, Corpo)")]
    public List<Renderer> meshesToHide = new List<Renderer>();

    [Tooltip("Se VERO, le mesh saranno completamente invisibili in FPS (niente ombre). Se FALSO, proietteranno ombra (ShadowsOnly).")]
    public bool completelyInvisibleInFPS = false;

    // Cache dello stato originale dei renderer
    private Dictionary<Renderer, ShadowCastingMode> originalShadowModes = new Dictionary<Renderer, ShadowCastingMode>();

    void Awake()
    {
        // Salva lo stato originale di ogni renderer
        foreach (Renderer meshRenderer in meshesToHide)
        {
            if (meshRenderer != null)
            {
                originalShadowModes[meshRenderer] = meshRenderer.shadowCastingMode;
            }
        }
    }

    void Start()
    {
        // default, inizia in FPS mode (nascosto)
        SetVisible(false);
    }

    public void SetVisible(bool visible)
    {
        foreach (Renderer meshRenderer in meshesToHide)
        {
            if (meshRenderer == null)
                continue;

            if (visible)
            {
                // TPS Mode: Mesh completamente visibile
                meshRenderer.enabled = true;
                // Ripristina la modalità ombra originale
                if (originalShadowModes.ContainsKey(meshRenderer))
                {
                    meshRenderer.shadowCastingMode = originalShadowModes[meshRenderer];
                }
                else
                {
                    meshRenderer.shadowCastingMode = ShadowCastingMode.On;
                }
            }
            else
            {
                // FPS Mode: Mesh nascosta
                if (completelyInvisibleInFPS)
                {
                    // Disabilita completamente il renderer (niente ombra, niente visuale)
                    meshRenderer.enabled = false;
                }
                else
                {
                    // La mesh esiste per le luci/ombre, ma è invisibile alla camera
                    meshRenderer.enabled = true;
                    meshRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                }
            }
        }
    }

    public void UpdateVisibility()
    {
        // Assume FPS mode per retrocompatibilità
        SetVisible(false);
    }
}