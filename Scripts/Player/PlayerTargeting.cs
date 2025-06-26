// PlayerTargeting.cs
using UnityEngine;

public class PlayerTargeting : MonoBehaviour
{
    private PlayerController player;

    [Tooltip("L'angle de détection des ennemis pour ciblage de la première attaque")]
    public float targetingAngle = 90.0f;

    [Header("Réglages du ciblage de combo")]
    [Range(0f, 1f)]
    public float directionWeight = 0.8f; // Poids de l'alignement avec le stick (entre 0 et 1)
    [Range(0f, 1f)]
    public float distanceWeight = 0.2f; // Poids de la proximité de la cible (entre 0 et 1)
    [Tooltip("Seuil d'intention (0-1). En dessous de ce score d'alignement, la cible est ignorée.")]
    [Range(0f, 1f)]
    public float intentThreshold = 0.25f; // 0.25 correspond à ignorer ce qui est à plus de 135 degrés
    [Tooltip("Pénalité pour la cible actuelle pour faciliter le changement (1 = aucune pénalité)")]
    [Range(0f, 1f)]
    public float currentTargetPenalty = 0.8f;

    public void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    // Pour le premier coup d'une chaîne. Cherche la meilleure cible dans un cône devant le joueur.
    public Transform FindInitialTarget(float targetingRange)
    {
        // Cette logique est probablement similaire à celle que vous aviez au tout début.
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;

        Collider[] collidersInRange = Physics.OverlapSphere(transform.position, targetingRange);
        foreach (var col in collidersInRange)
        {
            if (!col.CompareTag("Enemy")) continue;

            Vector3 directionToTarget = col.transform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, directionToTarget);

            if (angle < targetingAngle / 2)
            {
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    bestTarget = col.transform;
                }
            }
        }
        return bestTarget;
    }

    // Pour enchaîner en plein combo. Cherche la meilleure cible à 360° en se basant sur la direction du stick.
    public Transform FindComboTarget(float targetingRange, Vector2 playerMoveInput)
    {
        Transform bestTarget = null;
        float bestScore = -1f; // On commence avec un score négatif
        Vector3 desiredDirection = player.transform.forward; // par défaut on cible devant le joueur

        Collider[] collidersInRange = Physics.OverlapSphere(transform.position, targetingRange);

        // convertir l'input relativement au monde par rapport à la caméra
        if (playerMoveInput != Vector2.zero)
        {
            Vector3 camForward = player.CameraMainTransform.forward;
            Vector3 camRight = player.CameraMainTransform.right;
            camForward.y = 0;
            camRight.y = 0;
            desiredDirection = (camForward.normalized * playerMoveInput.y + camRight.normalized * playerMoveInput.x).normalized;
        }

        foreach (var col in collidersInRange)
        {
            if (!col.CompareTag("Enemy")) continue;

            Vector3 directionToTarget = (col.transform.position - transform.position).normalized;

            // --- LOGIQUE DE SCORE ---

            // 1.On prend le dot product (qui va de -1 à 1) et on le ramène sur une échelle de 0 à 1.
            // 1 = parfaitement aligné, 0.5 = à 90 degrés, 0 = à l'opposé.
            float dotProduct = Vector3.Dot(desiredDirection, directionToTarget);
            float directionScore = (dotProduct + 1f) / 2f;

            // 2.Si l'alignement est trop mauvais, on ignore complètement cette cible.
            if (directionScore < intentThreshold) continue;

            // 3. Calcul du "Score de Distance" (entre 0 et 1)
            // 1 = très proche, 0 = à la limite de la portée.
            float distance = Vector3.Distance(transform.position, col.transform.position);
            float distanceScore = 1f - (distance / targetingRange);

            // 4. Calcul du Score Final pondéré
            float finalScore = (directionScore * directionWeight) + (distanceScore * distanceWeight);

            // 5. Application de la pénalité sur la cible actuelle pour faciliter le changement
            /*if (col.transform == currentTarget) // Assurez-vous de passer 'currentTarget' à la fonction
            {
                finalScore *= currentTargetPenalty;
            }*/

            // 6. Mise à jour de la meilleure cible
            if (finalScore > bestScore)
            {
                bestScore = finalScore;
                bestTarget = col.transform;
            }
        }
        return bestTarget;
    }
}