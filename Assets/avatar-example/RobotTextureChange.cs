using System;
using Ubiq.Avatars;
using UnityEngine.Events;
using UnityEngine;
using Avatar = Ubiq.Avatars.Avatar;
using Ubiq.Rooms;
using Ubiq.Messaging;

public class RobotTextureChange : MonoBehaviour
{
    public RobotTexture Textures;

    [Serializable]
    public class TextureEvent : UnityEvent<Texture2D> { }
    public TextureEvent OnTextureChanged;

    private Avatar avatar;
    private string uuid;
    private RoomClient roomClient;

    private Texture2D cached; // Cache for GetTexture. Do not do anything else with this; use the uuid
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        roomClient = NetworkScene.Find(this).GetComponentInChildren<RoomClient>();
        
        avatar = GetComponent<Avatar>();
        
        roomClient.OnPeerUpdated.AddListener(RoomClient_OnPeerUpdated);
    }
    private void OnDestroy()
    {
        // Cleanup the event for new properties so it does not get called after
        if (roomClient)
        {
            roomClient.OnPeerUpdated.RemoveListener(RoomClient_OnPeerUpdated);
        }
    }
    void RoomClient_OnPeerUpdated(IPeer peer)
    {
        if (peer != avatar.Peer)
        {
            // The peer who is being updated is not our peer, so we can safely
            // ignore this event.
            return;
        }
        
        SetTexture(peer["ubiq.avatar.texture.uuid"]);
    }
     /// <summary>
    /// Try to set the Texture by reference to a Texture in the Catalogue. If the Texture is not in the
    /// catalogue then this method has no effect, as Texture2Ds cannot be streamed yet.
    /// </summary>
    public void SetTexture(Texture2D texture)
    {
        Debug.Log($" {Textures.Get(texture)}");
        SetTexture(Textures.Get(texture));
    }

    public void SetTexture(string uuid)
    {
        // Debug.Log($"{roomClient1}");
        avatar = GetComponent<Avatar>();

        if(String.IsNullOrWhiteSpace(uuid))
        {
            return;
        }

        if (this.uuid != uuid)
        {
            var texture = Textures.Get(uuid);
            this.uuid = uuid;
            this.cached = texture;
            //OnTextureChanged.Invoke(texture);
            SkinnedMeshRenderer[] allRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);

                
                // 查找特定的渲染器
            foreach (var renderer in allRenderers)
            {
                    Debug.Log($"Found renderer: {renderer.gameObject.name}");
                    
                    if (renderer.gameObject.name == "MediumMechStrikerChassis" || 
                        renderer.gameObject.name.Contains("Body") || 
                        renderer.gameObject.name.Contains("Chassis"))
                    {
                        renderer.material.SetTexture("_BaseMap", texture);
                    }
            }
            
            if(avatar.IsLocal)
            {
                roomClient.Me["ubiq.avatar.texture.uuid"] = this.uuid;
            }
        }
    }
    public Texture2D GetTexture()
    {
        return cached;
    }
}
