using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GoalZone : MonoBehaviour
{
    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<VagabundoController>() == null) return;

        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            gm.GanarPartida();
        }
    }
}