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

    private RobotData robotData;

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
        roomClient.OnPeerAdded.AddListener(RoomClient_OnPeerAdded);

        if (avatar.IsLocal)
        {
            Debug.Log("avatar.IsLocal");
            string jsonString = avatarJsonController.GetJsonString();
            robotData = JsonUtility.FromJson<RobotData>(jsonString);
            Debug.Log($"jsonString: {jsonString}");

            OnMaterialChanged.Invoke(Materials.Get(robotData.body));
            //SetMaterial(Materials.Get(robotData.body));
        }

    }

    private void RoomClient_OnPeerAdded(IPeer peer)
    {
        Debug.Log($"83: {name}, isLocal: {avatar.IsLocal} - RoomClient_OnPeerAdded");
        Send();
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
            //avatarJsonController.LoadJsonFromRobot();
        }
    }

    private void Send()
    {
        Debug.LogWarning("Send");

        context.SendJson(robotData);
        //context.SendJson(new RobotData { body = Materials.GetIndex(cached) });
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        if (avatar.IsLocal)
        {
            return;
        }

        Debug.LogWarning($"ProcessMessage, message: {message}");
        robotData = message.FromJson<RobotData>();

        OnMaterialChanged.Invoke(Materials.Get(robotData.body));
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