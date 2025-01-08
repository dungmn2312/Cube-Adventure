using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalPlatform : Platform
{
    //public delegate void LandOnPlatformHandler(NormalPlatform platform);
    //public static event LandOnPlatformHandler OnLandEnter, OnLandExit;
    public static event Action<NormalPlatform> OnLandEnter, OnLandExit;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnLandEnter?.Invoke(this);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnLandExit?.Invoke(this);
        }
    }
}
