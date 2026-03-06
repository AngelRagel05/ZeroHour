using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Partida")]
    public float tiempoInicial = 90f;
    public bool muerteInstantanea = true;
    public float delayReinicioMuerte = 0.05f;

    [Header("Coleccionables")]
    public int totalColeccionablesNivel = 3;

    [Header("Final")]
    public bool mostrarPantallaFinalEnNivel = true;
    public string escenaVictoria = "Victory";

    [Header("Pausa")]
    public bool permitirPausa = true;
    public string escenaMenu = "MainMenu";

    [Header("HUD Fallback")]
    public bool mostrarHUDFallback = true;
    public Vector2 hudPosicion = new Vector2(12f, 12f);

    [Header("Feedback Daño")]
    public float flashRojoDuracion = 0.35f;
    [Range(0f, 1f)] public float flashRojoAlphaMax = 0.35f;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioSource pausaSource;
    public AudioSource saltoSource;
    public AudioSource coleccionableSource;
    public AudioClip saltoSfx;
    public AudioClip correrSfx;
    public AudioClip danoSfx;
    public AudioClip victoriaSfx;
    public AudioClip coleccionableSfx;
    public AudioClip pausaSfx;
    [Range(0f, 1f)] public float volumenSalto = 1f;
    [Range(0f, 1f)] public float volumenCorrer = 0.75f;
    [Range(0f, 1f)] public float volumenDanio = 1f;
    [Range(0f, 1f)] public float volumenVictoria = 1f;
    [Range(0f, 1f)] public float volumenColeccionable = 1f;
    [Range(0f, 1f)] public float volumenPausa = 1f;
    [Min(0f)] public float delayMinimoReinicioParaOirDanio = 0.15f;
    [Header("Audio Salto (Segmento)")]
    public bool usarSegmentoSalto = true;
    [Min(0f)] public float saltoSfxInicio = 0f;
    [Min(0f)] public float saltoSfxFin = 0.2f;
    [Header("Audio Daño (Segmento)")]
    public bool usarSegmentoDano = true;
    [Min(0f)] public float danoSfxInicio = 0.2f;
    [Min(0f)] public float danoSfxFin = 0.4f;

    private const string HighscorePrefix = "ZH_BEST_";
    private const string LastRunKey = "ZH_LAST_RUN";
    private const int MaxHighscores = 5;
    private const string PodioVersionKey = "ZH_PODIO_VERSION";
    private const int PodioVersionActual = 1;

    private VagabundoController player;
    private bool tieneHUDExterno;
    private bool nivelTerminado;
    private bool reiniciando;
    private bool pausado;
    private float tiempoRestante;
    private float flashRojoTimer;
    private int coleccionablesRecogidos;
    private float tiempoFinalRun;
    private BackgroundMusicPlayer backgroundMusicPlayer;
    private AudioSource coleccionableRuntimeSource;
    private float pausaSfxTime;
    private bool hasPausaSfxTime;
    private bool mostrarPanelSonidoFallback;

    public bool NivelTerminado => nivelTerminado;
    public bool Pausado => pausado;
    public bool PermitirPausa => permitirPausa;
    public int ColeccionablesRecogidos => coleccionablesRecogidos;
    public int TotalColeccionablesNivel => Mathf.Max(0, totalColeccionablesNivel);
    public float TiempoFinalRun => tiempoFinalRun;
    public float TiempoTranscurrido => Mathf.Clamp(tiempoInicial - tiempoRestante, 0f, tiempoInicial);
    public bool MuestraPantallaFinal => nivelTerminado && mostrarPantallaFinalEnNivel;

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
        LimpiarPodioSiEsVersionNueva();
        player = FindFirstObjectByType<VagabundoController>();
        tieneHUDExterno = FindFirstObjectByType<UIHUD>() != null;
        backgroundMusicPlayer = BackgroundMusicPlayer.Instance != null
            ? BackgroundMusicPlayer.Instance
            : FindFirstObjectByType<BackgroundMusicPlayer>();
        if (backgroundMusicPlayer != null)
        {
            backgroundMusicPlayer.NotifyRunStart();
        }
        CargarAjustesAudio();
        tiempoRestante = tiempoInicial;
        coleccionablesRecogidos = 0;
    }

    private void Update()
    {
        if (permitirPausa && !nivelTerminado && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePausa();
        }

        if (flashRojoTimer > 0f)
        {
            flashRojoTimer = Mathf.Max(0f, flashRojoTimer - Time.unscaledDeltaTime);
        }

        if (nivelTerminado || pausado) return;

        tiempoRestante -= Time.deltaTime;
        if (tiempoRestante <= 0f)
        {
            tiempoRestante = 0f;
            StartCoroutine(ReiniciarNivelConDelay(0f));
            return;
        }
    }

    public void TogglePausa()
    {
        if (!permitirPausa || nivelTerminado) return;
        SetPausa(!pausado);
    }

    public void SetPausa(bool value)
    {
        if (value && (!permitirPausa || nivelTerminado)) return;
        if (pausado == value) return;

        pausado = value;
        Time.timeScale = pausado ? 0f : 1f;
        SyncMusicPause(pausado);

        if (pausado)
        {
            PlayPausaSfx();
            StopRunSfx();
        }
        else
        {
            StopPausaSfx();
        }
    }

    public void Reanudar()
    {
        SetPausa(false);
    }

    public void RegistrarCheckpoint(Vector3 spawn)
    {
        if (player != null)
        {
            player.SetSpawnPoint(spawn);
        }
    }

    public void RegistrarColeccionable()
    {
        if (nivelTerminado) return;
        coleccionablesRecogidos = Mathf.Min(coleccionablesRecogidos + 1, TotalColeccionablesNivel);
        PlayColeccionableSfx();
    }

    public void ConfigurarTotalColeccionablesNivel(int total)
    {
        totalColeccionablesNivel = Mathf.Max(0, total);
        coleccionablesRecogidos = Mathf.Clamp(coleccionablesRecogidos, 0, TotalColeccionablesNivel);
    }

    public void DanoJugador(int dano = 1)
    {
        Vector2 fallbackFuente = player != null ? (Vector2)player.transform.position : Vector2.zero;
        DanoJugador(dano, fallbackFuente);
    }

    public void DanoJugador(int dano, Vector2 fuenteDanio)
    {
        if (nivelTerminado || reiniciando) return;

        flashRojoTimer = flashRojoDuracion;
        PlayDanoSfx();
        if (player != null)
        {
            player.AplicarEmpujeDanio(fuenteDanio);
        }

        if (muerteInstantanea)
        {
            float delayEfectivo = delayReinicioMuerte;
            if (danoSfx != null)
            {
                delayEfectivo = Mathf.Max(delayEfectivo, delayMinimoReinicioParaOirDanio);
            }

            StartCoroutine(ReiniciarNivelConDelay(delayEfectivo));
        }
    }

    public void DanoJugadorPorLava(int dano, Vector2 fuenteDanio)
    {
        DanoJugador(dano, fuenteDanio);
    }

    public void GanarPartida()
    {
        if (nivelTerminado) return;

        StopRunSfx();
        SetPausa(false);
        nivelTerminado = true;
        if (backgroundMusicPlayer != null)
        {
            backgroundMusicPlayer.NotifyVictory();
        }
        PlaySfx(victoriaSfx, volumenVictoria);

        tiempoFinalRun = TiempoTranscurrido;
        GuardarTiempoTop5(tiempoFinalRun);
        PlayerPrefs.SetFloat(LastRunKey, tiempoFinalRun);
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        string destinoVictoria = string.IsNullOrWhiteSpace(escenaVictoria) ? "Victory" : escenaVictoria;
        SceneManager.LoadScene(destinoVictoria);
    }

    public void PlayJumpSfx()
    {
        PlayJumpSfxInternal();
    }

    public void SetRunSfxActivo(bool activo)
    {
        if (activo)
        {
            PlayRunSfx();
        }
        else
        {
            StopRunSfx();
        }
    }

    public void ReiniciarNivel()
    {
        StopRunSfx();
        Time.timeScale = 1f;
        string scene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(scene);
    }

    public void IrMenu()
    {
        StopRunSfx();
        Time.timeScale = 1f;
        if (!string.IsNullOrWhiteSpace(escenaMenu))
        {
            SceneManager.LoadScene(escenaMenu);
        }
    }

    public List<float> ObtenerTop5()
    {
        List<float> tiempos = new List<float>(MaxHighscores);
        for (int i = 0; i < MaxHighscores; i++)
        {
            string key = HighscorePrefix + i;
            if (PlayerPrefs.HasKey(key))
            {
                tiempos.Add(PlayerPrefs.GetFloat(key));
            }
        }
        return tiempos;
    }

    public static float ObtenerUltimoTiempo()
    {
        return PlayerPrefs.GetFloat(LastRunKey, -1f);
    }

    public static string FormatearTiempo(float tiempo)
    {
        float t = Mathf.Max(0f, tiempo);
        int minutos = Mathf.FloorToInt(t / 60f);
        int segundos = Mathf.FloorToInt(t % 60f);
        int centesimas = Mathf.FloorToInt((t - Mathf.Floor(t)) * 100f);
        return $"{minutos:00}:{segundos:00}.{centesimas:00}";
    }

    private IEnumerator ReiniciarNivelConDelay(float delay)
    {
        if (reiniciando) yield break;
        reiniciando = true;
        if (delay > 0f) yield return new WaitForSeconds(delay);
        ReiniciarNivel();
    }

    private void GuardarTiempoTop5(float tiempo)
    {
        List<float> tiempos = ObtenerTop5();
        tiempos.Add(tiempo);
        tiempos.Sort();

        if (tiempos.Count > MaxHighscores)
        {
            tiempos.RemoveRange(MaxHighscores, tiempos.Count - MaxHighscores);
        }

        for (int i = 0; i < tiempos.Count; i++)
        {
            PlayerPrefs.SetFloat(HighscorePrefix + i, tiempos[i]);
        }

        for (int i = tiempos.Count; i < MaxHighscores; i++)
        {
            PlayerPrefs.DeleteKey(HighscorePrefix + i);
        }

        PlayerPrefs.Save();
    }

    [ContextMenu("Resetear Podio (Top 5)")]
    public void ResetearPodio()
    {
        for (int i = 0; i < MaxHighscores; i++)
        {
            PlayerPrefs.DeleteKey(HighscorePrefix + i);
        }

        PlayerPrefs.DeleteKey(LastRunKey);
        PlayerPrefs.Save();
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }

    private void PlaySfx(AudioClip clip, float volumen)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumen));
    }

    private void PlayJumpSfxInternal()
    {
        if (saltoSfx == null) return;

        AudioSource src = saltoSource != null ? saltoSource : sfxSource;
        if (src == null) return;

        if (!usarSegmentoSalto)
        {
            src.PlayOneShot(saltoSfx, Mathf.Clamp01(volumenSalto));
            return;
        }

        float clipLength = saltoSfx.length;
        if (clipLength <= 0f)
        {
            return;
        }

        float inicio = Mathf.Clamp(saltoSfxInicio, 0f, Mathf.Max(0f, clipLength - 0.01f));
        float fin = Mathf.Clamp(saltoSfxFin, inicio + 0.01f, clipLength);
        StartCoroutine(ReproducirSegmentoSalto(src, inicio, fin, Mathf.Clamp01(volumenSalto)));
    }

    private IEnumerator ReproducirSegmentoSalto(AudioSource plantilla, float inicio, float fin, float volumen)
    {
        GameObject temp = new GameObject("SFX_SaltoSegmento");
        temp.transform.position = plantilla != null ? plantilla.transform.position : Vector3.zero;

        AudioSource src = temp.AddComponent<AudioSource>();
        CopiarAjustesAudioSource(plantilla, src);

        src.clip = saltoSfx;
        src.loop = false;
        src.volume = volumen;
        int inicioSamples = Mathf.Clamp(Mathf.RoundToInt(inicio * saltoSfx.frequency), 0, Mathf.Max(0, saltoSfx.samples - 1));
        src.timeSamples = inicioSamples;
        src.Play();

        float duracion = Mathf.Max(0.01f, fin - inicio);
        yield return new WaitForSecondsRealtime(duracion);

        if (src != null) src.Stop();
        Destroy(temp);
    }

    private void PlayRunSfx()
    {
        if (correrSfx == null) return;
        if (pausado || nivelTerminado) return;

        AudioSource src = ObtenerRunSource();
        if (src == null) return;

        if (src.clip != correrSfx)
        {
            src.clip = correrSfx;
        }

        src.loop = true;
        src.volume = Mathf.Clamp01(volumenCorrer);
        if (!src.isPlaying)
        {
            src.Play();
        }
    }

    private void StopRunSfx()
    {
        AudioSource src = sfxSource;
        if (src != null && src.isPlaying && src.clip == correrSfx)
        {
            src.Stop();
        }
    }

    private AudioSource ObtenerRunSource()
    {
        return sfxSource;
    }

    private void PlayPausaSfx()
    {
        if (pausaSfx == null) return;

        AudioSource src = ObtenerPausaSource();
        if (src == null) return;

        src.clip = pausaSfx;
        src.loop = true;
        src.volume = Mathf.Clamp01(volumenPausa);
        if (hasPausaSfxTime)
        {
            float maxTime = Mathf.Max(0f, pausaSfx.length - 0.01f);
            src.time = Mathf.Clamp(pausaSfxTime, 0f, maxTime);
            hasPausaSfxTime = false;
        }
        else
        {
            src.time = 0f;
        }

        src.Play();
    }

    private void StopPausaSfx()
    {
        AudioSource src = pausaSource != null ? pausaSource : sfxSource;
        if (src != null && src.isPlaying)
        {
            pausaSfxTime = src.time;
            hasPausaSfxTime = true;
            src.Stop();
        }
    }

    private AudioSource ObtenerPausaSource()
    {
        return pausaSource != null ? pausaSource : sfxSource;
    }

    private void PlayDanoSfx()
    {
        if (danoSfx == null) return;

        if (!usarSegmentoDano || sfxSource == null)
        {
            PlaySfx(danoSfx, volumenDanio);
            return;
        }

        float clipLength = danoSfx.length;
        if (clipLength <= 0f)
        {
            return;
        }

        float inicio = Mathf.Clamp(danoSfxInicio, 0f, Mathf.Max(0f, clipLength - 0.01f));
        float fin = Mathf.Clamp(danoSfxFin, inicio + 0.01f, clipLength);
        StartCoroutine(ReproducirSegmentoDano(inicio, fin, Mathf.Clamp01(volumenDanio)));
    }

    private void PlayColeccionableSfx()
    {
        if (coleccionableSfx == null) return;

        AudioSource src = ObtenerColeccionableSource();
        if (src == null) return;

        src.PlayOneShot(coleccionableSfx, Mathf.Clamp01(volumenColeccionable));
    }

    private AudioSource ObtenerColeccionableSource()
    {
        if (coleccionableSource != null) return coleccionableSource;
        if (coleccionableRuntimeSource != null) return coleccionableRuntimeSource;

        GameObject go = new GameObject("SFX_Coleccionable_Runtime");
        go.transform.SetParent(transform, false);
        coleccionableRuntimeSource = go.AddComponent<AudioSource>();

        if (sfxSource != null)
        {
            CopiarAjustesAudioSource(sfxSource, coleccionableRuntimeSource);
        }

        // Coleccionables deben oirse siempre, sin atenuacion 3D.
        coleccionableRuntimeSource.spatialBlend = 0f;
        coleccionableRuntimeSource.playOnAwake = false;
        coleccionableRuntimeSource.loop = false;
        return coleccionableRuntimeSource;
    }

    private IEnumerator ReproducirSegmentoDano(float inicio, float fin, float volumen)
    {
        GameObject temp = new GameObject("SFX_DanoSegmento");
        if (sfxSource != null)
        {
            temp.transform.position = sfxSource.transform.position;
        }

        AudioSource src = temp.AddComponent<AudioSource>();
        CopiarAjustesAudioSource(sfxSource, src);

        src.clip = danoSfx;
        src.loop = false;
        src.volume *= volumen;
        src.time = inicio;
        src.Play();

        float duracion = Mathf.Max(0.01f, fin - inicio);
        yield return new WaitForSecondsRealtime(duracion);

        if (src != null) src.Stop();
        Destroy(temp);
    }

    private static void CopiarAjustesAudioSource(AudioSource from, AudioSource to)
    {
        if (to == null) return;
        if (from == null)
        {
            to.spatialBlend = 0f;
            to.volume = 1f;
            return;
        }

        to.outputAudioMixerGroup = from.outputAudioMixerGroup;
        to.mute = from.mute;
        to.bypassEffects = from.bypassEffects;
        to.bypassListenerEffects = from.bypassListenerEffects;
        to.bypassReverbZones = from.bypassReverbZones;
        to.priority = from.priority;
        to.volume = from.volume;
        to.pitch = from.pitch;
        to.panStereo = from.panStereo;
        to.spatialBlend = from.spatialBlend;
        to.reverbZoneMix = from.reverbZoneMix;
        to.dopplerLevel = from.dopplerLevel;
        to.spread = from.spread;
        to.rolloffMode = from.rolloffMode;
        to.minDistance = from.minDistance;
        to.maxDistance = from.maxDistance;
    }

    private void SyncMusicPause(bool isPaused)
    {
        if (backgroundMusicPlayer == null)
        {
            backgroundMusicPlayer = BackgroundMusicPlayer.Instance != null
                ? BackgroundMusicPlayer.Instance
                : FindFirstObjectByType<BackgroundMusicPlayer>();
        }

        if (backgroundMusicPlayer != null)
        {
            backgroundMusicPlayer.SetPaused(isPaused);
        }
    }

    private void OnGUI()
    {
        if (mostrarHUDFallback && !tieneHUDExterno)
        {
            GUIStyle hudStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold
            };

            GUI.Label(new Rect(hudPosicion.x, hudPosicion.y, 420f, 32f), $"Tiempo: {FormatearTiempo(TiempoTranscurrido)}", hudStyle);
            GUI.Label(new Rect(hudPosicion.x, hudPosicion.y + 30f, 420f, 32f), $"Llaves: {coleccionablesRecogidos}/{TotalColeccionablesNivel}", hudStyle);
            GUI.Label(new Rect(hudPosicion.x, hudPosicion.y + 60f, 420f, 32f), pausado ? "PAUSA" : "", hudStyle);
            GUI.Label(new Rect(hudPosicion.x, hudPosicion.y + 90f, 420f, 32f), nivelTerminado ? "NIVEL TERMINADO" : "", hudStyle);

            if (pausado)
            {
                DibujarMenuPausaFallback();
            }
        }

        if (!pausado && !nivelTerminado && flashRojoTimer > 0f)
        {
            float pct = Mathf.Clamp01(flashRojoTimer / flashRojoDuracion);
            float alpha = flashRojoAlphaMax * pct;
            Color prev = GUI.color;
            GUI.color = new Color(1f, 0f, 0f, alpha);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = prev;
        }
    }

    private void LimpiarPodioSiEsVersionNueva()
    {
        int versionGuardada = PlayerPrefs.GetInt(PodioVersionKey, 0);
        if (versionGuardada >= PodioVersionActual) return;

        ResetearPodio();
        PlayerPrefs.SetInt(PodioVersionKey, PodioVersionActual);
        PlayerPrefs.Save();
    }

    public void CargarAjustesAudio()
    {
        volumenSalto = AudioSettingsStore.Jump;
        volumenCorrer = AudioSettingsStore.Run;
        volumenDanio = AudioSettingsStore.Damage;
        volumenVictoria = AudioSettingsStore.Victory;
        volumenColeccionable = AudioSettingsStore.Collectible;
        volumenPausa = AudioSettingsStore.Pause;

        // Apply live to currently playing loop sources.
        AudioSource srcRun = ObtenerRunSource();
        if (srcRun != null && correrSfx != null && srcRun.clip == correrSfx)
        {
            srcRun.volume = Mathf.Clamp01(volumenCorrer);
        }

        AudioSource srcPausa = ObtenerPausaSource();
        if (srcPausa != null && pausaSfx != null && srcPausa.clip == pausaSfx)
        {
            srcPausa.volume = Mathf.Clamp01(volumenPausa);
        }

        if (backgroundMusicPlayer != null)
        {
            backgroundMusicPlayer.CargarVolumenDesdeAjustes();
        }
    }

    private void DibujarMenuPausaFallback()
    {
        Color prev = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 0.65f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = prev;

        float bw = 230f;
        float bh = 44f;
        float x = (Screen.width * 0.5f) - (bw * 0.5f);
        float y = Screen.height * 0.35f;

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 34,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        GUI.Label(new Rect(x - 20f, y - 74f, bw + 40f, 48f), "PAUSA", titleStyle);

        if (GUI.Button(new Rect(x, y, bw, bh), "CONTINUAR"))
        {
            Reanudar();
        }
        y += 54f;

        if (GUI.Button(new Rect(x, y, bw, bh), "REINICIAR"))
        {
            ReiniciarNivel();
        }
        y += 54f;

        if (GUI.Button(new Rect(x, y, bw, bh), "SALIR"))
        {
            IrMenu();
        }
        y += 54f;

        if (GUI.Button(new Rect(x, y, bw, bh), mostrarPanelSonidoFallback ? "CERRAR SONIDO" : "SONIDO"))
        {
            mostrarPanelSonidoFallback = !mostrarPanelSonidoFallback;
        }

        if (mostrarPanelSonidoFallback)
        {
            DibujarPanelSonidoFallback(new Rect(x + bw + 24f, Screen.height * 0.24f, 360f, 320f));
        }
    }

    private void DibujarPanelSonidoFallback(Rect panel)
    {
        Color prev = GUI.color;
        GUI.color = new Color(0.08f, 0.1f, 0.14f, 0.96f);
        GUI.DrawTexture(panel, Texture2D.whiteTexture);
        GUI.color = new Color(0.18f, 0.72f, 0.95f, 1f);
        GUI.DrawTexture(new Rect(panel.x, panel.y, panel.width, 2f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panel.x, panel.yMax - 2f, panel.width, 2f), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panel.x, panel.y, 2f, panel.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(panel.xMax - 2f, panel.y, 2f, panel.height), Texture2D.whiteTexture);
        GUI.color = prev;

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        GUI.Label(new Rect(panel.x, panel.y + 8f, panel.width, 26f), "AJUSTES DE SONIDO", titleStyle);

        float y = panel.y + 44f;
        DibujarSliderFallback(panel.x + 12f, ref y, "Musica", AudioSettingsStore.Music, AudioSettingsStore.SetMusic);
        DibujarSliderFallback(panel.x + 12f, ref y, "Salto", AudioSettingsStore.Jump, AudioSettingsStore.SetJump);
        DibujarSliderFallback(panel.x + 12f, ref y, "Correr", AudioSettingsStore.Run, AudioSettingsStore.SetRun);
        DibujarSliderFallback(panel.x + 12f, ref y, "Dano", AudioSettingsStore.Damage, AudioSettingsStore.SetDamage);
        DibujarSliderFallback(panel.x + 12f, ref y, "Victoria", AudioSettingsStore.Victory, AudioSettingsStore.SetVictory);
        DibujarSliderFallback(panel.x + 12f, ref y, "Coleccionable", AudioSettingsStore.Collectible, AudioSettingsStore.SetCollectible);
        DibujarSliderFallback(panel.x + 12f, ref y, "Pausa", AudioSettingsStore.Pause, AudioSettingsStore.SetPause);
    }

    private void DibujarSliderFallback(float x, ref float y, string nombre, float actual, System.Action<float> setter)
    {
        GUI.Label(new Rect(x, y, 120f, 22f), nombre);
        float nuevo = GUI.HorizontalSlider(new Rect(x + 112f, y + 6f, 170f, 16f), actual, 0f, 1f);
        GUI.Label(new Rect(x + 290f, y, 50f, 22f), $"{Mathf.RoundToInt(actual * 100f)}%");
        y += 36f;

        if (Mathf.Abs(nuevo - actual) > 0.001f)
        {
            setter(nuevo);
            CargarAjustesAudio();
            if (backgroundMusicPlayer != null)
            {
                backgroundMusicPlayer.CargarVolumenDesdeAjustes();
            }
        }
    }
}
