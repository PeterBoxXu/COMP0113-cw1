using UnityEngine;

public class DoorOpenController : MonoBehaviour
{
    public Animation DoorOpenAnimation;
    public Animation LightGreenAnimation;

    public void OnDoorOpen()
    {
        Debug.Log("Door Opened");
        DoorOpenAnimation.Play();
        LightGreenAnimation.Play();
    }
}
