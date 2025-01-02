using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    public delegate void DamagePlayerHandler();
    public static event DamagePlayerHandler OnDamagePlayer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnDamagePlayer?.Invoke();
        }
    }
}
