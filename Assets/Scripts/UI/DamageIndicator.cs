using UnityEngine;
using UnityEngine.UI;

public class DamageIndicator : MonoBehaviour
{
    public Vector3 DamageLocation;
    public Transform PlayerObject;
    public Transform DamageImagePivot;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = (DamageLocation - PlayerObject.position).normalized;
        float angle = (Vector3.SignedAngle(dir, PlayerObject.forward, Vector3.up));
        DamageImagePivot.transform.localEulerAngles = new Vector3(0, 0, angle);
    }
}
