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
    }
}