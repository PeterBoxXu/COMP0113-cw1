using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ubiq.Rooms;
namespace Ubiq.Samples
{
    public class joinTeam : MonoBehaviour
    {
        public SocialMenu mainObject;

        // public GameObject
        public Transform spawnPoint;
        public GameObject xrRig;
        private bool shouldTeleport = false;
  
        private void OnEnable()
        {
            mainObject.roomClient.OnJoinedRoom.AddListener(OnRoomJoined);
        }

        private void OnDisable()
        {
            mainObject.roomClient.OnJoinedRoom.RemoveListener(OnRoomJoined);
        }

        public void JoinRedTeam(){
            shouldTeleport = true;
            Debug.Log(CreateRoom.redRoomId);
            foreach (var roomClient in FindObjectsByType<RoomClient>(FindObjectsSortMode.None))
            {
                roomClient.Join(CreateRoom.redRoomId);
            }
        }
        public void JoinBlueTeam(){
            shouldTeleport = true;
            foreach (var roomClient in FindObjectsByType<RoomClient>(FindObjectsSortMode.None))
            {
                roomClient.Join(CreateRoom.blueRoomId);
            }
            Debug.Log(CreateRoom.blueRoomId);
        }
        public void JoinGame(){
            shouldTeleport = true;
            foreach (var roomClient in FindObjectsByType<RoomClient>(FindObjectsSortMode.None))
            {
                roomClient.Join(CreateRoom.gameRoomId);
            }
            Debug.Log(CreateRoom.gameRoomId);
        }

        private void OnRoomJoined(IRoom room)
        {
            if (shouldTeleport && xrRig != null && spawnPoint != null)
            {
                xrRig.transform.position = spawnPoint.position;
                xrRig.transform.rotation = spawnPoint.rotation;
                Debug.Log("Player moved to spawn point");

                shouldTeleport = false;
            }
        }
    }

}
