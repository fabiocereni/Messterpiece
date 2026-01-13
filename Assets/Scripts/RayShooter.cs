using UnityEngine;
using UnityEngine.InputSystem;

public class RayShooter : MonoBehaviour
{
    private Camera _camera;

    void Start()
    {
        _camera = GetComponent<Camera>();
    }

    void Update()
    {
        // Disable shooting during warmup
        if (MatchFlowManager.Instance != null && !MatchFlowManager.Instance.CanPlayerMove())
        {
            return;
        }

        // Creates a Ray from the center of the viewport
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 point = new Vector3(_camera.pixelWidth / 2, _camera.pixelHeight / 2, 0);
            Ray ray = _camera.ScreenPointToRay(point);

            // Show Raycast in the scene
            Debug.DrawRay(ray.origin, ray.direction * 10);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                //Debug.Log("We have a hit!");
                GameObject hitObject = hit.transform.gameObject;
                Renderer targetRenderer = hitObject.GetComponent<MeshRenderer>();
                if (targetRenderer == null)
                {
                    Debug.Log("targetRenderer = null");
                    return;
                }

                foreach (var m in targetRenderer.materials)
                {
                    m.color = Color.red;
                }

            }

        }

    }
    
}