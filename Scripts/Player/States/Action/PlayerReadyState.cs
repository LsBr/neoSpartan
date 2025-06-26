using UnityEngine;

public class PlayerReadyState : State
{
    public override bool AllowsLocomotion => true;

    public PlayerReadyState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void LogicUpdate()
    {
    }
    public override void Exit()
    {
    }
}