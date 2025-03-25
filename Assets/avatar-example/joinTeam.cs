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
        public GameObject doorOpenController;
        public Transform fightSpawnPoint;
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
            Debug.Log(CreateRoom.redRoomId);
            foreach (var roomClient in FindObjectsByType<RoomClient>(FindObjectsSortMode.None))
            {
                roomClient.Join(CreateRoom.redRoomId);
            }
            doorOpenController.GetComponent<DoorOpenController>().OnDoorOpen();
        }
        public void JoinBlueTeam(){
            foreach (var roomClient in FindObjectsByType<RoomClient>(FindObjectsSortMode.None))
            {
                roomClient.Join(CreateRoom.blueRoomId);
            }
            Debug.Log(CreateRoom.blueRoomId);

            doorOpenController.GetComponent<DoorOpenController>().OnDoorOpen();
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
            if (shouldTeleport && xrRig != null && fightSpawnPoint != null)
            {
                xrRig.transform.position = fightSpawnPoint.position;
                xrRig.transform.rotation = fightSpawnPoint.rotation;
                Debug.Log("Player moved to spawn point");

                shouldTeleport = false;
            }
        }
    }

}
