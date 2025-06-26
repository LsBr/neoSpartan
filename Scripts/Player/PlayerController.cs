using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerCombat))]
[RequireComponent(typeof(PlayerTargeting))]
public class PlayerController : MonoBehaviour
{
    // RÉFÉRENCES AUX COMPOSANTS ET SOUS-SYSTÈMES
    public PlayerMovement Movement { get; private set; }
    public PlayerCombat Combat { get; private set; }
    public PlayerTargeting Targeting { get; private set; }
    public Animator Animator { get; private set; }
    public Transform CameraMainTransform { get; private set; }
    public StateMachine LocomotionStateMachine { get; private set; }
    public StateMachine ActionStateMachine { get; private set; }

    // INPUTS (le contrôleur central les reçoit et les délègue)
    public Vector2 MoveInput { get; private set; }

    // DÉCLARATION DES ÉTATS
    public PlayerIdleState IdleState { get; private set; }
    public PlayerMoveState MoveState { get; private set; }
    public PlayerReadyState ReadyState { get; private set; }
    public PlayerChainAttackState ChainAttackState { get; private set; }
    public PlayerHeavyAttackState HeavyAttackState { get; private set; }

    private void Awake()
    {
        Movement = GetComponent<PlayerMovement>();
        Combat = GetComponent<PlayerCombat>();
        Targeting = GetComponent<PlayerTargeting>();
        Animator = GetComponent<Animator>();
        CameraMainTransform = Camera.main.transform;

        LocomotionStateMachine = new StateMachine();
        ActionStateMachine = new StateMachine();

        IdleState = new PlayerIdleState(this, LocomotionStateMachine);
        MoveState = new PlayerMoveState(this, LocomotionStateMachine);
        ReadyState = new PlayerReadyState(this, ActionStateMachine);
        ChainAttackState = new PlayerChainAttackState(this, ActionStateMachine);
        HeavyAttackState = new PlayerHeavyAttackState(this, ActionStateMachine);
    }

    private void Start()
    {
        LocomotionStateMachine.Initialize(IdleState);
        ActionStateMachine.Initialize(ReadyState);
    }

    void Update()
    {
        ActionStateMachine.CurrentState.LogicUpdate();

        if (ActionStateMachine.CurrentState.AllowsLocomotion)
        {
            LocomotionStateMachine.CurrentState.LogicUpdate();
            Animator.SetFloat("Speed", MoveInput.magnitude, 0.1f, Time.deltaTime);
        }
        else
            Animator.SetFloat("Speed", 0, 0.1f, Time.deltaTime);
    }

    public void OnMove(InputValue value)
    {
        MoveInput = value.Get<Vector2>();
    }
}
