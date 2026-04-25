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

    [Header("Checks")]
    public Transform groundCheck;
    public Vector2 groundCheckSize;

    [Space(10)]
    public Transform wallCheck;
    public Vector2 wallCheckSize;

    [Space(10)]
    public LayerMask groundLayer;
    public LayerMask wallLayer;

    Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        dashInputTimer = dashInputCooldown;
        wallJumpTimer = wallJumpCooldown;
        originalSpeed = moveSpeed;
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

        #region Run
        float targetSpeed = moveInput * moveSpeed;
        float speedDif = targetSpeed - rb.velocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);

        rb.AddForce(movement * Vector2.right);
        #endregion

        #region Sprinting
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl))
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }
        
        if (rb.velocity.x == 0f)
        {
            moveSpeed = originalSpeed;
            canDash = true;
        }

        if (isSprinting)
        {
            if (moveSpeed < sprintSpeed && rb.velocity.x != 0f)
            {
                moveSpeed += Time.deltaTime * speedAdded;
                canDash = false;
            }
            else return;
        }
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

        #region Gun Fire Input
        if (gun != null && Input.GetMouseButtonDown(0) && availableDashes > 0 && dashInputTimer <= 0f && canDash)
        {
            Dash();
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

    private void OnDrawGizmos()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
    }
}