// PlayerCombat.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private PlayerController player; // Référence au coordinateur

    [Header("Weapon & Combat")]
    public Weapon currentWeapon;
    public float lungeSpeed = 5f;

    public float comboTransitionTime = 0.15f;

    private Transform intendedTarget;
    private string currentAttackAnimationName = "";

    private bool currentAttackHasHit = false;
    private AttackInputType? bufferedInput = null;
    private int lightAttackChainIndex = 0;
    private bool isAttacking = false;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    // --- API PUBLIQUE (appelée par d'autres scripts) ---
    public void SetIntendedTarget(Transform target) => intendedTarget = target;
    // Appelée par WeaponHitbox quand on touche un ennemi
    public void ReportSuccessfulHit() => currentAttackHasHit = true;

    // --- GESTION DES INPUTS (appelée par PlayerController) ---
    public void OnLightAttack() => HandleAttackInput(AttackInputType.Light);
    public void OnHeavyAttack() => HandleAttackInput(AttackInputType.Heavy);

    // --- GESTION DES ANIMATION EVENTS
    // Ces méthodes appellent l'arme qui se charge d'activer/désactiver sa propre hitbox.
    public void StartAttackHitbox() => currentWeapon?.EnableHitbox(intendedTarget);
    public void EndAttackHitbox() => currentWeapon?.DisableHitbox();

    public void CloseComboWindow()
    {
        // Si on a un input en buffer, on l'exécute immédiatement
        if (bufferedInput == AttackInputType.Light && currentAttackHasHit && isAttacking)
        {
            ContinueLightCombo();
        }
        else
        {
            bufferedInput = null;
        }
    }

    // AttackFinished() appelée en event à la fin des animations
    public void AttackFinished()
    {
        AnimatorStateInfo stateInfo = player.Animator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName(currentAttackAnimationName))
            return;

        ResetCombat();
    }

    // Point d'entrée unique pour gérer toutes les demandes d'attaque.
    private void HandleAttackInput(AttackInputType input)
    {
        if (currentWeapon == null) return;

        // --- CAS 1 : Nouvelle attaque depuis l'état neutre
        if (!isAttacking || player.ActionStateMachine.CurrentState is PlayerReadyState)
        {
            StartNewAttack(input);
        }
        // --- CAS 2 : Buffer l'input pendant une attaque
        else
        {
            bufferedInput = input;
        }
    }

    private void StartNewAttack(AttackInputType input)
    {
        if (input == AttackInputType.Light)
        {
            ExecuteLightAttack();
        }
        else if (input == AttackInputType.Heavy)
        {
            ExecuteHeavyAttack();
        }
    }

    private void ExecuteLightAttack()
    {
        lightAttackChainIndex = 0;
        List<AttackProfile> chain = currentWeapon.weaponData.lightAttackChain;
        if (chain.Count == 0) return;

        AttackProfile attack = chain[0];
        float range = currentWeapon.weaponData.GetTargetingRange(attack);
        Transform target = player.Targeting.FindInitialTarget(range);

        PrepareAttack(target);
        lightAttackChainIndex++;

        currentAttackAnimationName = attack.animationStateName;

        // Changer d'état
        player.ChainAttackState.PrepareAttack(attack, target);
        player.ActionStateMachine.ChangeState(player.ChainAttackState);
    }

    private void ExecuteHeavyAttack()
    {
        var heavyAttack = currentWeapon.weaponData.heavyAttackChain.FirstOrDefault();
        if (heavyAttack == null) return;

        currentAttackAnimationName = heavyAttack.animationStateName;

        player.ActionStateMachine.ChangeState(player.HeavyAttackState);
    }

    private void ContinueLightCombo()
    {
        List<AttackProfile> chain = currentWeapon.weaponData.lightAttackChain;
        if (chain.Count == 0) return;

        int nextIndex = lightAttackChainIndex >= chain.Count ? 0 : lightAttackChainIndex;
        AttackProfile nextAttack = chain[nextIndex];

        float range = currentWeapon.weaponData.GetTargetingRange(nextAttack);
        Transform target = player.Targeting.FindComboTarget(range, player.MoveInput);

        player.Animator.CrossFade(nextAttack.animationStateName, comboTransitionTime);

        // Préparer la prochaine attaque
        PrepareAttack(target);
        lightAttackChainIndex++;

        currentAttackAnimationName = nextAttack.animationStateName;

        // Notifier la state machine si nécessaire
        if (player.ActionStateMachine.CurrentState is PlayerChainAttackState chainState)
        {
            chainState.ChainAttack(nextAttack, target);
        }
    }

    private void PrepareAttack(Transform target)
    {
        intendedTarget = target;
        currentAttackHasHit = false;
        bufferedInput = null;
        isAttacking = true;
    }

    private void ResetCombat()
    {
        isAttacking = false;
        currentAttackHasHit = false;
        bufferedInput = null;
        intendedTarget = null;
        lightAttackChainIndex = 0;
        currentAttackAnimationName = "";

        player.ActionStateMachine.ChangeState(player.ReadyState);
    }
}


