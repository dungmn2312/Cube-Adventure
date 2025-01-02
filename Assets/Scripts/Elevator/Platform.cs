using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public List<Transform> posList;
    internal Rigidbody2D platformRb;

    private int posIndex = 0;
    internal float speed = 2f;
    private Vector3 direction;
    private int posLength;
    private float threshold = 0.05f;

    private float waitTime = 0.5f;

    private void Awake()
    {
        platformRb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        posLength = posList.Count;
        CaculateDirection();
    }

    //private void Update()
    //{
        
    //}

    private void FixedUpdate()
    {
        Move();
    }

    private async void Move()
    {
        platformRb.velocity = speed * direction;

        if (Vector2.Distance(transform.position, posList[posIndex].position) <= threshold)
        {
            platformRb.MovePosition(posList[posIndex].position);
            CaculatePos();
            direction = Vector3.zero;

            await UniTask.WaitForSeconds(waitTime);

            CaculateDirection();
        }
    }

    private void CaculatePos()
    {
        posIndex = (posIndex + 1) % posLength;
    }

    private void CaculateDirection()
    {
        direction = (posList[posIndex].position - transform.position).normalized;
    }
}