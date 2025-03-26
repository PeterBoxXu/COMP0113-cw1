using UnityEngine;

public class CameraRotClamp : MonoBehaviour
{
    public float maxRot = 60f;
    public float minRot = -60f;

    void Update()
    {
        float xRot = transform.eulerAngles.x;
        if (xRot > 180)
            xRot -= 360;
        xRot = Mathf.Clamp(xRot, minRot, maxRot);
        transform.rotation = Quaternion.Euler(xRot, transform.eulerAngles.y, transform.eulerAngles.z);
    }
}
