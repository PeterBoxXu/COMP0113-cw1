using UnityEngine;
using Ubiq.Avatars;
using Ubiq.Messaging;
using Ubiq.Rooms;

namespace Ubiq.Samples.Social
{
    public class AvatarJsonController : MonoBehaviour
    {
        [System.Serializable]
        public class RobotData
            {
                public int leftArm;
                public int rightArm;
                public int body;
            }
        public AvatarManager avatarManager;
        public NetworkedRobot robot;
        public RobotMaterial catalogue;
        public GameObject newAvatarPrefab;
        private NetworkScene networkScene;
        private RoomClient roomclient;

        private void Start()

        {
            var avatar1 = networkScene.GetComponentInChildren<AvatarManager>().LocalAvatar;
            Debug.Log($"11111111{avatar1}");
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
                Debug.LogError("Robot reference is missing!");
                return;
            }

            LoadJsonFromRobot();
        }

        public void LoadJsonFromRobot()
        {
            if (string.IsNullOrEmpty(robot.jsonString))
            {
                Debug.LogWarning("Robot jsonString is empty!");
                return;
            }

            avatarManager.avatarPrefab = newAvatarPrefab;
            // avatar = networkScene.GetComponentInChildren<AvatarManager>().LocalAvatar;
            // Debug.Log(avatar);
            avatarManager.UpdateAvatar();
            RobotData robotData = JsonUtility.FromJson<RobotData>(robot.jsonString);
            Debug.Log(robotData.body);

            if (robotData != null)
            {
                Debug.Log(robotData);
                ApplyBody(robotData.body);
            }
            //avatarManager.UpdateAvatar();
        }

        private void ApplyBody(int bodyIndex)
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
            // if (!roomclient)
            // {
            //     roomclient = GetComponentInChildren<RoomClient>();
                
            // }
            // Debug.Log(roomclient);
            //Debug.Log($"Avatar texture set to index: {bodyIndex} from robot JSON");
            Debug.Log($" {catalogue.Count}");
            if (bodyIndex >= 0 && bodyIndex < catalogue.Count)
            {
                var avatar = networkScene.GetComponentInChildren<AvatarManager>().LocalAvatar;
                //Debug.Log(avatar);
                 Debug.Log($"44444444444444444{avatar}");
                var texturedAvatar = avatar.GetComponent<RobotTextureChange>();
                Debug.Log(texturedAvatar);
                Debug.Log(texturedAvatar);
                if (texturedAvatar)
                {
                    Debug.Log("运行");
                    Debug.Log($" {catalogue.Get(bodyIndex)}");
                    texturedAvatar.SetMaterial(catalogue.Get(bodyIndex));
                    // avatarManager.UpdateAvatar();
                    Debug.Log("成功");
                }
                else{
                     Debug.LogWarning("失败");
                }
            }
            else
            {
                Debug.LogWarning($"Body index {bodyIndex} is out of range.");
            }
        }
    }
}