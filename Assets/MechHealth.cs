using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Spawning;
using Ubiq.Rooms;
using Avatar = Ubiq.Avatars.Avatar;

public class MechHealth : MonoBehaviour
{
    NetworkContext context;
    public Avatar avatar;
    public int health { get; private set; }
    public int maxHealth = 10;

    private void Start()
    {
        context = NetworkScene.Register(this);
        avatar = GetComponentInParent<Avatar>();
        Debug.Log($"14: Health Start, id: {context.Id}, roomClient: {context.Scene.GetComponentInChildren<RoomClient>()}");
        health = maxHealth;

        RoomClient roomClient = context.Scene.GetComponentInChildren<RoomClient>();
        roomClient.OnJoinedRoom.AddListener(RoomClient_OnJoinedRoom);
        roomClient.OnPeerAdded.AddListener(RoomClient_OnPeerAdded);
    }

    private struct HealthMessage
    {
        public bool fromLocal;
        public int health;
    }

    private void RoomClient_OnJoinedRoom(IRoom room)
    {
        Debug.Log("Health RoomClient_OnJoinedRoom");
        Send();
    }

    private void RoomClient_OnPeerAdded(IPeer peer)
    {
        Debug.Log("Health RoomClient_OnPeerAdded");
        Send();
    }

    private void Send()
    {
        if (avatar.IsLocal)
        {
            context.SendJson(new HealthMessage()
            {
                fromLocal = true,
                health = health
            });
        }
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"Health {avatar.name} TakeDamage");
        if (!avatar.IsLocal)
        {
            Debug.Log("Health is not local");
            context.SendJson(new HealthMessage()
            {
                fromLocal = false,
                health = health - damage
            });
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        if (health <= 0)
        {
            Debug.LogWarning("Health is 0");
            return;
        }

        HealthMessage healthMessage = message.FromJson<HealthMessage>();
        Debug.Log($"Health {avatar.name} ProcessMessage");

        if (!avatar.IsLocal && !healthMessage.fromLocal)
        {
            return;
        }
        
        if (avatar.IsLocal && !healthMessage.fromLocal)
        {
            Debug.Log("Health is local and message not from local");
            if (health < healthMessage.health)
            {
                context.SendJson(new HealthMessage()
                {
                    fromLocal = true,
                    health = health
                });
                return;
            }

            health = healthMessage.health;
            context.SendJson(new HealthMessage()
            {
                fromLocal = true,
                health = health
            });
        }

        else if (!avatar.IsLocal && healthMessage.fromLocal)
        {
            health = healthMessage.health;
        }

        if (health <= 0)
        {
            Debug.LogWarning("Health is 0");
        }
    }
}
