using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ubiq.Rooms;

public class JoinLobbyonStart : MonoBehaviour
{

    private void Start()
        {
            GetComponent<RoomClient>().Join(CreateRoom.lobbyId);
        }

}
