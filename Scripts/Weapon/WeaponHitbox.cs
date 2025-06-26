// WeaponHitbox.cs
using UnityEngine;
using System.Collections.Generic;

public class WeaponHitbox : MonoBehaviour
{
    private PlayerCombat combatManager;
    private AttackProfile currentAttack;
    private Collider hitboxCollider;
    private List<Collider> hitEnemies = new List<Collider>();
    private Transform intendedTarget;

    private void Awake()
    {
        combatManager = GetComponentInParent<PlayerCombat>();
        if (combatManager == null)
        {
            Debug.LogError("WeaponHitbox ne trouve pas de PlayerCombat sur ses parents !");
        }
        hitboxCollider = GetComponent<Collider>();
        hitboxCollider.enabled = false; // La hitbox est désactivée par défaut
    }

    public void StartAttack(AttackProfile attack, Transform target)
    {
        currentAttack = attack;
        intendedTarget = target;
        hitEnemies.Clear();
        hitboxCollider.enabled = true;
    }

    public void EndAttack()
    {
        hitboxCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si ce n'est pas un ennemi ou qu'on l'a déjà touché, on ignore.
        if (!other.CompareTag("Enemy") || hitEnemies.Contains(other))
        {
            return;
        }

        if (currentAttack.hitType == HitType.SingleTarget)
        {
            // Si l'ennemi touché n'est pas notre cible voulue
            if (intendedTarget != null && other.transform != intendedTarget)
            {
                return;
            }

            Debug.Log("ON A HIT");
            hitEnemies.Add(other);
            combatManager?.ReportSuccessfulHit();

            // Logique de dégâts
            // On récupère le composant de vie de l'ennemi (à créer)
            // if (other.TryGetComponent<EnemyHealth>(out EnemyHealth enemyHealth))
            // {
            //     enemyHealth.TakeDamage(currentAttack.damage);
            // }

            //Debug.Log($"COUP VALIDE ! L'attaque '{currentAttack.profileName}' a heurté '{other.name}'.");
        }
    }
}