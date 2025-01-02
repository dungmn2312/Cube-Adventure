using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : Player
{
    private PlayerInputAction input;
    public Rigidbody2D playerRb;

    private Vector2 platformVelocity;

    //[SerializeField] private Transform wallCheckPos;
    private float weightGravity = 20;
    private float initialGravity;
    private Vector3 initialPos;
    private int initialDirection;
    private float deadTime = 1f;

    private bool movePressed = false;
    private int direction;
    private float targetSpeed;

    private float speedMultiplier = 0f;
    //private float wallCheckRange = 0.1f;
    [SerializeField] private LayerMask hitLayer;

    private void Awake()
    {
        input = new PlayerInputAction();

        playerRb = GetComponent<Rigidbody2D>();
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
    }

    private void Start()
    {
        SubcribeEvent();

        direction = transform.rotation.y == 0f ? 1 : -1;
        targetSpeed = speed;
        initialPos = transform.position;
        initialDirection = direction;
        initialGravity = playerRb.gravityScale;
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

    private void FixedUpdate()
    {
        UpdateSpeedMultiplier();
        if (speedMultiplier != 0f)
        {
            Move();
        }
    }

    private void Move()
    {
        playerRb.velocity = new Vector2(direction * speed * speedMultiplier, playerRb.velocity.y) + platformVelocity;
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
            Flip();
        }
    }

    //private bool CheckWallTouch()
    //{
    //    RaycastHit2D hit = Physics2D.Raycast(wallCheckPos.position, new Vector2(direction, 0f), wallCheckRange, hitLayer);
    //    //Debug.DrawRay(wallCheckPos.position, new Vector2(direction, 0f) * wallCheckRange, Color.red, 1f);
    //    return hit.collider;
    //}
}
