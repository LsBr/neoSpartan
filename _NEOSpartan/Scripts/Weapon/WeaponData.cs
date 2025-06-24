using UnityEngine;
using System.Collections.Generic;

// On définit ici tous les types d'attaques possibles dans notre jeu.
// Correspond au "menu des commandes" du joueur
public enum AttackInputType { Light, Heavy }

public enum HitType { SingleTarget, MultiTarget }

// C'est le "profil" d'une seule attaque.
[System.Serializable]
public class AttackProfile
{
    public string profileName;
    public HitType hitType;
    public string animationTrigger; // Le nom du trigger à lancer dans l'Animator
    public int damage;
    [Tooltip("Portée de ciblage SPÉCIFIQUE à cette attaque. Laisser à 0 pour utiliser la valeur par défaut de l'arme.")]
    public float targetingRange; // Une valeur de 0 signifie "utiliser la valeur par défaut"
    [Tooltip("Allonge SPÉCIFIQUE à cette attaque (pour la décision de fente). Laisser à 0 pour utiliser la valeur par défaut de l'arme.")]
    public float weaponRange; // Une valeur de 0 signifie "utiliser la valeur par défaut"
}

// Notre ScriptableObject principal
[CreateAssetMenu(fileName = "New Weapon Data", menuName = "Game Data/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("Default Weapon Stats")]
    public float defaultTargetingRange = 3f; // NOUVEAU
    public float defaultWeaponRange = 2.4f; // NOUVEAU

    [Header("Attack Chains")]
    public List<AttackProfile> lightAttackChain;
    public List<AttackProfile> heavyAttackChain; // Même si elle n'a qu'un seul élément !

    /// Utilise la valeur du profil si elle est définie, sinon la valeur par défaut de l'arme.
    public float GetTargetingRange(AttackProfile profile)
    {
        // Si la portée du profil est supérieure à 0, on l'utilise.
        if (profile != null && profile.targetingRange > 0)
        {
            return profile.targetingRange;
        }
        // Sinon, on retourne la valeur par défaut de l'arme.
        return defaultTargetingRange;
    }

    // Retourne l'allonge effective pour un profil d'attaque donné.
    public float GetWeaponRange(AttackProfile profile)
    {
        if (profile != null && profile.weaponRange > 0)
        {
            return profile.weaponRange;
        }
        return defaultWeaponRange;
    }
}