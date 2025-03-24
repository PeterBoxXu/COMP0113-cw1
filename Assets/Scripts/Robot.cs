using System;
using UnityEngine;
using UnityEngine.UI;
using Ubiq.Rooms;
using Ubiq.Messaging;

public enum MaterialType
{
    Blue = 0,
    Red = 1,
    Yellow = 2,
}

public class NetworkedRobot : MonoBehaviour
{
    public string jsonString;
    public Text logText;
    public int robotCode;
    public SkinnedMeshRenderer leftArmRenderer;
    public MeshRenderer leftHandRenderer;
    public SkinnedMeshRenderer rightArmRenderer;
    public MeshRenderer rightHandRenderer;
    public SkinnedMeshRenderer bodyRenderer;
    public int[] robotState = new int[3];
    public Material[] bodyMaterials;

    private string networkIdString;
    private RoomClient roomClient;
    private string localPeerId;
    private bool isSyncing = false;
    private float syncCooldown = 0.1f; // 同步冷却时间
    private float lastSyncTime = 0f;
    private bool pendingSync = false;

    private void Awake()
    {
        // Initialize default robot state
        robotCode = 0;
        robotState[0] = 0;
        robotState[1] = 0;
        robotState[2] = 0;

        // Set initial materials if available
        if (bodyMaterials != null && bodyMaterials.Length > 0)
        {
            leftArmRenderer.material = bodyMaterials[0];
            leftHandRenderer.material = bodyMaterials[0];
            rightArmRenderer.material = bodyMaterials[0];
            rightHandRenderer.material = bodyMaterials[0];
            bodyRenderer.material = bodyMaterials[0];
        }
    }

    private void Start()
    {
        // Initialize networking
        networkIdString = NetworkId.Create(this).ToString();
        Debug.Log($"NetworkId created: {networkIdString}");
        
        roomClient = GetComponent<RoomClient>();
        if (roomClient == null)
        {
            Debug.LogError("RoomClient component not found!");
            return;
        }
        
        // 存储本地玩家ID用于权限管理
        if (roomClient.Me != null)
        {
            localPeerId = roomClient.Me.uuid;
            Debug.Log($"Local Peer ID: {localPeerId}");
        }
        
        Debug.Log("Adding room updated listener");
        roomClient.OnRoomUpdated.AddListener(RoomClient_OnRoomUpdated);
        //新用户进入的时候，自动同步一次信息
        roomClient.OnPeerAdded.AddListener(RoomClient_OnPeerAdded);
        SyncRobotState();
    }
    private void RoomClient_OnPeerAdded(IPeer peer)
    {
    // 避免对自己触发
    if (peer.uuid == localPeerId)
    {
        return;
    }

    Debug.Log($"New peer joined: {peer.uuid}. Syncing current robot state.");
    
    // 再次推送当前 JSON，确保新玩家能接收到
    SyncRobotState();
    }   
    private void Update()
    {
        // 处理延迟同步
        if (pendingSync && Time.time - lastSyncTime > syncCooldown)
        {
            pendingSync = false;
            SyncRobotState();
        }
    }

    private void RoomClient_OnRoomUpdated(IRoom room)
    {
        // 避免自己的更新触发自己的回调
        if (isSyncing)
        {
            isSyncing = false;
            return;
        }

        Debug.Log($"OnRoomUpdated: {networkIdString} value = {room[networkIdString]}");
        var robotProperty = room[networkIdString];
        if (!string.IsNullOrEmpty(robotProperty))
        {
            try
            {
                // 解析JSON数据
                RobotData data = JsonUtility.FromJson<RobotData>(robotProperty);
                
                // 检查是否需要应用更新 (避免无限循环)
                if (data.leftArm != robotState[0] || 
                    data.rightArm != robotState[1] || 
                    data.body != robotState[2])
                {
                    // 应用数据更新
                    ApplyRobotData(data);
                    
                    // 更新本地状态
                    robotState[0] = data.leftArm;
                    robotState[1] = data.rightArm;
                    robotState[2] = data.body;
                    
                    // 更新UI显示
                    UpdateRobotUI();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing robot data: {e.Message}");
            }
        }
    }

    // 应用机器人数据但不广播更改
    private void ApplyRobotData(RobotData data)
    {
        int left = data.leftArm;
        int right = data.rightArm;
        int body = data.body;
        
        if (bodyMaterials != null && bodyMaterials.Length > 0)
        {
            if (left >= 0 && left < bodyMaterials.Length)
            {
                leftArmRenderer.material = bodyMaterials[left];
                leftHandRenderer.material = bodyMaterials[left];
            }
            
            if (right >= 0 && right < bodyMaterials.Length)
            {
                rightArmRenderer.material = bodyMaterials[right];
                rightHandRenderer.material = bodyMaterials[right];
            }
            
            if (body >= 0 && body < bodyMaterials.Length)
            {
                bodyRenderer.material = bodyMaterials[body];
            }
        }
    }

    public void ChangeBodyColor(int color)
    {
        if (color >= 0 && color < bodyMaterials.Length)
        {
            Material m = bodyMaterials[color];
            bodyRenderer.material = m;
            robotState[2] = color;
            
            // 设置延迟同步
            ScheduleSync();
        }
    }
    
    public void ChangeLeftArmColor(int color)
    {
        if (color >= 0 && color < bodyMaterials.Length)
        {
            Material m = bodyMaterials[color];
            leftArmRenderer.material = m;
            leftHandRenderer.material = m;
            robotState[0] = color;
            
            // 设置延迟同步
            ScheduleSync();
        }
    }
    
    public void ChangeRightArmColor(int color)
    {
        if (color >= 0 && color < bodyMaterials.Length)
        {
            Material m = bodyMaterials[color];
            rightArmRenderer.material = m;
            rightHandRenderer.material = m;
            robotState[1] = color;
            
            // 设置延迟同步
            ScheduleSync();
        }
    }
    
    public void ResetRobot()
    {
        logText.text = "";
        
        if (bodyMaterials != null && bodyMaterials.Length > 0)
        {
            leftArmRenderer.material = bodyMaterials[0];
            leftHandRenderer.material = bodyMaterials[0];
            rightArmRenderer.material = bodyMaterials[0];
            rightHandRenderer.material = bodyMaterials[0];
            bodyRenderer.material = bodyMaterials[0];
        }
        
        robotState[0] = 0;
        robotState[1] = 0;
        robotState[2] = 0;

        // 立即同步重置状态到网络
        SyncRobotState();
    }

    [Serializable]
    public class RobotData
    {
        public int leftArm;
        public int rightArm;
        public int body;
    }

    // 更新UI
    private void UpdateRobotUI()
    {
        robotCode++;
        
        if (robotCode < 10)
        {
            logText.text = "Robot Build-Up Complete! Robot Code: 00" + robotCode;
        }
        else if (robotCode < 100)
        {
            logText.text = "Robot Build-Up Complete! Robot Code: 0" + robotCode;
        }
        else
        {
            logText.text = "Robot Build-Up Complete! Robot Code: " + robotCode;
        }
    }
    
    // 延迟同步，防止过于频繁的网络更新
    private void ScheduleSync()
    {
        pendingSync = true;
        
        // 如果冷却已结束，可以立即同步
        if (Time.time - lastSyncTime > syncCooldown)
        {
            pendingSync = false;
            SyncRobotState();
        }
    }

    private void SyncRobotState()
    {
        if (roomClient != null && roomClient.Room != null)
        {
            lastSyncTime = Time.time;
            
            RobotData data = new RobotData
            {
                leftArm = robotState[0],
                rightArm = robotState[1],
                body = robotState[2]
            };
            Debug.Log(data);
            // 转换为JSON
            jsonString = JsonUtility.ToJson(data);
            Debug.Log(jsonString);
            
            UpdateRobotUI();
            // 设置同步标志以避免处理自己的更新
            isSyncing = true;
            
            // 更新房间属性
            roomClient.Room[networkIdString] = jsonString;
        }
    }

    public void ShowErrorUI()
    {
        logText.text = "Robot incomplete! Please try again.";
    }
}