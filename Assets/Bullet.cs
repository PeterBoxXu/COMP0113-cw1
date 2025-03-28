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
    public float flyingTime = 5.0f;
    public float explodeTime = 5.0f;
    public bool owner = false;
    public ParticleSystem particleSystem;
    public MeshRenderer meshRenderer;

    private bool canExplode = false;
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
            flyingTime -= Time.deltaTime;

            if (lastPosition != transform.localPosition)
            {
                lastPosition = transform.localPosition;
                context.SendJson(new Message()
                {
                    position = transform.localPosition,
                    rotation = transform.localRotation.eulerAngles,
                    doExplode = false,
                });
            }

        }

        if (flyingTime <= 0 && !canExplode)
        {
            canExplode = true;
            context.SendJson(new Message()
            {
                position = transform.localPosition,
                rotation = transform.localRotation.eulerAngles,
                doExplode = true,
            });

            Explode();
        }

        if (canExplode)
        {
            explodeTime -= Time.deltaTime;
            if (explodeTime <= 0)
            {
                NetworkSpawnManager.Find(this).Despawn(gameObject);
            }
        }
    }

    private struct Message
    {
        public Vector3 position;
        public Vector3 rotation;
        public Team shooterTeam;
        public bool doExplode;
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // Parse the message
        var m = message.FromJson<Message>();

        // Use the message to update the Component
        transform.localPosition = m.position;
        transform.localRotation = Quaternion.Euler(m.rotation);

        // Make sure the logic in Update doesn't trigger as a result of this message
        lastPosition = transform.localPosition;

        if (m.doExplode)
        {
            Explode();
        }
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
            health.TakeDamage(1);
            Debug.Log($"Bullet hit player {player.name}, {player.IsLocal}");

            NetworkSpawnManager.Find(this).Despawn(gameObject);
        }
    }

    private void Explode()
    {
        meshRenderer.enabled = false;
        particleSystem.Play();
    }
}
