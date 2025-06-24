public class StateMachine
{
    // Propriété pour accéder à l'état actuellement actif (lecture seule de l'extérieur)
    public State CurrentState { get; private set; }

    // Fonction pour initialiser la machine avec un état de départ
    public void Initialize(State startingState)
    {
        CurrentState = startingState;
        startingState.Enter();
    }

    // Fonction pour changer d'état
    public void ChangeState(State newState)
    {
        // On exécute la logique de sortie de l'état actuel
        CurrentState.Exit();

        // On passe au nouvel état et on exécute sa logique d'entrée
        CurrentState = newState;
        newState.Enter();
    }

    // On va aussi ajouter une fonction pour faire le lien avec le Update de Unity
    public void ExecuteStateUpdate()
    {
        CurrentState?.LogicUpdate(); // Le '?' est une sécurité qui vérifie que CurrentState n'est pas nul
    }
}