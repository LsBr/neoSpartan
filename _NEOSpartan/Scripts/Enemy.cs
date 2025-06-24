using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 3;

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        Debug.Log(gameObject.name + " a pris " + damageAmount + " point(s) de dégât ! Vie restante : " + health);

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " est mort !");

        // On désactive le collider pour ne pas le toucher à nouveau
        GetComponent<Collider>().enabled = false;

        // On pourrait jouer une animation de mort ici

        // Pour l'instant, on détruit simplement l'objet après 2 secondes
        Destroy(gameObject, 2f);
    }
}
