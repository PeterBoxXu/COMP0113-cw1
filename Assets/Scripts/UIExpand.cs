using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIExpand : MonoBehaviour
{
    public RectTransform panel;
    public float expandSpeed = 2.0f;
    private float targetHeight; // 目标高度
    private bool isExpanded = false; // 记录 UI 是否展开

    void Start()
    {
        targetHeight = panel.sizeDelta.y; // 记录原始高度
        panel.sizeDelta = new Vector2(panel.sizeDelta.x, 0); // 初始化高度为0
    }

    public void ToggleExpand()
    {
        StopAllCoroutines(); // 防止短时间内重复调用导致动画冲突
        if (isExpanded)
            StartCoroutine(CollapseUI());
        else
            StartCoroutine(ExpandUI());
        isExpanded = !isExpanded;
    }

    IEnumerator ExpandUI()
    {
        float elapsedTime = 0f;
        float startHeight = 0f;
        float startPosY = panel.anchoredPosition.y; // 记录初始位置
        float targetPosY = startPosY - targetHeight / 2; // 目标位置调整

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * expandSpeed;
            float t = Mathf.SmoothStep(0, 1, elapsedTime); // 平滑加速动画
            float newHeight = Mathf.Lerp(startHeight, targetHeight, t);
            float newPosY = Mathf.Lerp(startPosY, targetPosY, t);
            panel.sizeDelta = new Vector2(panel.sizeDelta.x, newHeight);
            panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, newPosY);
            yield return null;
        }
    }

    IEnumerator CollapseUI()
    {
        float elapsedTime = 0f;
        float startHeight = panel.sizeDelta.y;
        float startPosY = panel.anchoredPosition.y;
        float targetPosY = startPosY + startHeight / 2; // 回到收缩状态的中心

        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * expandSpeed;
            float t = Mathf.SmoothStep(0, 1, elapsedTime);
            float newHeight = Mathf.Lerp(startHeight, 0, t);
            float newPosY = Mathf.Lerp(startPosY, targetPosY, t);
            panel.sizeDelta = new Vector2(panel.sizeDelta.x, newHeight);
            panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, newPosY);
            yield return null;
        }
    }
}
