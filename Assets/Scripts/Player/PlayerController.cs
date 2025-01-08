using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Player
{
    public static event Action OnPlayerMove, OnPlayerFall;
    public static event Action<Vector3, int> OnPlayerFlip;
    public static event Action<Vector3> OnPlayerDie;

    private PlayerInputAction input;
    private Rigidbody2D playerRb;

    private Animator animator;
    private int animInTeleport, animOutTeleport;

    private Vector2 platformVelocity;

    //[SerializeField] private Transform wallCheckPos;
    private float weightGravity = 20;
    private float initialGravity;
    private Vector3 initialPos;
    private int initialDirection;
    private float deadTime = 1.5f;

    private bool readyTele = true;

    private bool prevIsOnGround = true, isOnGround = true;
    [SerializeField] private Transform groundCheck;
    private float groundCheckRadius = 0.02f;
    [SerializeField] private LayerMask groundLayer;

    private bool movePressed = false;
    private int direction;
    private float targetSpeed;

    private float teleDuration = 1f;

    private float speedMultiplier = 0f;
    //private float wallCheckRange = 0.1f;
    private float counter = 0f;

    [SerializeField] private LayerMask hitLayer;
    
    [SerializeField] private float thresholdVelocity;

    [SerializeField] private float thresholdAmoutMoveParticle;

    private void Awake()
    {
        input = new PlayerInputAction();

        playerRb = GetComponent<Rigidbody2D>();

        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        input.Enable();

        input.Player.Move.performed += OnMove;
        input.Player.Move.canceled += _ => OnMoveCanceled();
    }

    private void OnDisable()
    {
        input.Disable();

        input.Player.Move.performed -= OnMove;
        input.Player.Move.canceled -= _ => OnMoveCanceled();
    }

    private void SubcribeEvent()
    {
        NormalPlatform.OnLandEnter += OnObserverLandEnter;
        NormalPlatform.OnLandExit += OnObserverLandExit;

        ObstacleController.OnDamagePlayer += OnObserverDamagePlayer;

        //TeleportController.OnInTeleport += OnObserverInTeleport;
    }

    private void Start()
    {
        SubcribeEvent();

        direction = transform.rotation.y == 0f ? 1 : -1;
        targetSpeed = speed;
        initialPos = transform.position;
        initialDirection = direction;
        initialGravity = playerRb.gravityScale;

        animInTeleport = Animator.StringToHash("in_teleport");
        animOutTeleport = Animator.StringToHash("out_teleport");
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        if (ctx.ReadValue<float>() == 1f) movePressed = true;
    }

    private void OnMoveCanceled()
    {
        movePressed = false;
    }

    private void OnObserverLandEnter(NormalPlatform platform)
    {
        Debug.Log("Enter");
        platformVelocity = new Vector2(platform.speed, platform.platformRb.velocity.y);
        playerRb.gravityScale = weightGravity;
    }

    private void OnObserverLandExit(NormalPlatform platform)
    {
        Debug.Log("Exit");
        platformVelocity = Vector2.zero;
        playerRb.gravityScale = initialGravity;
    }

    private void OnObserverDamagePlayer()
    {
        Die();
    }

    //private void OnObserverInTeleport(Vector3 gateA, Vector3 gateB)
    //{
    //    PlayerAnimationController.Instance.PlayAnimInTeleport();
    //    transform.DOMove(gateA, teleDuration)
    //        .SetEase(Ease.Linear);
    //        //.OnComplete(() =>
    //        //{
    //        //    transform.position = gateB;
    //        //    PlayerAnimationController.Instance.PlayAnimOutTeleport();
    //        //    transform.DOMove(gateB + new Vector3(di, 0, 0), teleDuration)
    //        //        .SetEase(Ease.Linear);
    //        //});
    //}

    private void Update()
    {
        CheckVelocity();
    }

    private void FixedUpdate()
    {
        CheckGround();
        UpdateSpeedMultiplier();
        if (speedMultiplier != 0f)
        {
            Move();
        }
        else
        {
            if (playerRb.velocity.x != 0f && !isOnGround) playerRb.velocity = new Vector2(0f, playerRb.velocity.y);
        }
    }

    private void Move()
    {
        playerRb.velocity = new Vector2(direction * speed * speedMultiplier, playerRb.velocity.y) + platformVelocity;
    }

    private void CheckVelocity()
    {
        counter += Time.deltaTime;
        if (Mathf.Abs(playerRb.velocity.x) > thresholdVelocity)
        {
            if (counter > thresholdAmoutMoveParticle && isOnGround)
            {
                OnPlayerMove?.Invoke();
                counter = 0;
            }
        }
    }

    private void CheckGround()
    {
        isOnGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isOnGround == true && prevIsOnGround == false)
        {
            OnPlayerFall?.Invoke();
        }
        prevIsOnGround = isOnGround;
    }

    private void UpdateSpeedMultiplier()
    {
        if (speedMultiplier < 1f && movePressed)
        {
            speedMultiplier += 1.5f * Time.fixedDeltaTime;
            if (speedMultiplier > 1f) speedMultiplier = 1f;
        }
        else if (speedMultiplier > 0f && !movePressed)
        {
            speedMultiplier -= 1.5f * Time.fixedDeltaTime;
            if (speedMultiplier < 0f) speedMultiplier = 0f;
        }
    }

    private void Flip()
    {
        transform.Rotate(0, 180, 0);
        direction *= -1;
    }

    private async void Die()
    {
        OnPlayerDie?.Invoke(transform.position);
        gameObject.SetActive(false);
        speedMultiplier = 0f;
        transform.position = initialPos;

        await UniTask.WaitForSeconds(deadTime);

        if (initialDirection != direction)
        {
            Flip();
        }
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            OnPlayerFlip?.Invoke(transform.position, direction);
            Flip();
        }
        if (collision.CompareTag("Teleport") && readyTele)
        {
            input.Disable();
            playerRb.velocity = Vector3.zero;
            readyTele = false;

            GameObject teleportIn = collision.gameObject;
            Transform teleportOut = teleportIn.GetComponent<TeleportController>().otherGate;
            EntryTeleport(teleportIn.transform, teleportOut.transform);
        }
    }

    private void EntryTeleport(Transform teleportIn, Transform teleportOut)
    {
        animator.SetTrigger(animInTeleport);
        transform.DOMove(teleportIn.position, teleDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                Debug.Log(teleportOut.right.normalized.x);
                animator.SetTrigger(animOutTeleport);
                if (direction * teleportOut.right.normalized.x < 0)    Flip();
                transform.position = teleportOut.position;
                transform.DOMove(teleportOut.position + new Vector3(teleportOut.right.normalized.x, 0, 0), teleDuration)
                    .SetEase(Ease.Linear)
                    .OnComplete(() =>
                    {
                        readyTele = true;
                        input.Enable();
                    });
            });
    }

    //private bool CheckWallTouch()
    //{
    //    RaycastHit2D hit = Physics2D.Raycast(wallCheckPos.position, new Vector2(direction, 0f), wallCheckRange, hitLayer);
    //    //Debug.DrawRay(wallCheckPos.position, new Vector2(direction, 0f) * wallCheckRange, Color.red, 1f);
    //    return hit.collider;
    //}
}
