using UnityEngine;

public class PlayerChainAttackState : State
{
    private AttackProfile currentAttackProfile;

    // Variables pour la logique interne de cet état
    private bool isLunging;
    private Transform currentTarget;

    public PlayerChainAttackState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine)
    {
    }

    // Appelé dans PlayerCombat
    public void SetAttack(AttackProfile profile, Transform target)
    {
        currentAttackProfile = profile;
        currentTarget = target;
    }

    public override void Enter()
    {
        if (currentAttackProfile == null)
        {
            Debug.LogError("PlayerChainAttackState entré sans AttackProfile");
            stateMachine.ChangeState(player.ReadyState); // Sécurité
            return;
        }

        player.Combat.ResetHitConfirmation();

        if (currentTarget != null)
        {
            // On se tourne vers la cible
            Vector3 directionToTarget = currentTarget.position - player.transform.position;
            directionToTarget.y = 0;
            player.transform.rotation = Quaternion.LookRotation(directionToTarget);

            // On décide si on doit faire une fente
            float effectiveWeaponRange = player.Combat.currentWeapon.weaponData.GetWeaponRange(currentAttackProfile);
            float distance = Vector3.Distance(player.transform.position, currentTarget.position);
            if (distance > effectiveWeaponRange)
            {
                isLunging = true;
            }
        }
        // On lance le trigger d'animation correspondant à cette attaque
        player.Animator.SetTrigger(currentAttackProfile.animationTrigger);
    }

    public override void LogicUpdate()
    {
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
        isLunging = false;
        currentTarget = null;
    }
}