using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class VagabundoController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 8f;
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

    private Rigidbody2D rb;
    private Animator anim;

    private float movH;
    private bool saltoSoltado;

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
        movH = LeerHorizontal();

        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space))
        {
            jumpBufferCounter = jumpBufferTime;
        }

        if (Input.GetButtonUp("Jump") || Input.GetKeyUp(KeyCode.Space))
        {
            saltoSoltado = true;
        }

        if (anim != null)
        {
            anim.SetFloat("Velocidad", Mathf.Abs(movH));
        }

        if (movH > 0.01f) transform.localScale = new Vector3(1f, 1f, 1f);
        else if (movH < -0.01f) transform.localScale = new Vector3(-1f, 1f, 1f);

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
        float control = enSuelo ? 1f : aceleracionAire;
        float targetSpeed = movH * velocidad;
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
        rb.linearVelocity = Vector2.zero;
        transform.position = spawnPoint;
    }

    public void ForzarSpawnInicial(Vector3 newSpawn)
    {
        spawnPoint = newSpawn;
        transform.position = spawnPoint;
        rb.linearVelocity = Vector2.zero;
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
