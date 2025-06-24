// PlayerCombat.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private PlayerController player; // Référence au coordinateur

    [Header("Weapon & Combat")]
    public Weapon currentWeapon;
    public float lungeSpeed = 5f;

    private Transform intendedTarget;

    private bool isComboWindowOpen = false;
    private bool currentAttackHasHit = false;
    private AttackInputType? bufferedInput = null;
    private int lightAttackChainIndex = 0;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    // --- API PUBLIQUE (appelée par d'autres scripts) ---
    public void SetIntendedTarget(Transform target) { intendedTarget = target; }
    public void ReportSuccessfulHit() { currentAttackHasHit = true; }
    public void ResetHitConfirmation() { currentAttackHasHit = false; }
    public void ResetLightCombo()
    {
        lightAttackChainIndex = 0;
        Debug.LogWarning("!!! COMBO RESET !!! La chaîne d'attaques légères a été réinitialisée.", this.gameObject);
        Debug.Log(Environment.StackTrace); // Affiche la pile d'appels complète
    }

    // --- GESTION DES INPUTS (appelée par PlayerController) ---
    public void OnLightAttack() => HandleAttackInput(AttackInputType.Light);
    public void OnHeavyAttack() => HandleAttackInput(AttackInputType.Heavy);

    // --- GESTION DES ANIMATION EVENTS
    // Ces méthodes appellent l'arme qui se charge d'activer/désactiver sa propre hitbox.
    public void StartAttackHitbox() => currentWeapon?.EnableHitbox(intendedTarget);
    public void EndAttackHitbox() => currentWeapon?.DisableHitbox();

    // AttackFinished() appelée automatiquement dans AttackStateBehavior.cs
    public void AttackFinished()
    {
        // Avant de retourner à l'état neutre, on vérifie si un coup lourd a été demandé.
        if (bufferedInput.HasValue && bufferedInput.Value == AttackInputType.Heavy)
        {
            bufferedInput = null; // On consomme l'input
            ExecuteAttack(AttackInputType.Heavy, null); // On lance l'attaque lourde
        }
        else
        {
            Debug.Log("COMBOFINI");
            // Sinon, le combo est vraiment fini.
            player.ActionStateMachine.ChangeState(player.ReadyState);
            bufferedInput = null;
            ResetLightCombo();
        }
    }

    public void OpenComboWindow()
    {
        isComboWindowOpen = true;
        if (bufferedInput.HasValue)
        {
            HandleAttackInput(bufferedInput.Value);
            bufferedInput = null;
        }
    }

    public void CloseComboWindow()
    {
        if (isComboWindowOpen == false) return;

        isComboWindowOpen = false;
        bufferedInput = null;
        ResetLightCombo();
    }

    // --- LOGIQUE CENTRALE (Le Cerveau du Combat)

    // Point d'entrée unique pour gérer toutes les demandes d'attaque.
    private void HandleAttackInput(AttackInputType input)
    {
        if (currentWeapon == null) return;

        // --- CAS 1 : On continue un combo léger ---
        if (input == AttackInputType.Light && isComboWindowOpen && currentAttackHasHit)
        {
            var chain = currentWeapon.weaponData.lightAttackChain;
            if (chain.Count == 0) return;

            // On prend l'index de la PROCHAINE attaque pour connaître sa portée de ciblage
            int nextIndex = lightAttackChainIndex >= chain.Count ? 0 : lightAttackChainIndex;
            AttackProfile nextAttackProfile = chain[nextIndex];
            float targetingRange = currentWeapon.weaponData.GetTargetingRange(nextAttackProfile);

            // On trouve la cible avec la logique de redirection
            Vector3 moveDirection = player.MoveInput != Vector2.zero ? new Vector3(player.MoveInput.x, 0, player.MoveInput.y).normalized : player.transform.forward;
            Transform target = player.Targeting.FindComboTarget(targetingRange, moveDirection);

            ExecuteAttack(AttackInputType.Light, target);
            return;
        }

        // --- CAS 2 : On commence une nouvelle attaque depuis l'état neutre ---
        if (player.ActionStateMachine.CurrentState is PlayerReadyState)
        {
            Transform target = null;
            if (input == AttackInputType.Light)
            {
                ResetLightCombo();
                var chain = currentWeapon.weaponData.lightAttackChain;
                if (chain.Count == 0) return;
                // On prend la portée de la PREMIÈRE attaque de la chaîne
                AttackProfile firstAttackProfile = chain[0];
                float engagementRange = currentWeapon.weaponData.GetTargetingRange(firstAttackProfile);
                target = player.Targeting.FindInitialTarget(engagementRange);
            }
            ExecuteAttack(input, target);
            return;
        }
        // --- CAS 3 : On n'est dans aucune des situations ci-dessus, on bufférise l'input ---
        if (!isComboWindowOpen)
        {
            bufferedInput = input;
        }
    }

    // Exécute l'attaque demandée avec la cible fournie. Ne réfléchit plus, ne fait qu'agir.
    private void ExecuteAttack(AttackInputType input, Transform target)
    {
        AttackProfile attackToExecute = null;
        if (input == AttackInputType.Light)
        {
            var chain = currentWeapon.weaponData.lightAttackChain;
            if (lightAttackChainIndex >= chain.Count) lightAttackChainIndex = 0;
            attackToExecute = chain[lightAttackChainIndex];
        }
        else if (input == AttackInputType.Heavy)
        {
            attackToExecute = currentWeapon.weaponData.heavyAttackChain.FirstOrDefault();
        }

        if (attackToExecute == null) return;

        SetIntendedTarget(target);
        currentWeapon.SetCurrentAttack(attackToExecute);

        if (input == AttackInputType.Light)
        {
            Debug.Log("Executing attack n. " + lightAttackChainIndex);
            player.ChainAttackState.SetAttack(attackToExecute, target);
            player.ActionStateMachine.ChangeState(player.ChainAttackState);
            lightAttackChainIndex++;
        }
        else if (input == AttackInputType.Heavy)
        {
            player.ActionStateMachine.ChangeState(player.HeavyAttackState);
        }

        bufferedInput = null;
        isComboWindowOpen = false;
        ResetHitConfirmation();
    }
}