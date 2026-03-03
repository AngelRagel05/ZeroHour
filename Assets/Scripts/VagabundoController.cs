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

    private Rigidbody2D rb;
    private Animator anim;

    private float movH;
    private bool saltoSoltado;
    private bool controlesBloqueados;
    private bool estaMuerto;
    private bool estaCorriendo;
    private bool estaVacilando;
    private float vacileDisponibleEn;

    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool enSuelo;

    private Vector3 spawnPoint;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        spawnPoint = transform.position;
    }

    private void Update()
    {
        if (controlesBloqueados)
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
            float velAbs = Mathf.Abs(rb.linearVelocity.x);
            anim.SetFloat("Velocidad", velAbs);
            anim.SetFloat("VelY", rb.linearVelocity.y);
            anim.SetBool("EnSuelo", enSuelo);
            anim.SetBool("Corriendo", estaCorriendo);
            anim.SetBool("Muerto", estaMuerto);
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
            enSuelo = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
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
        if (anim != null) anim.SetBool("Muerto", false);
        rb.linearVelocity = Vector2.zero;
        transform.position = spawnPoint;
    }

    public void ForzarSpawnInicial(Vector3 newSpawn)
    {
        spawnPoint = newSpawn;
        transform.position = spawnPoint;
        rb.linearVelocity = Vector2.zero;
    }

    public void ReproducirDanio()
    {
        if (anim != null && !estaMuerto)
        {
            anim.SetTrigger("Hurt");
        }
    }

    public void Morir()
    {
        estaMuerto = true;
        controlesBloqueados = true;
        rb.linearVelocity = Vector2.zero;
        if (anim != null)
        {
            anim.SetBool("Muerto", true);
            anim.SetTrigger("Dead");
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
        anim.SetTrigger("Vacilar");

        yield return new WaitForSeconds(duracionVacile);

        if (!estaMuerto)
        {
            controlesBloqueados = false;
        }

        estaVacilando = false;
        vacileDisponibleEn = Time.time + cooldownVacile;
    }

    private static float LeerHorizontal()
    {
        float h = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) h -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h += 1f;
        if (Mathf.Approximately(h, 0f)) h = Input.GetAxisRaw("Horizontal");
        return Mathf.Clamp(h, -1f, 1f);
    }

    private void OnDrawGizmosSelected()
    {
        if (!mostrarDebug || groundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
