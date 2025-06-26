// PlayerMovement.cs
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    // Références aux composants principaux
    private CharacterController controller;
    private PlayerController player; // Référence au coordinateur

    [Header("Movement Settings")]
    public float PlayerMaxSpeed = 5.0f;
    public float RotationSpeed = 15f;

    // Propriété pour l'intention de mouvement, mise à jour dans PlayerMoveState
    public Vector3 InputMovement { get; set; }
    // Propriété pour les mouvements scriptés comme la fente
    public Vector3 OverrideMovement { get; set; }
    // Propriété pour la vitesse de déplacement
    public float InputMagnitude { get; set; }

    private bool isRotationOverridden = false;
    private Vector3 rotationDirection;
    private float currentRotationSpeed;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        player = GetComponent<PlayerController>(); // On récupère le chef d'orchestre
    }

    private void Update()
    {
        // On exécute la rotation si un ordre a été donné
        if (isRotationOverridden)
        {
            PerformRotation();
        }
    }

    public void SetRotationTarget(Vector3 direction, float speed)
    {
        isRotationOverridden = true;
        rotationDirection = direction;
        currentRotationSpeed = speed;
    }

    public void StopRotationOverride()
    {
        isRotationOverridden = false;
    }

    private void PerformRotation()
    {
        if (rotationDirection == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(rotationDirection);
        float angle = Vector3.Angle(transform.forward, rotationDirection);

        if (angle > 1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, currentRotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.rotation = targetRotation; // Snap final
            isRotationOverridden = false; // La rotation est terminée, on arrête
        }
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
            float currSpeed = PlayerMaxSpeed * InputMagnitude;
            finalMovement += InputMovement * currSpeed * Time.deltaTime;
        }

        // 4. On applique le mouvement final UNE SEULE FOIS
        controller.Move(finalMovement);

        // 5. TRÈS IMPORTANT: On réinitialise le mouvement prioritaire après l'avoir appliqué
        //    pour qu'il ne s'applique qu'à cette frame.
        OverrideMovement = Vector3.zero;
    }
}