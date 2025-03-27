using System;
using UnityEngine;
using UnityEngine.UI;
using Ubiq.Rooms;
using Ubiq.Messaging;

public class NetworkedRobot : MonoBehaviour
{
    public String jsonString;
    public SkinnedMeshRenderer leftArmRenderer;
    public MeshRenderer leftHandRenderer;
    public SkinnedMeshRenderer rightArmRenderer;
    public MeshRenderer rightHandRenderer;
    public SkinnedMeshRenderer bodyRenderer;
    public int[] robotState = new int[5];
    public Material[] bodyMaterials;
    public GameObject[] armPrefabs;
    private GameObject leftArm;
    private GameObject rightArm;

    //private string networkIdString;
    private NetworkContext context;
    private RoomClient roomClient;

    private bool canSend = false;
    private int seqNo = 0;
    private struct Message
    {
        public int seqNo;
        public RobotData data;
    }

    private void Awake()
    {
        robotState[0] = 0;
        robotState[1] = 0;
        robotState[2] = 0;
        robotState[3] = 3;
        robotState[4] = 4;
        leftArmRenderer.material = bodyMaterials[0];
        leftHandRenderer.material = bodyMaterials[0];
        rightArmRenderer.material = bodyMaterials[0];
        rightHandRenderer.material = bodyMaterials[0];
        bodyRenderer.material = bodyMaterials[0];
        leftArm = transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).gameObject;
        rightArm = transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetChild(2).GetChild(1).GetChild(0).GetChild(0).gameObject;
    }

    private void Start()
    {
        context = NetworkScene.Register(this);
        roomClient = context.Scene.GetComponentInChildren<RoomClient>();

        Debug.Log("context: " + context.Id);
        Debug.Log("context.Scene: " + context.Scene.transform.name);
        Debug.Log("roomclient: " + roomClient);
        //// Initialize networking
        //networkIdString = NetworkId.Create(this).ToString();
        //roomClient = GetComponentInParent<RoomClient>();
        //roomClient.OnPeerUpdated.AddListener(RoomClient_OnRoomUpdated);
    }

    private void Update()
    {
        //SyncRobotState();
        GetRobotData();
        //ApplyRobotData(JsonUtility.FromJson<RobotData>(jsonString));
        if (canSend)
        {
            Send();
        }
    }

    private void Send()
    {
        Debug.Log("81: Sending: " + jsonString);
        canSend = false;
        context.SendJson<Message>(new Message()
        {
            seqNo = seqNo++,
            data = JsonUtility.FromJson<RobotData>(jsonString)
        });
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        Debug.Log("72: " + message);
        Message msg = message.FromJson<Message>();
        if (msg.seqNo < seqNo)
        {
            Send();
            return;
        }
        seqNo = msg.seqNo + 1;
        // Parse the JSON data from the room property
        RobotData data = msg.data;
        jsonString = JsonUtility.ToJson(data);

        // Apply the robot configuration based on the parsed data
        ApplyRobotData(data);
        
    }


    //private void RoomClient_OnRoomUpdated(IPeer peer)
    //{
    //    Debug.Log("Room updated: " + networkIdString);
    //    var robotProperty = room[networkIdString];
    //    if (!string.IsNullOrEmpty(robotProperty))
    //    {
    //        // Parse the JSON data from the room property
    //        RobotData data = JsonUtility.FromJson<RobotData>(robotProperty);

    //        // Apply the robot configuration based on the parsed data
    //        ApplyRobotData(data);
    //    }
    //}

    // Apply robot data without broadcasting changes
    private void ApplyRobotData(RobotData data)
    {
        int left = data.leftArm;
        int right = data.rightArm;
        int body = data.body;
        int leftWeapon = data.leftArmWeapon;
        int rightWeapon = data.rightArmWeapon;
        
        leftArmRenderer.material = bodyMaterials[left];
        if (leftHandRenderer != null)
        {
            leftHandRenderer.material = bodyMaterials[left];
        }
        rightArmRenderer.material = bodyMaterials[right];
        if (rightHandRenderer != null)
        {
            rightHandRenderer.material = bodyMaterials[right];
        }
        bodyRenderer.material = bodyMaterials[body];
        
        foreach (Transform child in leftArm.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in rightArm.transform)
        {
            Destroy(child.gameObject);
        }
        
        GameObject leftHand = Instantiate(armPrefabs[leftWeapon]);
        GameObject rightHand = Instantiate(armPrefabs[rightWeapon]);
        Vector3 pos = Vector3.zero;
        Vector3 rot = Vector3.zero;
        
        switch (leftWeapon)
        {
            case 0:
                pos = new Vector3(0.5f, 1.2f, -0.25f);
                rot = new Vector3(90, 0, 0);
                break;
            case 1:
                pos = new Vector3(1.5f, 1.5f, -1f);
                rot = new Vector3(-90, 0, -90);
                break;
            case 2:
                pos = new Vector3(0.0f, 1.0f, 3.0f);
                rot = new Vector3(-70, 0, 90);
                break;
            case 3:
                pos = new Vector3(-0.2f, 3.5f, -0.3f);
                rot = new Vector3(-90, 90, 90);
                break;
        }
        leftHand.transform.SetParent(leftArm.transform);
        leftHand.transform.localPosition = pos;
        leftHand.transform.localRotation = Quaternion.Euler(rot);

        switch (rightWeapon)
        {
            case 0:
                pos = new Vector3(0.5f, 1.2f, -0.25f);
                rot = new Vector3(90, 0, 0);
                break;
            case 1:
                pos = new Vector3(-1f, 1.5f, 0.5f);
                rot = new Vector3(-90, 0, 90);
                break;
            case 2:
                pos = new Vector3(0.0f, 1.0f, 3.0f);
                rot = new Vector3(-70, 0, 90);
                break;
            case 4:
                pos = new Vector3(-0.2f, 3.5f, -0.3f);
                rot = new Vector3(-90, 90, 90);
                break;
        }
        rightHand.transform.SetParent(rightArm.transform);
        rightHand.transform.localPosition = pos;
        rightHand.transform.localRotation = Quaternion.Euler(rot);
        
        robotState[0] = left;
        robotState[1] = right;
        robotState[2] = body;
        robotState[3] = leftWeapon;
        robotState[4] = rightWeapon;
    }

    public void ChangeBodyColor(int color)
    {
        if (0 <= color && color < bodyMaterials.Length)
        {
            canSend = true;
            Material m = bodyMaterials[color];
            bodyRenderer.material = m;
            robotState[2] = color;
        }
    }
    
    public void ChangeLeftArmColor(int color)
    {
        if (0 <= color && color < bodyMaterials.Length)
        {
            canSend = true;
            Material m = bodyMaterials[color];
            leftArmRenderer.material = m;
            if (leftHandRenderer != null)
            {
                leftHandRenderer.material = m;
            }
            robotState[0] = color;
        }
    }
    
    public void ChangeRightArmColor(int color)
    {
        if (0 <= color && color < bodyMaterials.Length)
        {
            canSend = true;
            Material m = bodyMaterials[color];
            rightArmRenderer.material = m;
            if (rightHandRenderer != null)
            {
                rightHandRenderer.material = m;
            }
            robotState[1] = color;
        }
    }

    public void ChangeArmType(int type)
    {
        int flag = type % 10;
        bool isLeft = flag <= 0;
        type = type / 10 - 1;
        Debug.Log(type);
        Vector3 pos = Vector3.zero;
        Vector3 rot = Vector3.zero;
        if (isLeft)
        {
            robotState[3] = type;
        }
        else
        {
            robotState[4] = type;
        }
        
        if (type == 0)
        {
            pos = new Vector3(0.5f, 1.2f, -0.25f);
            rot = new Vector3(90, 0, 0);

        }else if (type == 1)
        {
            if (isLeft)
            {
                pos = new Vector3(1.5f, 1.5f, -1f);
                rot = new Vector3(-90, 0, -90);
            }
            else
            {
                pos = new Vector3(-1f, 1.5f, 0.5f);
                rot = new Vector3(-90, 0, 90);
            }
        }else if (type == 2)
        {
            pos = new Vector3(0.0f, 1.0f, 3.0f);
            rot = new Vector3(-70, 0, 90);
        }

        if (0 <= type && type < armPrefabs.Length)
        {
            canSend = true;
            GameObject arm = Instantiate(armPrefabs[type]);
            if (isLeft)
            {
                foreach (Transform child in leftArm.transform)
                {
                    Destroy(child.gameObject);
                }

                arm.transform.SetParent(leftArm.transform);
            }
            else
            {
                foreach (Transform child in rightArm.transform)
                {
                    Destroy(child.gameObject);
                }
                arm.transform.SetParent(rightArm.transform);
            }
            arm.transform.localPosition = pos;
            arm.transform.localRotation = Quaternion.Euler(rot);
            
            arm.SetActive(true);
        }
    }
    
    public void ResetRobot()
    {
        robotState[0] = 0;
        robotState[1] = 0;
        robotState[2] = 0;
        
        leftArmRenderer.material = bodyMaterials[0];
        if (leftHandRenderer != null)
        {
            leftHandRenderer.material = bodyMaterials[0];
        }
        rightArmRenderer.material = bodyMaterials[0];
        if (rightHandRenderer != null)
        {
            rightHandRenderer.material = bodyMaterials[0];
        }
        bodyRenderer.material = bodyMaterials[0];
        foreach (Transform child in leftArm.transform)
        {
            Destroy(child.gameObject);
        }
        GameObject leftHand = Instantiate(armPrefabs[3]);
        leftHand.transform.SetParent(leftArm.transform);
        leftHandRenderer = leftHand.GetComponent<MeshRenderer>();
        leftHand.transform.localPosition = new Vector3(-0.2f, 3.5f, -0.3f);
        leftHand.transform.localRotation = Quaternion.Euler(-90, 90, 90);
        robotState[3] = 3;
        foreach (Transform child in rightArm.transform)
        {
            Destroy(child.gameObject);
        }
        GameObject rightHand = Instantiate(armPrefabs[4]);
        rightHand.transform.SetParent(rightArm.transform);
        rightHand.transform.localPosition = new Vector3(-0.2f, 3.5f, -0.3f);
        rightHand.transform.localRotation = Quaternion.Euler(-90, 90, 90);
        robotState[4] = 4;
        rightHandRenderer = rightHand.GetComponent<MeshRenderer>();
        
        // Clear the robot state on the network
        if (roomClient && roomClient.Room != null)
        {
            canSend = true;
            //roomClient.Room[networkIdString] = "";
        }
    }



    // This method now collects the robot data and syncs it across the network
    public void GetRobotData()
    {
        RobotData data = new RobotData();
        bool isComplete = true;
        
        data.leftArm = robotState[0];
        data.rightArm = robotState[1];
        data.body = robotState[2];
        data.leftArmWeapon = robotState[3];
        data.rightArmWeapon = robotState[4];
        
        jsonString = JsonUtility.ToJson(data);
        Debug.Log("Robot Data in JSON: " + jsonString);
    }

    public void testApply()
    {
        RobotData data = JsonUtility.FromJson<RobotData>(jsonString);
        ApplyRobotData(data);
    }

    public void disableArms()
    {
        leftArm.gameObject.SetActive(false);
        rightArm.gameObject.SetActive(false);
        leftArmRenderer.gameObject.SetActive(false);
        rightArmRenderer.gameObject.SetActive(false);
    }

    public void enableArms()
    {
        leftArm.gameObject.SetActive(true);
        rightArm.gameObject.SetActive(true);
        leftArmRenderer.gameObject.SetActive(true);
        rightArmRenderer.gameObject.SetActive(true);
    }
}

[Serializable]
public class RobotData
{
    public int leftArm;
    public int rightArm;
    public int body;
    public int leftArmWeapon;
    public int rightArmWeapon;
}