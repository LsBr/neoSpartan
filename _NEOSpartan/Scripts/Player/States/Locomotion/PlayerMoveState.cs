using UnityEngine;

public class PlayerMoveState : State
{
    // Le constructeur qui passe les informations à la classe "State" de base
    public PlayerMoveState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
    }

    public override void LogicUpdate()
    {
        // Si le joueur relâche le stick, on retourne à l'état Idle
        if (player.MoveInput.magnitude <= 0.1f)
        {
            stateMachine.ChangeState(player.IdleState);
            return;
        }

        // 1. On calcule la direction du mouvement en fonction de la caméra et de l'input
        Vector3 moveDirection = (player.CameraMainTransform.forward * player.MoveInput.y + player.CameraMainTransform.right * player.MoveInput.x);
        moveDirection.y = 0;
        moveDirection.Normalize();

        // 2. On met à jour la propriété sur le PlayerController
        player.Movement.InputMovement = moveDirection;

        // 3. On fait tourner le personnage
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, player.Movement.RotationSpeed * Time.deltaTime);
        }

    }

    public override void Exit()
    {
        // En quittant l'état Move, on s'assure d'arrêter l'animation de course
        player.Movement.InputMovement = Vector3.zero;
    }
}