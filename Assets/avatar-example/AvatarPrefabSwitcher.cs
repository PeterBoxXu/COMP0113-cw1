using UnityEngine;
using UnityEngine.UI;
using Ubiq.Avatars; 

public class AvatarPrefabSwitcher : MonoBehaviour
{
    [Header("References")]
    public AvatarManager avatarManager;

    // 在Inspector里指定想切换到的新Prefab
    public GameObject newAvatarPrefab;

    public void SwitchAvatarPrefab()
{
    if (avatarManager != null && newAvatarPrefab != null)
    {
        avatarManager.avatarPrefab = newAvatarPrefab;
       
        avatarManager.UpdateAvatar(); 
        Debug.LogWarning("Success");
    }
    else
    {
        Debug.LogWarning("AvatarManager or newAvatarPrefab is not assigned!");
    }
}
}