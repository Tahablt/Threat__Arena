using UnityEngine;
using System.Collections.Generic;

public class VFXDamage : MonoBehaviour
{
    private float currentDamage;

    private List<Collider> alreadyHit = new List<Collider>();

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Character characterScript = player.GetComponent<Character>();
            if (characterScript != null)
            {
                currentDamage = characterScript.attackDamage;

                // --- YENİ EKLENEN KISIM: Kılıç boyutunu karakterden çek ve büyüt ---
                float mult = characterScript.vfxScaleMultiplier;
                transform.localScale = transform.localScale * mult;
                // -------------------------------------------------------------------
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy == null)
            {
                enemy = other.GetComponentInParent<Enemy>();
            }

            if (enemy != null)
            {
                if (!alreadyHit.Contains(other))
                {
                    enemy.TakeDamage(currentDamage);
                    alreadyHit.Add(other);
                }
            }
        }
    }
}