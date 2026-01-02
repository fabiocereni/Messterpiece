using UnityEngine;

public class GroundCheckVisualizer : MonoBehaviour
{
    public PlayerMovement playerMovement;

    void Update()
    {
        if (playerMovement != null)
        {
            float height = playerMovement.playerHeight;
            Vector3 start = transform.position;
            Debug.DrawRay(start, Vector3.down * (height * 0.5f + 0.3f), Color.green);
      }
  }
}