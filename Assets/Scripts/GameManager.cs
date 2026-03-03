using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Partida")]
    public int vidasIniciales = 3;
    public float tiempoInicial = 90f;
    public float respawnDelay = 0.6f;

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
        if (nivelTerminado || respawneando) return;

        vidasActuales -= dano;
        PlaySfx(danoSfx);

        if (vidasActuales <= 0)
        {
            vidasActuales = 0;
            PerderPartida();
            return;
        }

        StartCoroutine(RespawnJugador());
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

    private System.Collections.IEnumerator RespawnJugador()
    {
        respawneando = true;
        if (player != null)
        {
            yield return new WaitForSeconds(respawnDelay);
            player.Respawn();
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
}