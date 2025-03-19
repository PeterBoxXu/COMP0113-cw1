using UnityEngine;
using UnityEngine.EventSystems;

public class WorldSpaceUIHover : MonoBehaviour
{
    private Vector3 originalScale;
    private Vector3 targetScale;
    public float scaleFactor = 1.2f;
    public float speed = 10f;

    private void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        // 平滑缩放
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * speed);

        // 检测鼠标是否悬停在 UI Image 上
        if (IsMouseOverUI())
        {
            targetScale = originalScale * scaleFactor;
        }
        else
        {
            targetScale = originalScale;
        }
    }

    private bool IsMouseOverUI()
    {
        // 创建 UI 射线
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        // 存储射线结果
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        // 如果射线命中了当前对象，返回 true
        foreach (var result in results)
        {
            if (result.gameObject == gameObject)
            {
                return true;
            }
        }
        return false;
    }
}