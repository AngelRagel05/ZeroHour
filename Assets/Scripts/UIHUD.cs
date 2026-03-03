using UnityEngine;

public class UIHUD : MonoBehaviour
{
    [Header("HUD Debug (sin paquete UI)")]
    public bool mostrarHUD = true;
    public Vector2 posicion = new Vector2(12f, 12f);

    private GameManager gm;

    private void Start()
    {
        gm = GameManager.Instance;
    }

    private void Update()
    {
        if (gm == null)
        {
            gm = GameManager.Instance;
        }
    }

    private void OnGUI()
    {
        if (!mostrarHUD || gm == null) return;

        float t = Mathf.Max(0f, gm.TiempoRestante);
        int minutos = Mathf.FloorToInt(t / 60f);
        int segundos = Mathf.FloorToInt(t % 60f);

        GUI.Label(new Rect(posicion.x, posicion.y, 260f, 24f), $"Tiempo: {minutos:00}:{segundos:00}");
        GUI.Label(new Rect(posicion.x, posicion.y + 22f, 260f, 24f), $"Vidas: {gm.VidasActuales}");
        GUI.Label(new Rect(posicion.x, posicion.y + 44f, 320f, 24f), gm.NivelTerminado ? "Nivel Terminado" : "");
    }
}
