using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    public ParticleSystem moveParticle;
    public ParticleSystem fallParticle;
    public ParticleSystem touchParticle;
    public ParticleSystem dieParticle;

    private Vector3 flipPosOffset = new Vector3(0.4f, 0, 0);

    private void OnEnable()
    {
        PlayerController.OnPlayerMove += OnObserverPlayerMove;
        PlayerController.OnPlayerFall += OnObserverPlayerFall;
        PlayerController.OnPlayerFlip += OnObserverPlayerFlip;
        PlayerController.OnPlayerDie += OnObserverPlayerDie;
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerMove -= OnObserverPlayerMove;
        PlayerController.OnPlayerFall -= OnObserverPlayerFall;
        PlayerController.OnPlayerFlip -= OnObserverPlayerFlip;
        PlayerController.OnPlayerDie -= OnObserverPlayerDie;
    }

    private void OnObserverPlayerMove()
    {
        moveParticle.Play();
    }

    private void OnObserverPlayerFall()
    {
        fallParticle.Play();
    }

    private void OnObserverPlayerFlip(Vector3 effectPos, int direction)
    {
        touchParticle.transform.position = effectPos + flipPosOffset * direction;
        touchParticle.Play();
    }

    private void OnObserverPlayerDie(Vector3 effectPos)
    {
        dieParticle.transform.position = effectPos;
        dieParticle.Play();
    }
}