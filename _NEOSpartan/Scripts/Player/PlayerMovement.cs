// PlayerMovement.cs
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float PlayerSpeed = 5.0f;
    public float RotationSpeed = 15f;

    // Références aux composants principaux
    private CharacterController controller;
    private Transform cameraMainTransform;
    private PlayerController player; // Référence au coordinateur

    // Propriété pour l'intention de mouvement, mise à jour dans PlayerMoveState
    public Vector3 InputMovement { get; set; }
    // Propriété pour les mouvements scriptés comme la fente
    public Vector3 OverrideMovement { get; set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        cameraMainTransform = Camera.main.transform;
        player = GetComponent<PlayerController>(); // On récupère le chef d'orchestre
    }

    private void OnAnimatorMove()
    {
        // 1. On commence avec le mouvement de l'animation
        // si je ne veux pas de mouvement d'animation je peux "Bake Into Pose" dans l'animation
        Vector3 finalMovement = player.Animator.deltaPosition;

        // 2. On vérifie s'il y a un mouvement prioritaire (la fente)
        if (OverrideMovement != Vector3.zero)
        {
            // Si oui, on l'ajoute. Il a la priorité.
            finalMovement += OverrideMovement;
        }
        // 3. Sinon, s'il n'y a pas de mouvement prioritaire ET que l'état le permet...
        else if (player.ActionStateMachine.CurrentState.AllowsLocomotion)
        {
            // ... on ajoute le mouvement normal du joueur.
            finalMovement += InputMovement * PlayerSpeed * Time.deltaTime;
        }

        // 4. On applique le mouvement final UNE SEULE FOIS
        controller.Move(finalMovement);

        // 5. TRÈS IMPORTANT: On réinitialise le mouvement prioritaire après l'avoir appliqué
        //    pour qu'il ne s'applique qu'à cette frame.
        OverrideMovement = Vector3.zero;
    }
}