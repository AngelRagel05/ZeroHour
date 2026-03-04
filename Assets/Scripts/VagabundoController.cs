using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class VagabundoController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadCaminar = 4.5f;
    public float velocidadCorrer = 8f;
    public float aceleracionAire = 0.85f;

    [Header("Salto")]
    public float fuerzaSalto = 12f;
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;
    public float jumpCutMultiplier = 0.5f;

    [Header("Suelo")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Ajustes")]
    public bool mostrarDebug = false;

    [Header("Vacilar")]
    public float duracionVacile = 0.45f;
    public float cooldownVacile = 0.3f;

    [Header("Daño")]
    public float empujeDanioX = 5.5f;
    public float empujeDanioY = 3.5f;
    public float bloqueoControlesDanio = 0.18f;
    public float parpadeoDanioTiempo = 0.22f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private float movH;
    private bool saltoSoltado;
    private bool controlesBloqueados;
    private bool estaMuerto;
    private bool estaCorriendo;
    private bool estaVacilando;
    private float vacileDisponibleEn;
    private bool enDanio;

    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool enSuelo;

    private Vector3 spawnInicial;
    private Vector3 spawnPoint;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        spawnInicial = transform.position;
        spawnPoint = transform.position;
    }

    private void Update()
    {
        if (controlesBloqueados || enDanio)
        {
            movH = 0f;
            return;
        }

        movH = LeerHorizontal();
        estaCorriendo = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Mathf.Abs(movH) > 0.01f;

        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }

        if (Input.GetButtonUp("Jump") || Input.GetKeyUp(KeyCode.Space))
        {
            saltoSoltado = true;
        }

        if (movH > 0.01f) transform.localScale = new Vector3(1f, 1f, 1f);
        else if (movH < -0.01f) transform.localScale = new Vector3(-1f, 1f, 1f);

        if (anim != null)
        {
            float velAnim = Mathf.Max(Mathf.Abs(movH), Mathf.Abs(rb.linearVelocity.x));
            SetFloatIfExists("Velocidad", velAnim);
            SetFloatIfExists("VelY", rb.linearVelocity.y);
            SetBoolIfExists("EnSuelo", enSuelo);
            SetBoolIfExists("Corriendo", estaCorriendo);
            SetBoolIfExists("Muerto", estaMuerto);
            SetBoolIfExists("Muerte", estaMuerto);
        }

        if (Input.GetKeyDown(KeyCode.E) && PuedeVacilar())
        {
            StartCoroutine(HacerVacile());
        }

        jumpBufferCounter -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        ActualizarSuelo();
        AplicarMovimientoHorizontal();
        IntentarSalto();
        AplicarJumpCut();

        saltoSoltado = false;
    }

    private void ActualizarSuelo()
    {
        if (groundCheck != null)
        {
            enSuelo = false;
            Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayer);
            for (int i = 0; i < hits.Length; i++)
            {
                // Ignora el propio jugador para no detectar suelo falso.
                if (hits[i] != null && hits[i].attachedRigidbody != rb)
                {
                    enSuelo = true;
                    break;
                }
            }
        }
        else
        {
            // Fallback simple por velocidad vertical si no hay groundCheck asignado.
            enSuelo = Mathf.Abs(rb.linearVelocity.y) < 0.05f;
        }

        if (enSuelo) coyoteCounter = coyoteTime;
        else coyoteCounter -= Time.fixedDeltaTime;
    }

    private void AplicarMovimientoHorizontal()
    {
        float velocidadActual = estaCorriendo ? velocidadCorrer : velocidadCaminar;
        float control = enSuelo ? 1f : aceleracionAire;
        float targetSpeed = movH * velocidadActual;
        float newVelX = Mathf.Lerp(rb.linearVelocity.x, targetSpeed, control);
        rb.linearVelocity = new Vector2(newVelX, rb.linearVelocity.y);
    }

    private void IntentarSalto()
    {
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
            GameManager gm = GameManager.Instance;
            if (gm != null) gm.PlayJumpSfx();
        }
    }

    private void AplicarJumpCut()
    {
        if (saltoSoltado && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }

    public void SetSpawnPoint(Vector3 newSpawn)
    {
        spawnPoint = newSpawn;
    }

    public void Respawn()
    {
        estaMuerto = false;
        controlesBloqueados = false;
        enDanio = false;
        if (anim != null)
        {
            SetBoolIfExists("Muerto", false);
            SetBoolIfExists("Muerte", false);
        }
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
        rb.linearVelocity = Vector2.zero;
        transform.position = spawnPoint;
    }

    public void ForzarSpawnInicial(Vector3 newSpawn)
    {
        spawnInicial = newSpawn;
        spawnPoint = newSpawn;
        transform.position = spawnPoint;
        rb.linearVelocity = Vector2.zero;
    }

    public void RespawnAlInicio()
    {
        spawnPoint = spawnInicial;
        Respawn();
    }

    public void ReproducirDanio()
    {
        if (anim != null && !estaMuerto)
        {
            SetTriggerIfExists("Hurt");
        }
    }

    public void AplicarEmpujeDanio(Vector2 fuenteDanio)
    {
        if (estaMuerto) return;

        ReproducirDanio();
        StopCoroutine(nameof(CorutinaDanioVisual));
        StartCoroutine(CorutinaDanioVisual());

        float dir = Mathf.Sign(((Vector2)transform.position - fuenteDanio).x);
        if (Mathf.Approximately(dir, 0f)) dir = transform.localScale.x >= 0f ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * empujeDanioX, empujeDanioY);
        StartCoroutine(CorutinaBloqueoDanio());
    }

    public void Morir()
    {
        estaMuerto = true;
        controlesBloqueados = true;
        rb.linearVelocity = Vector2.zero;
        if (anim != null)
        {
            SetBoolIfExists("Muerto", true);
            SetBoolIfExists("Muerte", true);
            SetTriggerIfExists("Dead");
        }
    }

    private bool PuedeVacilar()
    {
        return enSuelo && !estaMuerto && !estaVacilando && Time.time >= vacileDisponibleEn && anim != null;
    }

    private System.Collections.IEnumerator HacerVacile()
    {
        estaVacilando = true;
        controlesBloqueados = true;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        SetTriggerIfExists("Vacilar");

        yield return new WaitForSeconds(duracionVacile);

        if (!estaMuerto)
        {
            controlesBloqueados = false;
        }

        estaVacilando = false;
        vacileDisponibleEn = Time.time + cooldownVacile;
    }

    private System.Collections.IEnumerator CorutinaBloqueoDanio()
    {
        enDanio = true;
        controlesBloqueados = true;
        yield return new WaitForSeconds(bloqueoControlesDanio);
        enDanio = false;
        if (!estaMuerto && !estaVacilando)
        {
            controlesBloqueados = false;
        }
    }

    private System.Collections.IEnumerator CorutinaDanioVisual()
    {
        if (spriteRenderer == null) yield break;

        Color original = Color.white;
        Color golpe = new Color(1f, 0.45f, 0.45f, 1f);
        float t = 0f;

        while (t < parpadeoDanioTiempo)
        {
            spriteRenderer.color = golpe;
            yield return new WaitForSeconds(0.05f);
            spriteRenderer.color = original;
            yield return new WaitForSeconds(0.05f);
            t += 0.1f;
        }

        spriteRenderer.color = original;
    }

    private static float LeerHorizontal()
    {
        float h = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h += 1f;
        if (Mathf.Approximately(h, 0f)) h = Input.GetAxisRaw("Horizontal");
        return Mathf.Clamp(h, -1f, 1f);
    }

    private void SetBoolIfExists(string param, bool value)
    {
        if (HasParameter(param, AnimatorControllerParameterType.Bool))
        {
            anim.SetBool(param, value);
        }
    }

    private void SetFloatIfExists(string param, float value)
    {
        if (HasParameter(param, AnimatorControllerParameterType.Float))
        {
            anim.SetFloat(param, value);
        }
    }

    private void SetTriggerIfExists(string param)
    {
        if (HasParameter(param, AnimatorControllerParameterType.Trigger))
        {
            anim.SetTrigger(param);
        }
    }

    private bool HasParameter(string param, AnimatorControllerParameterType type)
    {
        if (anim == null) return false;
        AnimatorControllerParameter[] parameters = anim.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].name == param && parameters[i].type == type)
            {
                return true;
            }
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (!mostrarDebug || groundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
