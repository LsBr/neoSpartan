using UnityEngine;
using System.Collections;
using System.Linq;

public class PlayerCombat : MonoBehaviour
{
    private PlayerController player;

    [Header("Weapon & Combat Stats")]
    public Weapon currentWeapon;
    public float lungeSpeed = 15f;
    [Tooltip("La durée en secondes de la fenêtre de tolérance après la fin d'une animation d'attaque réussie.")]
    public float comboLeniency = 0.2f;

    // Variables d'état internes pour la gestion des combos
    private int lightAttackChainIndex = 0;
    private AttackInputType? bufferedInput = null;
    private bool currentAttackHasHit = false;
    private Coroutine comboResetCoroutine;
    private Transform intendedTarget;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    #region --- Gestion des Inputs (Appelé par PlayerController) ---

    public void OnLightAttack() => HandleAttackInput(AttackInputType.Light);
    public void OnHeavyAttack() => HandleAttackInput(AttackInputType.Heavy);

    private void HandleAttackInput(AttackInputType input)
    {
        if (currentWeapon == null) return;

        // Si on est déjà dans un état d'attaque, on met simplement l'input en mémoire tampon.
        if (player.ActionStateMachine.CurrentState is not PlayerReadyState)
        {
            bufferedInput = input;
            return;
        }

        // Si on est prêt, on lance directement une nouvelle chaîne d'attaques.
        ExecuteAttack(input, null);
    }

    #endregion

    #region --- Logique de Combo (Le Cerveau) ---

    private void ExecuteAttack(AttackInputType input, Transform target)
    {
        // On a lancé une nouvelle attaque, on annule donc tout timer de reset de combo.
        if (comboResetCoroutine != null)
        {
            StopCoroutine(comboResetCoroutine);
        }

        bufferedInput = null;
        currentAttackHasHit = false;

        AttackProfile attackToExecute = null;
        if (input == AttackInputType.Light)
        {
            var chain = currentWeapon.weaponData.lightAttackChain;
            if (lightAttackChainIndex >= chain.Count)
            {
                ResetLightCombo(); // Si on est au bout de la chaîne, on recommence.
            }
            attackToExecute = chain[lightAttackChainIndex];
        }
        else if (input == AttackInputType.Heavy)
        {
            attackToExecute = currentWeapon.weaponData.heavyAttackChain.FirstOrDefault();
            ResetLightCombo(); // Une attaque lourde brise la chaîne d'attaques légères.
        }

        if (attackToExecute == null) { ResetAndGoToReadyState(); return; }

        // Si aucune cible n'a été fournie, on en cherche une.
        if (target == null)
        {
            float range = currentWeapon.weaponData.GetTargetingRange(attackToExecute);
            // On se base sur la direction du stick si le joueur bouge, sinon sur la direction du personnage.
            Vector3 desiredDirection = player.MoveInput.magnitude > 0.1f ? new Vector3(player.MoveInput.x, 0, player.MoveInput.y) : player.transform.forward;
            target = player.Targeting.FindComboTarget(range, desiredDirection);
        }

        SetIntendedTarget(target);
        currentWeapon.SetCurrentAttack(attackToExecute);

        // On passe à l'état d'attaque correspondant
        if (input == AttackInputType.Light)
        {
            player.ChainAttackState.SetAttack(attackToExecute, target);
            player.ActionStateMachine.ChangeState(player.ChainAttackState);
            lightAttackChainIndex++; // On prépare l'index pour le PROCHAIN coup de la chaîne.
        }
        else if (input == AttackInputType.Heavy)
        {
            player.ActionStateMachine.ChangeState(player.HeavyAttackState);
        }
    }

    // Appelée par la WeaponHitbox quand un coup touche. C'est le coeur du Hit-Confirm.
    public void ReportSuccessfulHit()
    {
        if (currentAttackHasHit) return;

        currentAttackHasHit = true;
        Debug.Log("HIT CONFIRMED!");

        // Si un coup est en attente dans le buffer, on l'exécute IMMÉDIATEMENT.
        if (bufferedInput.HasValue)
        {
            ExecuteAttack(bufferedInput.Value, null);
        }
    }

    #endregion

    #region --- Événements d'Animation & Reset ---

    // Appelée par un Animation Event à la fin de chaque animation d'attaque.
    public void OnAnimationFinished()
    {
        // On annule tout timer qui aurait pu être lancé par une attaque précédente
        if (comboResetCoroutine != null) StopCoroutine(comboResetCoroutine);

        // CAS 1 : Le coup a raté. Le combo est brisé.
        if (!currentAttackHasHit)
        {
            ResetAndGoToReadyState();
            return;
        }

        // CAS 2 : Le coup a touché, mais le joueur n'a pas encore enchaîné via ReportSuccessfulHit.
        // On lui donne une dernière chance en lançant le timer de tolérance.
        comboResetCoroutine = StartCoroutine(ResetComboAfterDelay(comboLeniency));
    }

    // Le timer de la fenêtre de tolérance.
    private IEnumerator ResetComboAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetAndGoToReadyState();
    }

    // La fonction de nettoyage finale pour retourner à l'état neutre.
    private void ResetAndGoToReadyState()
    {
        bufferedInput = null;
        player.ActionStateMachine.ChangeState(player.ReadyState);
        ResetLightCombo();
    }

    public void ResetLightCombo()
    {
        lightAttackChainIndex = 0;
    }

    public void SetIntendedTarget(Transform target)
    {
        intendedTarget = target;
    }

    // Fonctions appelées par les Animation Events pour la hitbox
    public void StartAttackHitbox() => currentWeapon?.EnableHitbox(intendedTarget);
    public void EndAttackHitbox() => currentWeapon?.DisableHitbox();

    #endregion
}
