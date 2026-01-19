using UnityEngine;

/// <summary>
/// Target mobile per lo shooting range. Si muove avanti/indietro o su/giù.
/// Usa TutorialTarget esistente per il sistema di danno.
/// </summary>
public class MovingTarget : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private Vector3 moveDirection = Vector3.right;
    [SerializeField] private float moveDistance = 3f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private bool oscillate = true; // Altrimenti fa un loop continuo

    private Vector3 startPosition;
    private float timeElapsed = 0f;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        timeElapsed += Time.deltaTime * moveSpeed;

        if (oscillate)
        {
            // Movimento oscillatorio (ping-pong)
            float offset = Mathf.PingPong(timeElapsed, moveDistance);
            transform.position = startPosition + moveDirection.normalized * offset;
        }
        else
        {
            // Movimento circolare continuo
            float offset = (timeElapsed % moveDistance);
            transform.position = startPosition + moveDirection.normalized * offset;
        }
    }
}
