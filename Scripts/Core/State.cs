using UnityEngine;

public abstract class State
{
    // Chaque état aura besoin d'une référence vers son "superviseur" (la StateMachine)
    // pour pouvoir demander des changements d'état.
    protected StateMachine stateMachine;
    protected PlayerController player;

    public virtual bool AllowsLocomotion { get; } = false;

    // Le constructeur qui sera appelé par chaque état enfant
    protected State(PlayerController player, StateMachine stateMachine)
    {
        this.player = player;
        this.stateMachine = stateMachine;
    }

    // Appelé une seule fois, quand on entre dans l'état. Pour l'initialisation.
    public virtual void Enter()
    {
    }

    // Appelé à chaque frame dans Update(). Pour la logique qui dépend du framerate.
    public virtual void LogicUpdate()
    {
    }

    // Appelé à chaque frame de physique dans FixedUpdate(). Pour la physique.
    public virtual void PhysicsUpdate()
    {
    }

    // Appelé une seule fois, quand on quitte l'état. Pour le nettoyage.
    public virtual void Exit()
    {
    }
}