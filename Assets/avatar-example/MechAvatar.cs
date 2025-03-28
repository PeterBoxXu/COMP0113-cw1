using Ubiq;
using Unity.XR.CoreUtils;
using UnityEngine;
using Avatar = Ubiq.Avatars.Avatar;

/// <summary>
/// Recroom/rayman style avatar with hands, torso and head. This class is only
/// here for compatibility. You can safely ignore it for this example/tutorial.
/// </summary>
public class MechAvatar : MonoBehaviour
{
    public Animator animator;
    public Transform head;
    public Transform torsoBase;
    public Transform torso;
    public Transform leftHand;
    public Transform rightHand;
    public SkinnedMeshRenderer skinnedMeshRenderer;
    public MeshRenderer LeftArmMeshRenderer;
    public MeshRenderer RightArmMeshRenderer;
    public MeshRenderer LeftHand;
    public MeshRenderer RightHand;
    public GameObject Left_Hand;
    public GameObject Right_Hand;
    public GameObject LeftMissileMatrixArm;
    public GameObject RightMissileMatrixArm;
    public GameObject LeftMissileArm;
    public GameObject RightMissileArm;
    public GameObject LeftCannonArm;
    public GameObject RightCannonArm;


    public Renderer headRenderer;


    private HeadAndHandsAvatar headAndHandsAvatar;
    public Renderer leftHandRenderer;
    public Renderer rightHandRenderer;
    private RobotTextureChange robotChange;

    private Avatar avatar;
    private InputVar<Pose> lastGoodHeadPose;
    private XROrigin xrOrigin;
    private void Start()
    {
        // avatar = GetComponentInParent<Avatar>();

        // Debug.Log("Avatar: " + avatar + " is local: " + avatar.IsLocal);
        // // if (avatar.IsLocal)
        // // {
        // //     xrOrigin = FindAnyObjectByType<XROrigin>();
        // //     // xrOrigin.gameObject.transform.SetParent(camPos);
        // // }
    }

    private void OnEnable()
    {
        headAndHandsAvatar = GetComponentInParent<HeadAndHandsAvatar>();

        if (headAndHandsAvatar)
        {
            headAndHandsAvatar.OnHeadUpdate.AddListener(HeadAndHandsEvents_OnHeadUpdate);
             headAndHandsAvatar.OnLeftHandUpdate.AddListener(HeadAndHandsEvents_OnLeftHandUpdate);
            headAndHandsAvatar.OnRightHandUpdate.AddListener(HeadAndHandsEvents_OnRightHandUpdate);
        }
        robotChange = GetComponentInParent<RobotTextureChange>();
        //robotChange.OnMaterialChanged.AddListener(robotMaterialChange);
       // 身体材质更新事件
        robotChange.OnBodyMaterialChanged.AddListener(UpdateBodyMaterial);
        // 左手臂材质更新事件
        robotChange.OnLeftArmMaterialChanged.AddListener(UpdateLeftArmMaterial);
        // 右手臂材质更新事件
        robotChange.OnRightArmMaterialChanged.AddListener(UpdateRightArmMaterial);
        // 左手材质更新事件
        robotChange.OnLeftHandMaterialChanged.AddListener(UpdateLeftHandMaterial);
        // 右手材质更新事件
        robotChange.OnRightHandMaterialChanged.AddListener(UpdateRightHandMaterial);
        //左手臂武器更新事件
        robotChange.OnLeftArmWeaponChanged.AddListener(UpdateLeftArmWeapon);
        //右手臂武器更新事件
        robotChange.OnRightArmWeaponChanged.AddListener(UpdateRightArmWeapon);
    }
    private void OnDisable()
    {
        if (headAndHandsAvatar && headAndHandsAvatar != null)
        {
            headAndHandsAvatar.OnHeadUpdate.RemoveListener(HeadAndHandsEvents_OnHeadUpdate);
             headAndHandsAvatar.OnLeftHandUpdate.RemoveListener(HeadAndHandsEvents_OnLeftHandUpdate);
            headAndHandsAvatar.OnRightHandUpdate.RemoveListener(HeadAndHandsEvents_OnRightHandUpdate);
        }
        //robotChange.OnMaterialChanged.RemoveListener(robotMaterialChange);
        robotChange.OnBodyMaterialChanged.RemoveListener(UpdateBodyMaterial);
        
        robotChange.OnLeftArmMaterialChanged.RemoveListener(UpdateLeftArmMaterial);
        
        robotChange.OnRightArmMaterialChanged.RemoveListener(UpdateRightArmMaterial);

        robotChange.OnLeftArmWeaponChanged.RemoveListener(UpdateLeftArmWeapon);
  
        robotChange.OnRightArmWeaponChanged.RemoveListener(UpdateRightArmWeapon);
    }

    private void UpdateLeftArmMaterial(Material material)
    {
        if (LeftArmMeshRenderer == null)
        {
            return;
        }
       LeftArmMeshRenderer.material = material;
    }
  private void UpdateLeftHandMaterial(Material material)
    {
        if (LeftHand == null)
        {
            return;
        }
        LeftHand.material = material;
    }

    private void UpdateRightHandMaterial(Material material)
    {
        if (RightHand == null)
        {
            return;
        }
        RightHand.material = material;
    }
    private void UpdateRightArmMaterial(Material material)
    {
        if (RightArmMeshRenderer == null)
        {
            return;
        }
        RightArmMeshRenderer.material = material;
    }
    private void UpdateLeftArmWeapon(int weapon){
        if(weapon == 0){
            LeftMissileArm.SetActive(true);
        }
        else if (weapon == 1){
            LeftCannonArm.SetActive(true);
        }
        else  if (weapon == 2){
            LeftMissileMatrixArm.SetActive(true);
        }
        else if(weapon == 3){
            Left_Hand.SetActive(true);
        }
    }
    private void UpdateRightArmWeapon(int weapon){
        if(weapon == 0){
            RightMissileArm.SetActive(true);
        }
        else if (weapon == 1){
            RightCannonArm.SetActive(true);
        }
        else  if (weapon == 2){
            RightMissileMatrixArm.SetActive(true);
        }
        else if(weapon == 4){
            Right_Hand.SetActive(true);
        }
    }
    private void HeadAndHandsEvents_OnHeadUpdate(InputVar<Pose> pose)
    {
        if (!pose.valid)
        {
            if (!lastGoodHeadPose.valid)
            {
                headRenderer.enabled = false;
                return;
            }
            
            pose = lastGoodHeadPose;
        }
        
        head.position = pose.value.position;
        head.rotation = Quaternion.Euler(0, pose.value.rotation.eulerAngles.y, 0);        
        if (lastGoodHeadPose.value == pose.value)
        {
            animator.SetBool("walking", false);
        }
        else
        {
            animator.SetBool("walking", true);
        }
        lastGoodHeadPose = pose;
    }

    private void HeadAndHandsEvents_OnLeftHandUpdate(InputVar<Pose> pose)
    {
        if (!pose.valid)
        {
            leftHandRenderer.enabled = false;
            return;
        }

        leftHandRenderer.enabled = true;
        leftHand.position = pose.value.position;
        leftHand.rotation = pose.value.rotation;
    }

    private void HeadAndHandsEvents_OnRightHandUpdate(InputVar<Pose> pose)
    {
        if (!pose.valid)
        {
            rightHandRenderer.enabled = false;
            return;
        }

        rightHandRenderer.enabled = true;
        rightHand.position = pose.value.position;
        rightHand.rotation = pose.value.rotation;
    }

   private void UpdateBodyMaterial(Material material)
    {
        skinnedMeshRenderer.material = material;
    }

    private void Update()
    {
        UpdateTorso();
        // Debug.Log("torso position: " + torso.position);
    }

    private void UpdateTorso()
    {
        torso.position = torsoBase.position;
        //torso rotation is the rotation of the torso base except for the y axis

        torso.rotation = Quaternion.Euler(0, torsoBase.rotation.eulerAngles.y, 0);
    }

    // private Vector3 handsFwdStore;

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.blue;
    //     Gizmos.DrawLine(head.position, footPosition);
    //     // Gizmos.DrawLine(head.position,head.position + handsFwdStore);
    // }
}
