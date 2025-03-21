using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class OverlayController : MonoBehaviour
{
    // 关联UI黑幕的材质
    public Material overlayMaterial;

    // 摇杆最大输入幅度（通常为1.0）
    public float maxJoystickMagnitude = 1f;

    // 当摇杆输入为0时，HoleRadius（孔半径）的值（孔较大）
    public float maxHoleRadius = 0.5f;

    // 当摇杆输入达到最大时，HoleRadius的值（孔较小）
    public float minHoleRadius = 0.2f;

    // 左手设备对象
    private InputDevice leftHandDevice;

    void Start()
    {
        // 在Start中初始化获取左手控制器设备
        List<InputDevice> leftHandDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandDevices);
        if (leftHandDevices.Count > 0)
        {
            leftHandDevice = leftHandDevices[0];
        }
    }

    void Update()
    {
        // 如果当前设备失效，则尝试重新获取
        if (!leftHandDevice.isValid)
        {
            List<InputDevice> leftHandDevices = new List<InputDevice>();
            InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandDevices);
            if (leftHandDevices.Count > 0)
            {
                leftHandDevice = leftHandDevices[0];
            }
        }

        // 读取左手摇杆的二维轴输入（primary2DAxis）
        if (leftHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 joystickValue))
        {
            // 计算摇杆的输入幅度，归一化值在0~1之间
            float normJoystick = Mathf.Clamp01(joystickValue.magnitude / maxJoystickMagnitude);

            // 使用线性插值计算当前HoleRadius
            // 当摇杆输入为0（normJoystick==0）时，HoleRadius为maxHoleRadius；当输入最大时，HoleRadius为minHoleRadius
            float currentHoleRadius = Mathf.Lerp(maxHoleRadius, minHoleRadius, normJoystick);

            // 将计算后的HoleRadius值传递给材质
            overlayMaterial.SetFloat("_HoleRadius", currentHoleRadius);
        }
    }
}
