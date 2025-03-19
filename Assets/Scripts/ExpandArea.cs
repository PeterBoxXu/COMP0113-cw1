using System;
using Ubiq.Avatars;
using UnityEngine;

public class ExpandArea : MonoBehaviour
{
    public BoxCollider boxCollider;
    private GameObject avatar;
    private Transform t;

    private void Awake()
    {
        t = avatar.transform;

        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider>();
        }
    }

    void Update()
    {

        if (boxCollider.bounds.Contains(t.position))
        {
            Debug.Log("玩家在 Box Collider 内部");
        }
        else
        {
            Debug.Log("玩家在 Box Collider 外部: " + t.position);
        }
    }
}