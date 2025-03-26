using UnityEngine;
using Ubiq.Avatars;
using Ubiq.Messaging;
using Ubiq.Rooms;

namespace Ubiq.Samples.Social
{
    public class AvatarJsonController : MonoBehaviour
    {
        public AvatarManager avatarManager;
        public NetworkedRobot robot;
        public RobotMaterial catalogue;
        public GameObject newAvatarPrefab;
        private NetworkScene networkScene;
        private RoomClient roomclient;

        private void Start()
        {
            if (!networkScene)
            {
                networkScene = NetworkScene.Find(this);
                if (!networkScene)
                {
                    Debug.LogError("NetworkScene not found.");
                    return;
                }
            }
            if (robot == null)
            {
                robot = FindFirstObjectByType<NetworkedRobot>();
                Debug.Log("Networked robot found: " + (robot != null));
            }
        }

        public string GetJsonString()
        {
            return robot.jsonString;
        }

        public Material GetBodyMaterial()
        {
            RobotData robotData = JsonUtility.FromJson<RobotData>(robot.jsonString);
            int bodyIndex = robotData.body;
            return catalogue.Get(bodyIndex);
        }

        //public void LoadJsonFromRobot()
        //{
        //    Debug.Log($"{name} LoadJsonFromRobot, jsonString: ");
        //    RobotData robotData = JsonUtility.FromJson<RobotData>(robot.jsonString);
        //    Debug.Log(robotData.body);

        //    if (robotData != null)
        //    {
        //        Debug.Log(robotData);
        //        ApplyBody(robotData.body);
        //    }
        //}

        //private void ApplyBody(int bodyIndex)
        //{
        //    if (!networkScene)
        //    {
        //        networkScene = NetworkScene.Find(this);
        //        if (!networkScene)
        //        {
        //            Debug.LogError("NetworkScene not found.");
        //            return;
        //        }
        //    }
        //    // if (!roomclient)
        //    // {
        //    //     roomclient = GetComponentInChildren<RoomClient>();
                
        //    // }
        //    // Debug.Log(roomclient);
        //    //Debug.Log($"Avatar texture set to index: {bodyIndex} from robot JSON");
        //    Debug.Log($" {catalogue.Count}");
        //    if (bodyIndex >= 0 && bodyIndex < catalogue.Count)
        //    {
        //        var avatar = networkScene.GetComponentInChildren<AvatarManager>().LocalAvatar;
        //        //Debug.Log(avatar);
        //         Debug.Log($"44444444444444444{avatar}");
        //        var texturedAvatar = avatar.GetComponent<RobotTextureChange>();
        //        Debug.Log(texturedAvatar);
        //        if (texturedAvatar)
        //        {
        //            Debug.Log("运行");
        //            Debug.Log($" {catalogue.Get(bodyIndex)}");
        //            texturedAvatar.SetMaterial(catalogue.Get(bodyIndex));
        //            // avatarManager.UpdateAvatar();
        //            Debug.Log("成功");
        //        }
        //        else{
        //             Debug.LogWarning("失败");
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogWarning($"Body index {bodyIndex} is out of range.");
        //    }
        //}
    }
}