using UnityEngine;

public class CameraRotClamp : MonoBehaviour
{
    void Update()
    {

        float xRot = transform.rotation.x;
        Debug.Log($"xRot: {xRot}, {xRot * Mathf.Rad2Deg}");
        xRot = Mathf.Clamp(xRot, -35 * Mathf.Deg2Rad, 35 * Mathf.Deg2Rad);
        transform.rotation = Quaternion.Euler(xRot, transform.rotation.y, transform.rotation.z);


        //xRot = Mathf.Clamp(xRot, )
    }
}
