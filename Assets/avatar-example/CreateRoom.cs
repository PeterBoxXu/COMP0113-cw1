using UnityEngine;
using System;
using Ubiq.Rooms;

public class CreateRoom : MonoBehaviour
{
    public static Guid redRoomId;
    public static Guid blueRoomId;
    public static Guid lobbyId;
    public static Guid gameRoomId;

    void Awake()
    {
        if (lobbyId == Guid.Empty) 
        {
            lobbyId = Guid.NewGuid();
            //redRoomId = Guid.NewGuid('e0842a4a-e84b-4b8f-af5a-89e35e801913');
            redRoomId = Guid.Parse("e0842a4a-e84b-4b8f-af5a-89e35e801913");
            //blueRoomId = Guid.NewGuid();
            blueRoomId = Guid.Parse("3e0ec519-a7b5-4f0c-bd82-bc980d5cf371");
            //gameRoomId = Guid.NewGuid();
            gameRoomId = Guid.Parse("4ae8f004-176a-4f01-888e-9b9c078fee59");
        }
       Debug.Log($"Lobby ID: {lobbyId}, Red Room ID: {redRoomId}, Blue Room ID: {blueRoomId}, Game Room ID: {gameRoomId}");
    }

    
}