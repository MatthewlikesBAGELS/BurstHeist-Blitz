using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int currentHealth;
    public int maxHealth;
    public int damage;

    private void Update()
    {
        if (currentHealth <= 0)
        {
            this.gameObject.SetActive(false);
        }


    }
}
