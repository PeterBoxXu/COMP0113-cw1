using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Spawning;
using Ubiq.Rooms;
using Avatar = Ubiq.Avatars.Avatar;

public enum Team : int
{
    Red,
    Blue
}

public class Bullet : MonoBehaviour, INetworkSpawnable
{
    public NetworkId NetworkId { get; set; }
    public float timeLeft = 5.0f;
    public bool owner = false;

    NetworkContext context;

    // Start is called before the first frame update
    void Start()
    {
        context = NetworkScene.Register(this);
        Debug.Log($"22: Bullet Start, id: {context.Id}, roomClient: {context.Scene.GetComponentInChildren<RoomClient>()}");
        //Avatar[] avatar = context.Scene.GetComponentsInChildren<Avatar>();
    }

    Vector3 lastPosition;

    // Update is called once per frame
    void Update()
    {
        if (owner)
        {
            timeLeft -= Time.deltaTime;

            if (lastPosition != transform.localPosition)
            {
                lastPosition = transform.localPosition;
                context.SendJson(new Message()
                {
                    position = transform.localPosition,
                    timeLeft = timeLeft,
                });
            }

        }

        if (timeLeft <= 0)
        {
            NetworkSpawnManager.Find(this).Despawn(gameObject);
        }
    }

    private struct Message
    {
        public Vector3 position;
        public Team shooterTeam;
        public float timeLeft;
        public bool canDestroy;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // Parse the message
        var m = message.FromJson<Message>();

        // Use the message to update the Component
        transform.localPosition = m.position;

        // Make sure the logic in Update doesn't trigger as a result of this message
        lastPosition = transform.localPosition;
        timeLeft = m.timeLeft;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!owner)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            var player = other.GetComponentInParent<MechAvatar>().GetComponentInParent<Avatar>();
            MechHealth health = other.GetComponentInParent<MechHealth>();
            Debug.Log($"Bullet hit player {player.name}, {player.IsLocal}");

            NetworkSpawnManager.Find(this).Despawn(gameObject);
        }
    }
}
