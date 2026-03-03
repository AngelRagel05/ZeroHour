using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButtons : MonoBehaviour
{
    public string escenaJuego = "SampleScene";
    public string escenaMenu = "MainMenu";

    public void Jugar()
    {
        if (!string.IsNullOrWhiteSpace(escenaJuego))
        {
            SceneManager.LoadScene(escenaJuego);
        }
    }

    public void Menu()
    {
        if (!string.IsNullOrWhiteSpace(escenaMenu))
        {
            SceneManager.LoadScene(escenaMenu);
        }
    }

    public void ReiniciarNivel()
    {
        string scene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(scene);
    }

    public void Salir()
    {
        Application.Quit();
    }
}