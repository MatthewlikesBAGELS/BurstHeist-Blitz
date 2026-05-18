using UnityEngine;

public class ExtraDash : MonoBehaviour
{
    public Movement movement;

    [Header("Respawn")]
    public float cooldownTime = 5f;

    float cooldownTimer;
    bool collected = false;

    SpriteRenderer sr;
    Collider2D col;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        cooldownTimer = cooldownTime;
    }

    private void Update()
    {
        if (collected)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0f)
            {
                RespawnPickup();
            }
        }

        transform.Rotate(0, 0, 36f * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collected) return;

        if (!collision.CompareTag("Player")) return;

        if (movement.availableDashes < 3)
        {
            collected = true;

            movement.availableDashes++;

            CollectPickup();
        }
    }

    void CollectPickup()
    {
        collected = true;
        cooldownTimer = cooldownTime;

        sr.enabled = false;
        col.enabled = false;
    }

    void RespawnPickup()
    {
        collected = false;

        sr.enabled = true;
        col.enabled = true;
    }
}