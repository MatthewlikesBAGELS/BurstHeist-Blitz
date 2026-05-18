using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ActivateTilemap : MonoBehaviour
{
    public GameObject tilemap;

    private void Start()
    {
        tilemap.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Invoke("Activate", 1);
        }
    }

    public void Deactivate()
    {
        if (tilemap != null)
        {
            tilemap.SetActive(false);
        }
    }

    public void Activate()
    {
        if (tilemap != null)
        {
            tilemap.SetActive(true);
        }
    }
}
