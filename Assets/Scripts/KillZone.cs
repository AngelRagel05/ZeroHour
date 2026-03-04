using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class KillZone : MonoBehaviour
{
    public int dano = 1;

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
            gm.DanoJugador(dano, transform.position);
        }
    }
}
