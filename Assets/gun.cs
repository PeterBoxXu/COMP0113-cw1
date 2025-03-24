using UnityEngine;
using Ubiq.Messaging;

public class Gun : MonoBehaviour
{
    private NetworkContext context;

    // 定义消息格式
    private struct SyncMessage
    {
        public Vector3 position;

        public SyncMessage(Vector3 pos)
        {
            position = pos;
        }
    }

    private void Start()
    {
        // 注册到 Ubiq 网络
        context = NetworkScene.Register(this);
    }

    private void FixedUpdate()
    {
        // 持续广播位置
        Debug.Log("1111");
        context.SendJson(new SyncMessage(transform.position));
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        var data = msg.FromJson<SyncMessage>();
        Debug.Log("3333333333333333");
        transform.position = data.position;
    }
}