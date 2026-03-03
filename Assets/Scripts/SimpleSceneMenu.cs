using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleSceneMenu : MonoBehaviour
{
    [Header("Escenas")]
    public string escenaJugar = "SampleScene";
    public string escenaMenu = "MainMenu";

    [Header("Textos")]
    public string titulo = "ZEROHOUR";
    public string subtitulo = "Escapa antes de que llegue a 00:00";
    public bool mostrarBotonJugar = true;
    public bool mostrarBotonMenu = false;
    public bool mostrarBotonReintentar = false;
    public bool mostrarBotonSalir = true;

    private void OnGUI()
    {
        float w = Screen.width;
        float h = Screen.height;

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 42;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.alignment = TextAnchor.MiddleCenter;

        GUIStyle subStyle = new GUIStyle(GUI.skin.label);
        subStyle.fontSize = 18;
        subStyle.alignment = TextAnchor.MiddleCenter;

        GUI.Label(new Rect(0f, h * 0.15f, w, 50f), titulo, titleStyle);
        GUI.Label(new Rect(0f, h * 0.23f, w, 30f), subtitulo, subStyle);

        float bw = 220f;
        float bh = 42f;
        float x = (w - bw) * 0.5f;
        float y = h * 0.45f;

        if (mostrarBotonJugar)
        {
            if (GUI.Button(new Rect(x, y, bw, bh), "JUGAR"))
            {
                if (!string.IsNullOrWhiteSpace(escenaJugar)) SceneManager.LoadScene(escenaJugar);
            }
            y += 52f;
        }

        if (mostrarBotonReintentar)
        {
            if (GUI.Button(new Rect(x, y, bw, bh), "REINTENTAR"))
            {
                SceneManager.LoadScene(escenaJugar);
            }
            y += 52f;
        }

        if (mostrarBotonMenu)
        {
            if (GUI.Button(new Rect(x, y, bw, bh), "MENU"))
            {
                if (!string.IsNullOrWhiteSpace(escenaMenu)) SceneManager.LoadScene(escenaMenu);
            }
            y += 52f;
        }

        if (mostrarBotonSalir)
        {
            if (GUI.Button(new Rect(x, y, bw, bh), "SALIR"))
            {
                Application.Quit();
            }
        }
    }
}