using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Partida")]
    public int vidasIniciales = 3;
    public float tiempoInicial = 90f;
    public float respawnDelay = 0.2f;
    public float delayGameOverMuerte = 0.7f;

    [Header("HUD Fallback")]
    public bool mostrarHUDFallback = true;
    public Vector2 hudPosicion = new Vector2(12f, 12f);

    [Header("Feedback Daño")]
    public float flashRojoDuracion = 0.35f;
    [Range(0f, 1f)] public float flashRojoAlphaMax = 0.35f;

    [Header("Escenas")]
    public string escenaMenu = "MainMenu";
    public string escenaVictoria = "Victory";
    public string escenaGameOver = "GameOver";

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip saltoSfx;
    public AudioClip danoSfx;
    public AudioClip checkpointSfx;
    public AudioClip victoriaSfx;

    private VagabundoController player;
    private int vidasActuales;
    private float tiempoRestante;
    private bool nivelTerminado;
    private bool respawneando;
    private bool tieneHUDExterno;
    private float flashRojoTimer;

    public int VidasActuales => vidasActuales;
    public float TiempoRestante => tiempoRestante;
    public bool NivelTerminado => nivelTerminado;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        player = FindFirstObjectByType<VagabundoController>();
        tieneHUDExterno = FindFirstObjectByType<UIHUD>() != null;
        vidasActuales = vidasIniciales;
        tiempoRestante = tiempoInicial;
    }

    private void Update()
    {
        if (nivelTerminado) return;

        tiempoRestante -= Time.deltaTime;
        if (tiempoRestante <= 0f)
        {
            tiempoRestante = 0f;
            PerderPartida();
        }

        if (flashRojoTimer > 0f)
        {
            flashRojoTimer -= Time.deltaTime;
        }
    }

    public void RegistrarCheckpoint(Vector3 spawn)
    {
        if (player != null)
        {
            player.SetSpawnPoint(spawn);
            PlaySfx(checkpointSfx);
        }
    }

    public void DanoJugador(int dano = 1)
    {
        Vector2 fallbackFuente = player != null ? (Vector2)player.transform.position : Vector2.zero;
        DanoJugador(dano, fallbackFuente);
    }

    public void DanoJugador(int dano, Vector2 fuenteDanio)
    {
        DanoJugador(dano, fuenteDanio, false);
    }

    public void DanoJugadorPorLava(int dano, Vector2 fuenteDanio)
    {
        DanoJugador(dano, fuenteDanio, true);
    }

    private void DanoJugador(int dano, Vector2 fuenteDanio, bool respawnEnInicio)
    {
        if (nivelTerminado || respawneando) return;

        vidasActuales -= dano;
        flashRojoTimer = flashRojoDuracion;
        PlaySfx(danoSfx);
        if (player != null)
        {
            player.AplicarEmpujeDanio(fuenteDanio);
        }

        if (vidasActuales <= 0)
        {
            vidasActuales = 0;
            if (player != null) player.Morir();
            StartCoroutine(PerderPartidaConDelay());
            return;
        }

        if (respawnEnInicio)
        {
            // En lava, vuelve al inicio al instante mientras se mantiene el flash rojo.
            if (player != null) player.RespawnAlInicio();
        }
        else
        {
            StartCoroutine(RespawnJugador(false));
        }
    }

    public void GanarPartida()
    {
        if (nivelTerminado) return;

        nivelTerminado = true;
        PlaySfx(victoriaSfx);

        if (!string.IsNullOrWhiteSpace(escenaVictoria))
        {
            SceneManager.LoadScene(escenaVictoria);
        }
    }

    public void PlayJumpSfx()
    {
        PlaySfx(saltoSfx);
    }

    public void ReiniciarNivel()
    {
        string scene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(scene);
    }

    public void IrMenu()
    {
        if (!string.IsNullOrWhiteSpace(escenaMenu))
        {
            SceneManager.LoadScene(escenaMenu);
        }
    }

    private System.Collections.IEnumerator RespawnJugador(bool enInicio)
    {
        respawneando = true;
        if (player != null)
        {
            yield return new WaitForSeconds(respawnDelay);
            if (enInicio) player.RespawnAlInicio();
            else player.Respawn();
        }
        respawneando = false;
    }

    private void PerderPartida()
    {
        if (nivelTerminado) return;

        nivelTerminado = true;

        if (!string.IsNullOrWhiteSpace(escenaGameOver))
        {
            SceneManager.LoadScene(escenaGameOver);
        }
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }

    private System.Collections.IEnumerator PerderPartidaConDelay()
    {
        yield return new WaitForSeconds(delayGameOverMuerte);
        PerderPartida();
    }

    private void OnGUI()
    {
        if (mostrarHUDFallback && !tieneHUDExterno)
        {
            float t = Mathf.Max(0f, tiempoRestante);
            int minutos = Mathf.FloorToInt(t / 60f);
            int segundos = Mathf.FloorToInt(t % 60f);

            GUI.Label(new Rect(hudPosicion.x, hudPosicion.y, 280f, 24f), $"Tiempo: {minutos:00}:{segundos:00}");
            GUI.Label(new Rect(hudPosicion.x, hudPosicion.y + 22f, 280f, 24f), $"Vidas: {vidasActuales}");
        }

        if (flashRojoTimer > 0f)
        {
            float pct = Mathf.Clamp01(flashRojoTimer / flashRojoDuracion);
            float alpha = flashRojoAlphaMax * pct;
            Color prev = GUI.color;
            GUI.color = new Color(1f, 0f, 0f, alpha);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = prev;
        }
    }
}
