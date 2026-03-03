using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    private bool activado;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activado) return;
        if (other.GetComponent<VagabundoController>() == null) return;

        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            gm.RegistrarCheckpoint(transform.position);
            activado = true;
        }
    }
}