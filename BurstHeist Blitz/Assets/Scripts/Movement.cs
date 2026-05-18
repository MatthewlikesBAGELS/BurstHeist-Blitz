using Unity.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class Movement : MonoBehaviour
{
    [Header("Run")]
    public float moveSpeed;
    public float acceleration;
    public float decceleration;
    public float velPower;

    [Space(10)]
    public float frictionAmount;

    [Header("Sprint")]
    [Tooltip("Must be HIGHER than Move Speed")]
    public float sprintSpeed;
    [Tooltip("Speed to add to Move Speed every second")]
    public float speedAdded;

    float originalSpeed;
    bool isSprinting = false;

    [Header("Jump")]
    public float jumpForce;
    [Range(0, 1)]
    public float jumpCutMultiplier;

    [Space(10)]
    public float jumpBufferTime;
    public float coyoteTime;

    [Space(10)]
    public float gravityScale;
    public float fallGravityMultiplier;

    float lastGroundedTime;
    float lastJumpTime;
    bool isJumping = false;

    [Header("Wall Jump")]
    public float wallJumpForce;

    [Range(0, 1)]
    public float wallSlideMultiplier;
    public float wallJumpCooldown;

    public Vector2 wallJumpDirection;

    float wallJumpTimer;
    bool canWallJump = false;


    [Header("Gun")]
    public LookAtCursor gun;

    [Space(10)]
    public float gunDashForce;

    [Range(0, 5)]
    public int availableDashes;

    [Range(0, 5)]
    public int maxDashes;

    public float dashInputCooldown = 0.15f;
    float dashInputTimer;
    bool canDash = false;

    [Space(10)]
    public float shootCooldown = 0.5f;
    float shootTimer = 0f;

    [Space(10)]
    public bool isAiming = false;

    [Header("Checks")]
    public Transform groundCheck;
    public Vector2 groundCheckSize;

    [Space(10)]
    public Transform wallCheck;
    public Vector2 wallCheckSize;

    [Space(10)]
    public LayerMask groundLayer;
    public LayerMask wallLayer;

    [Space(10)]
    public GameObject bulletUI;
    public GameObject bulletUIExtra;
    public Sprite[] bulletCountSprites;
    public Sprite[] bulletCountExtraSprites;

    [Space(10)]
    public FollowCursor followCursor;

    [Header("Super Friggin' Cool Combos!")]
    public int comboCountForExtraDash = 5;
    public int comboMultiplierForEveryBullet = 2;
    int currentComboCount;

    Rigidbody2D rb;
    SpriteRenderer bulletCountSR;
    SpriteRenderer bulletCountExtraSR;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        dashInputTimer = dashInputCooldown;
        wallJumpTimer = wallJumpCooldown;

        originalSpeed = moveSpeed;
        moveSpeed = originalSpeed;

        bulletCountSR = bulletUI.GetComponent<SpriteRenderer>();
        bulletCountExtraSR = bulletUIExtra.GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        #region Timers
        lastGroundedTime -= Time.fixedDeltaTime;
        lastJumpTime -= Time.fixedDeltaTime;
        dashInputTimer -= Time.fixedDeltaTime;
        wallJumpTimer -= Time.fixedDeltaTime;
        #endregion
    }

    private void Update()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput > 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }

        if (moveInput < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        bulletUI.transform.rotation = Quaternion.Euler(0, 0, 0);
        bulletUI.transform.position = new Vector3(transform.position.x - 1.25f, transform.position.y + 0.25f, transform.position.z);

        if (availableDashes < 4)
            bulletCountSR.sprite = bulletCountSprites[availableDashes];
        
        if(availableDashes > 3)
        {
            bulletUIExtra.SetActive(true);
            bulletCountExtraSR.sprite = bulletCountExtraSprites[availableDashes - 4];
        }
        else
        {
            bulletUIExtra.SetActive(false);
        }

        #region Run
            float targetSpeed = moveInput * moveSpeed;
        float speedDif = targetSpeed - rb.velocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

        rb.AddForce(movement * Vector2.right);
        #endregion

        #region Sprinting

        isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl);

        if (Mathf.Abs(rb.velocity.x) < 0.1f)
        {
            canDash = true;
        }

        float targetMoveSpeed = originalSpeed;

        if (isSprinting && Mathf.Abs(moveInput) > 0.01f)
        {
            targetMoveSpeed = sprintSpeed;
            canDash = false;
        }

        moveSpeed = Mathf.MoveTowards(moveSpeed, targetMoveSpeed, speedAdded * Time.deltaTime);

        #endregion

        #region Friction
        if (lastGroundedTime > 0 && Mathf.Abs(moveInput) < 0.01f)
        {
            float amount = Mathf.Min(Mathf.Abs(rb.velocity.x), Mathf.Abs(frictionAmount));
            amount *= Mathf.Sign(rb.velocity.x);
            rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }
        #endregion

        #region Ground Check
        if (Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, groundLayer))
        {
            lastGroundedTime = coyoteTime;
            isJumping = false;
        }
        #endregion

        #region Wall Check
        if (Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0, wallLayer) && lastGroundedTime <= 0 && wallJumpTimer <= 0)
        {
            canWallJump = true;
            if(rb.velocity.y < 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * wallSlideMultiplier);
            }
        }
        else
        {
            canWallJump = false;
            rb.gravityScale = gravityScale;
        }
        #endregion

        #region Jump Input
        if (Input.GetButtonDown("Jump"))
        {
            if (canWallJump)
                WallJumpUp();
            else
                OnJump();
        }

        if (Input.GetButtonUp("Jump"))
        {
            OnJumpUp();
        }
        #endregion

        #region Jump Buffer
        if (lastGroundedTime > 0 && lastJumpTime > 0 && !isJumping)
        {
            Jump();
        }
        #endregion

        #region Jump Gravity
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = gravityScale * fallGravityMultiplier;
        }
        else
        {
            rb.gravityScale = gravityScale;
        }
        #endregion

        #region Aiming Toggle
        if (Input.GetMouseButtonDown(1))
        {
            isAiming = !isAiming;
            followCursor.gameObject.SetActive(isAiming);
        }
        #endregion

        #region Shoot Timer
        shootTimer -= Time.deltaTime;
        #endregion

        #region Left Click Action
        if (Input.GetMouseButtonDown(0))
        {
            if (isAiming)
            {
                if (shootTimer <= 0f)
                {
                    Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);

                    if (hit != null && hit.CompareTag("Enemy"))
                    {
                        Shoot(hit);
                        shootTimer = shootCooldown;
                    }
                }
            }
            else
            {
                if (gun != null && availableDashes > 0 && dashInputTimer <= 0f && canDash)
                {
                    Dash();
                }
            }
        }
        #endregion
    }

    public void Jump()
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        lastGroundedTime = 0;
        lastJumpTime = 0;
        isJumping = true;
    }

    public void OnJump()
    {
        lastJumpTime = jumpBufferTime;
    }

    public void OnJumpUp()
    {
        if (rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
        }

        if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.gravityScale = gravityScale * fallGravityMultiplier;
        }

        lastJumpTime = 0;
    }

    public void WallJumpUp()
    {
        rb.velocity = Vector2.zero;
        rb.gravityScale = gravityScale;

        if(transform.rotation == Quaternion.Euler(0, 180, 0))
            rb.AddForce(new Vector2(wallJumpDirection.x * wallJumpForce * -1, wallJumpDirection.y * wallJumpForce), ForceMode2D.Impulse);
        else
            rb.AddForce(wallJumpDirection * wallJumpForce, ForceMode2D.Impulse);

        wallJumpTimer = wallJumpCooldown;
    }

    public void Dash()
    {
        rb.velocity = Vector2.zero;
        rb.velocity = -gun.rawDirectionRef * gunDashForce;
        dashInputTimer = dashInputCooldown;
        availableDashes--;
    }

    public void Shoot(Collider2D enemyCollider)
    {

    }

    private void OnDrawGizmos()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
    }
}