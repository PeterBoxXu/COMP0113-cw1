using UnityEngine;

public class DoorOpenController : MonoBehaviour
{
    public Animation DoorOpenAnimation;
    public Animation LightGreenAnimation;

    public void OnDoorOpen()
    {
        DoorOpenAnimation.Play();
        LightGreenAnimation.Play();
    }
}
