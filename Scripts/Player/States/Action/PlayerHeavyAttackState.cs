using UnityEngine;
using System.Linq;

public class PlayerHeavyAttackState : State
{
    private AttackProfile attack;

    public PlayerHeavyAttackState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        attack = player.Combat.currentWeapon.weaponData.heavyAttackChain.First();

        player.Combat.currentWeapon.SetCurrentAttack(attack);

        // On dit à l'Animator de prendre le contrôle du mouvement
        player.Animator.applyRootMotion = true;

        // On déclenche la bonne animation, dont le nom est dans notre "fiche technique" d'arme
        player.Animator.Play(attack.animationStateName);
    }

    public override void Exit()
    {
        // En quittant l'état, on s'assure de redonner le contrôle du mouvement au script
        player.Animator.applyRootMotion = false;
    }
}