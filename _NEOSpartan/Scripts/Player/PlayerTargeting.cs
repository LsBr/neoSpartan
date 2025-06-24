// PlayerTargeting.cs
using UnityEngine;

public class PlayerTargeting : MonoBehaviour
{
    [Tooltip("L'angle de détection des ennemis, en degrés.")]
    public float targetingAngle = 90.0f;

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
    public Transform FindComboTarget(float targetingRange, Vector3 desiredDirection)
    {
        Transform bestTarget = null;
        float bestScore = -1f; // On commence avec un score négatif

        Collider[] collidersInRange = Physics.OverlapSphere(transform.position, targetingRange);

        foreach (var col in collidersInRange)
        {
            if (!col.CompareTag("Enemy")) continue;

            Vector3 directionToTarget = (col.transform.position - transform.position).normalized;

            // Calcul du score
            float angle = Vector3.Dot(desiredDirection.normalized, directionToTarget); // Proximité angulaire (entre -1 et 1)
            float distance = Vector3.Distance(transform.position, col.transform.position);

            // On veut un score élevé pour un angle proche de 1 et une distance faible.
            // Le poids de l'angle est plus important que celui de la distance.
            float score = (angle * 1.5f) + (1f / (1f + distance));

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = col.transform;
            }
        }
        return bestTarget;
    }
}