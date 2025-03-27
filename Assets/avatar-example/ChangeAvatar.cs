using UnityEngine;
using Ubiq.Avatars;

public class ChangeAvatar : MonoBehaviour
{
    public AvatarManager avatarManager;
    public GameObject newAvatarPrefab;

    public void SetAvatarPrefab(GameObject avatarPrefab)
    {
        newAvatarPrefab = avatarPrefab;
    }

    public void ChangeAvatarPrefab()
    {
        avatarManager.avatarPrefab = newAvatarPrefab;
    }
}
