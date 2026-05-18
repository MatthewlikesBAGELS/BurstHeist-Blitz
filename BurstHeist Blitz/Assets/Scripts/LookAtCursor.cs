using UnityEngine;

public class LookAtCursor : MonoBehaviour
{
    public Camera mainCam;
    public Transform player;
    public float maxDistance = 2f;
    public float rotationSpeed = 12f;

    [HideInInspector] public Vector3 rawDirectionRef;
    Movement playerMovement;

    private float currentAngle;

    private void Start()
    {
        currentAngle = transform.eulerAngles.z;
        playerMovement = player.gameObject.GetComponent<Movement>();
    }

    private void Update()
    {
        Vector3 mouseWorldPosition = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0f;

        Vector3 rawDirection = (mouseWorldPosition - player.position).normalized;

        rawDirection = -rawDirection;
        rawDirectionRef = rawDirection;

        float targetAngle = Mathf.Atan2(rawDirection.y, rawDirection.x) * Mathf.Rad2Deg;
        targetAngle = Mathf.Round(targetAngle / 45f) * 45f;

        currentAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * rotationSpeed);

        Vector3 direction = new Vector3(
            Mathf.Cos(currentAngle * Mathf.Deg2Rad),
            Mathf.Sin(currentAngle * Mathf.Deg2Rad),
            0f
        );

        if (!playerMovement.isAiming)
        {
            transform.position = player.position + direction * maxDistance;

            transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
        }
        else if (playerMovement.isAiming)
        {
            transform.position = player.position + direction * maxDistance * -1f;

            transform.rotation = Quaternion.Euler(180f, 180f, currentAngle);
        }

    }
}