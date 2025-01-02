using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;

    [Range(0, 1)]
    public float smooth;

    private Vector3 velocity = Vector3.zero;

    public Vector3 posOffset;

    [Header("Axis Limitation")]
    public Vector2 xLimit;
    public Vector2 yLimit;

    private Vector3 targetPos;

    private void LateUpdate()
    {
        MoveCamera(playerTransform.position);
    }

    public void MoveCamera(Vector3 playerPos)
    {
        targetPos = playerPos + posOffset;
        targetPos = new Vector3(Mathf.Clamp(targetPos.x, xLimit.x, xLimit.y),
                                Mathf.Clamp(targetPos.y, yLimit.x, yLimit.y),
                                -10);

        //transform.DOMove(targetPos, smooth).SetEase(Ease.InOutQuad);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smooth);
    }
}
