using UnityEngine;

public class PlayerIdleState : State
{
    // Le constructeur qui passe les informations à la classe "State" de base
    public PlayerIdleState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
    }

    public override void LogicUpdate()
    {
        // À chaque frame, on surveille si le joueur appuie sur une touche de mouvement.
        // On utilise .magnitude > 0.1f comme petite "zone morte" pour les joysticks.
        if (player.MoveInput.magnitude > 0.1f)
        {
            // Si oui, on demande à la machine à états de passer à l'état de Mouvement.
            stateMachine.ChangeState(player.MoveState);
        }
    }
}