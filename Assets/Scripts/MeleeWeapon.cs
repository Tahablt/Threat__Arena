using UnityEngine;
using System.Collections;

public class MeleeWeapon : MonoBehaviour
{
    [Header("Kilic Ayarlari")]
    public float damage = 25f;
    public bool isAttacking = false;

    private System.Collections.Generic.HashSet<IDamageable> hitThisSwing = new System.Collections.Generic.HashSet<IDamageable>();

    private void OnTriggerEnter(Collider other)
    {
        if (!isAttacking) return;

        if (other.TryGetComponent(out IDamageable hitTarget))
        {
            if (!hitThisSwing.Contains(hitTarget))
            {
                hitTarget.TakeDamage(damage);
                hitThisSwing.Add(hitTarget);
            }
        }
    }

    public void PerformAttack()
    {
        if (!isAttacking)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        hitThisSwing.Clear();

        yield return new WaitForSeconds(0.5f);

        isAttacking = false;
    }
}
