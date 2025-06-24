// Weapon.cs
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponData weaponData;
    public WeaponHitbox hitbox;

    private AttackProfile currentAttack;

    // Méthode appelée par l'Animation Event (via PlayerCombat)
    public void EnableHitbox(Transform target)
    {
        hitbox.StartAttack(currentAttack, target); // On passe le profil de l'attaque en cours à la hitbox
    }

    // Méthode appelée par l'Animation Event (via PlayerCombat)
    public void DisableHitbox()
    {
        hitbox.EndAttack();
    }

    // Méthode appelée par PlayerCombat au début d'une attaque
    public void SetCurrentAttack(AttackProfile attack)
    {
        currentAttack = attack;
    }
}