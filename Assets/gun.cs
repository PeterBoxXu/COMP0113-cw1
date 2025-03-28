using UnityEngine;
using Ubiq.Messaging;
using UnityEngine.XR.Interaction.Toolkit;

public class Gun : MonoBehaviour
{
    private NetworkContext context;
    private bool owner;
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;

    // 定义消息格式
    private struct SyncMessage
    {
        public Vector3 position;
        public Quaternion rotation;


       public SyncMessage(Transform transform)
        {
            this.position = transform.position;
            this.rotation = transform.rotation;
        }
    }

    private void Start()
    {
        grab.selectEntered.AddListener(XRGrabInteractable_SelectEntered);
        grab.selectExited.AddListener(XRGrabInteractable_SelectExited);
        // 注册到 Ubiq 网络
        context = NetworkScene.Register(this);
    }

    private void FixedUpdate()
    {
        if(owner)
        {
            // 持续广播位置
            Debug.Log("1111");
            context.SendJson(new SyncMessage(transform));
        }
        
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage msg)
    {
        var data = msg.FromJson<SyncMessage>();
        Debug.Log("3333333333333333");
        transform.position = data.position;
        transform.rotation = data.rotation;
    }

      private void XRGrabInteractable_SelectEntered(SelectEnterEventArgs arg0)
    {
        owner = true;
    }

    private void XRGrabInteractable_SelectExited(SelectExitEventArgs eventArgs)
    {
        owner = false;
    }

}