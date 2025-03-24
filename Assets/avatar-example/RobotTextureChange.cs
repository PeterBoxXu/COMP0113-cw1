using System;
using Ubiq.Avatars;
using UnityEngine.Events;
using UnityEngine;
using Avatar = Ubiq.Avatars.Avatar;
using Ubiq.Rooms;
using Ubiq.Messaging;

public class RobotTextureChange : MonoBehaviour
{
    public RobotMaterial Materials;

    [Serializable]
    public class MaterialEvent : UnityEvent<Material> { }
    public MaterialEvent OnMaterialChanged;

    private Avatar avatar;
    private string uuid;
    private RoomClient roomClient;

    private Material cached; 

    private void Start()
    {
        roomClient = NetworkScene.Find(this).GetComponentInChildren<RoomClient>();
        avatar = GetComponent<Avatar>();
        
        roomClient.OnPeerUpdated.AddListener(RoomClient_OnPeerUpdated);
        // OnMaterialChanged.AddListener(ApplyMaterialToMeshes);
    }

    private void OnDestroy()
    {
        if (roomClient)
        {
            roomClient.OnPeerUpdated.RemoveListener(RoomClient_OnPeerUpdated);
        }
    }

    void RoomClient_OnPeerUpdated(IPeer peer)
    {
        if (peer != avatar.Peer)
        {
            // 更新的 peer 不是我们的 peer，忽略该事件
            return;
        }
        
        SetMaterial(peer["ubiq.avatar.material.uuid"]);
    }

    /// <summary>
    /// 通过传入 Material 对象尝试设置材质。如果该材质不在目录中，则不做任何处理，
    /// </summary>
    public void SetMaterial(Material material)
    {
        Debug.Log($"{Materials.Get(material)}");
        SetMaterial(Materials.Get(material));
    }

    public void SetMaterial(string uuid)
    {
        avatar = GetComponent<Avatar>();
        Debug.Log($"avatar{avatar}");
        roomClient = NetworkScene.Find(this).GetComponentInChildren<RoomClient>();

        if (string.IsNullOrWhiteSpace(uuid))
        {
            return;
        }

        if (this.uuid != uuid)
        {
            var material = Materials.Get(uuid);
            //Debug.Log($"material{material}");
            this.uuid = uuid;
            this.cached = material;
            OnMaterialChanged.Invoke(material);

            // SkinnedMeshRenderer[] allRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
            // foreach (var renderer in allRenderers)
            // {
            //     if (renderer.gameObject.name == "MediumMechStrikerChassis" ||
            //         renderer.gameObject.name.Contains("Body") ||
            //         renderer.gameObject.name.Contains("Chassis"))
            //     {
            //         renderer.material = material;
            //     }
            // }

            if (avatar.IsLocal)
            {
                roomClient.Me["ubiq.avatar.material.uuid"] = this.uuid;
            }
        }
    }
    public void ApplyMaterialToMeshes()
    {
        var material = this.cached;
        Debug.Log("更改材质");
        if (material == null)
        {
            return;
        }
        
        SkinnedMeshRenderer[] allRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
        int appliedCount = 0;
        
        foreach (var renderer in allRenderers)
        {
            if (renderer == null) continue;
            
            if (renderer.gameObject.name == "MediumMechStrikerChassis" ||
                renderer.gameObject.name.Contains("Body") ||
                renderer.gameObject.name.Contains("Chassis"))
            {
                renderer.material = material;
            }
        }
        
        Debug.Log($"Applied material to {appliedCount} renderers");
    }
    public Material GetMaterial()
    {
        return cached;
    }

}
