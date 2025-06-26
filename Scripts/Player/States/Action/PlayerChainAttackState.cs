using UnityEngine;

public class PlayerChainAttackState : State
{
    private AttackProfile currentAttackProfile;
    // Variables pour la logique interne de cet état
    private bool isLunging;
    private bool isFirstAttack;
    private Transform currentTarget;

    public PlayerChainAttackState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine)
    {
    }

    // Appelé dans PlayerCombat
    public void PrepareAttack(AttackProfile profile, Transform target)
    {
        currentAttackProfile = profile;
        currentTarget = target;
        player.Combat.currentWeapon.SetCurrentAttack(profile);
        isFirstAttack = true;
    }

    public void ChainAttack(AttackProfile profile, Transform target)
    {
        PrepareAttack(profile, target);
        isFirstAttack = false;
    }

    public override void Enter()
    {
        ExecuteAttackLogic();
    }

    private void ExecuteAttackLogic()
    {
        if (currentAttackProfile == null)
        {
            stateMachine.ChangeState(player.ReadyState); // Sécurité
            return;
        }

        isLunging = false;
        if (currentTarget != null)
        {
            // On décide si on doit faire une fente
            float effectiveWeaponRange = player.Combat.currentWeapon.weaponData.GetWeaponRange(currentAttackProfile);
            float distance = Vector3.Distance(player.transform.position, currentTarget.position);
            if (distance > effectiveWeaponRange)
            {
                isLunging = true;
            }
        }
        if (isFirstAttack)
        {
            // Option 1: Play immédiat (pas de blend)$
            Debug.Log("Play " + currentAttackProfile.animationStateName);
            player.Animator.Play(currentAttackProfile.animationStateName);

            // Option 2: CrossFade pour une transition smooth
            // player.Animator.CrossFade(currentAttackProfile.animationName, 0.1f);
        }
    }

    public override void LogicUpdate()
    {
        if (currentTarget != null)
        {
            Vector3 directionToTarget = currentTarget.position - player.transform.position;
            directionToTarget.y = 0;
            player.Movement.SetRotationTarget(directionToTarget.normalized, player.Movement.RotationSpeed);
        }

        // Si on est en mode fente, on exécute la logique de fente
        if (isLunging)
        {
            bool isInWeaponRange = Vector3.Distance(player.transform.position, currentTarget.position) <= currentAttackProfile.weaponRange;
            if (currentTarget != null && !isInWeaponRange)
            {
                Vector3 lungeDirection = (currentTarget.position - player.transform.position).normalized;
                lungeDirection.y = 0;
                player.Movement.OverrideMovement = lungeDirection * player.Combat.lungeSpeed * Time.deltaTime;
            }
            else
            {
                isLunging = false;
            }
        }
    }

    public override void Exit()
    {
        // On nettoie nos variables quand on quitte l'état
        player.Movement.StopRotationOverride();
        isLunging = false;
        currentTarget = null;
        currentAttackProfile = null;
        isFirstAttack = false;
    }
}