using UnityEngine;

public class RespawnPlayer : MonoBehaviour
{
    [SerializeField] private Transform respawnPoint; 
    void OnTriggerEnter(Collider other)                                                                         
    {                                                                                                           
        if (other.CompareTag("Player"))                                                                         
        {                                                                                                       
            other.transform.position = respawnPoint.position;                                                   
        }                                                                                                       
    } 
}
