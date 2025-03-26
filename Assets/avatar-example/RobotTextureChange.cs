using System;
using Ubiq.Avatars;
using UnityEngine.Events;
using UnityEngine;
using Avatar = Ubiq.Avatars.Avatar;
using Ubiq.Rooms;
using Ubiq.Messaging;
using System.Runtime.InteropServices;
using Ubiq.Samples.Social;

public class RobotTextureChange : MonoBehaviour
{
    public RobotMaterial Materials;

    [Serializable]
    public class MaterialEvent : UnityEvent<Material> { }
    public MaterialEvent OnMaterialChanged;

    private Avatar avatar;
    private string uuid;
    private RoomClient roomClient;
    private AvatarJsonController avatarJsonController;
    private Material cached;
    private NetworkContext context;
    private Transform networkSceneRoot;
    private bool changeMat = false;
    
    private void Start()
    {
        if (!avatar)
        {
            avatar = GetComponentInParent<Avatar>();
            if (!avatar)
            {
                Debug.LogWarning("No Avatar could be found among parents. This script will be disabled.");
                enabled = false;
                return;
            }
        }
        context = NetworkScene.Register(this, NetworkId.Create(avatar.NetworkId, nameof(RobotTextureChange)));
        networkSceneRoot = context.Scene.transform;
        roomClient = context.Scene.GetComponentInChildren<RoomClient>();
        avatarJsonController = context.Scene.GetComponentInChildren<AvatarJsonController>();

        Debug.Log($"name {name},  roomClient" + roomClient);
        Debug.Log($"name {name}, roomClient.Me" + roomClient.Me);
        Debug.Assert(roomClient != null, "RoomClient not found in scene");
        Debug.Assert(roomClient.Me != null, "RoomClient.Me is null");
        ////roomClient = NetworkScene.Find(this).GetComponentInChildren<RoomClient>();
        //avatar = GetComponent<Avatar>();
        //// if (avatar.IsLocal)
        //// {
        ////     SetMaterial(Materials.Get(0));
        //// }

        //foreach (var roomClient1 in FindObjectsByType<RoomClient>(FindObjectsSortMode.None))
        //{
        //    if (roomClient1.gameObject.name == "peer")
        //    {
        //        roomClient = roomClient1;
        //        roomClient1.OnPeerUpdated.AddListener(RoomClient_OnPeerUpdatedMaterial);
        //    }
        //}
        roomClient.OnPeerUpdated.AddListener(RoomClient_OnPeerUpdatedMaterial);

        Debug.Log($"66: {name}, isLocal: {avatar.IsLocal}, JSON Controller: {avatarJsonController.name}");
        SetMaterial(avatarJsonController.GetBodyMaterial());
    }

    private void Update()
    {
        if (!avatar.IsLocal)
        {
            return;
        }

        if (changeMat)
        {
            Debug.Log("changeMat");
            changeMat = false;
            //roomClient.Me["ubiq.avatar.material.uuid"] = this.uuid;

            Send();
            avatarJsonController.LoadJsonFromRobot();
        }
    }

    private void Send()
    {
        Debug.LogWarning("Send");

        //context.SendJson(new RobotData { body = Materials.GetIndex(cached) });
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        Debug.LogWarning($"ProcessMessage, message: {message}");
        //MemoryMarshal.Cast<byte, State>(
        //        new ReadOnlySpan<byte>(message.bytes, message.start, message.length))
        //    .CopyTo(new Span<State>(state));
        //OnStateChange();
    }


    void RoomClient_OnPeerUpdatedMaterial(IPeer peer)
    {
        if (peer != avatar.Peer)
        {
            // The peer who is being updated is not our peer, so we can safely
            // ignore this event.
            return;
        }
        Debug.Log($"91: {name}, peer[\"ubiq.avatar.material.uuid\"] = " + peer["ubiq.avatar.material.uuid"]);
        SetMaterial(peer["ubiq.avatar.material.uuid"]);
    }

    public void SetMaterial(Material material)
    {
        SetMaterial(Materials.Get(material));
    }

    public void SetMaterial(string uuid)
    {
        Debug.Log($"102: this.uuid: {this.uuid}, param uuid: {uuid}");
        if (string.IsNullOrWhiteSpace(uuid))
        {
            return;
        }
        if (this.uuid != uuid)
        {
            var material = Materials.Get(uuid);
            Debug.Log($"12345material{material}");
            this.uuid = uuid;
            this.cached = material;
            this.changeMat = true;

            OnMaterialChanged.Invoke(material);
           
            //if (avatar == null)
            //{ avatar = GetComponent<Avatar>(); }

            //if (avatar.IsLocal)
            //{
            //    Debug.Log($"154 name {name}, roomClient:{roomClient}");
            //    Debug.Log($"aaaaaaaaaaaaaaaaaaaaaaaaaaaa,{roomClient.Me["ubiq.avatar.material.uuid"]}");
            //}

            //ApplyMaterialToMeshes(material);
        }
    }

    // public void ApplyMaterialToMeshes(Material material)
    // {
    //     Debug.Log("更改材质");
    //     if (material == null)
    //     {
    //         return;
    //     }

    //     SkinnedMeshRenderer[] allRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
    //     int appliedCount = 0;

    //     foreach (var renderer in allRenderers)
    //     {
    //         if (renderer == null) continue;
    //         if (renderer.gameObject.name == "MediumMechStrikerChassis" ||
    //             renderer.gameObject.name.Contains("Body") ||
    //             renderer.gameObject.name.Contains("Chassis"))
    //         {
    //             renderer.material = material;
    //         }
    //     }

    //     Debug.Log($"Applied material to {appliedCount} renderers");
    // }

    public Material GetMaterial()
    {
        return cached;
    }
}