using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Spawning;
using Ubiq.Rooms;
using Avatar = Ubiq.Avatars.Avatar;

public class MechHealth : MonoBehaviour
{
    NetworkContext context;
    Avatar avatar;
    public int health { get; private set; }
    public int maxHealth = 10;

    private void Start()
    {
        context = NetworkScene.Register(this);
        avatar = GetComponentInParent<Avatar>();
        Debug.Log($"14: Health Start, id: {context.Id}, roomClient: {context.Scene.GetComponentInChildren<RoomClient>()}");
        health = maxHealth;
    }

    private struct HealthMessage
    {
        public bool fromLocal;
        public int health;
    }

    public void TakeDamage(int damage)
    {
        if (!avatar.IsLocal)
        {
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
        if (!avatar.IsLocal && !healthMessage.fromLocal)
        {
            return;
        }
        
        if (avatar.IsLocal && !healthMessage.fromLocal)
        {
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
