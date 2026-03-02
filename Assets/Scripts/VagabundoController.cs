using UnityEngine;

public class VagabundoController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float velocidad = 6f;
    public float fuerzaSalto = 10f;
    public float velocidadEscalera = 4f;

    private Rigidbody2D rb;
    private Animator anim;
    private float movH;
    private float movV;
    private bool estaEnEscalera;

    void Start()
    {
        // Esto conecta el script con los componentes del objeto
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // Capturar las teclas (Flechas, WASD)
        movH = Input.GetAxisRaw("Horizontal");
        movV = Input.GetAxisRaw("Vertical");

        // --- ANIMACIONES ---
        // Le pasamos la velocidad al Animator (valor absoluto para que siempre sea positivo)
        anim.SetFloat("Velocidad", Mathf.Abs(movH));

        // Girar el sprite según la dirección
        if (movH > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (movH < 0) transform.localScale = new Vector3(-1, 1, 1);

        // --- SALTO ---
        // Saltamos si pulsamos Espacio y no nos estamos moviendo mucho en vertical (estamos en el suelo)
        if (Input.GetButtonDown("Jump") && Mathf.Abs(rb.linearVelocity.y) < 0.01f && !estaEnEscalera)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
        }

        // --- EL VACILE (Mecánica Especial) ---
        if (Input.GetKeyDown(KeyCode.E)) // Pulsa la E para vacilar con la litrona
        {
            anim.SetTrigger("Vacilar");
        }
    }

    void FixedUpdate()
    {
        // Aplicamos el movimiento físico
        if (estaEnEscalera)
        {
            rb.gravityScale = 0; // Quitamos gravedad para no caer al trepar
            rb.linearVelocity = new Vector2(movH * velocidad, movV * velocidadEscalera);
        }
        else
        {
            rb.gravityScale = 3; // Gravedad estilo plataformas pesado
            rb.linearVelocity = new Vector2(movH * velocidad, rb.linearVelocity.y);
        }
    }

    // --- DETECCIÓN DE ESCALERAS ---
    // El objeto escalera debe tener un Collider2D con "Is Trigger" activado y el Tag "Escalera"
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Escalera")) estaEnEscalera = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Escalera"))
        {
            estaEnEscalera = false;
            rb.gravityScale = 3; // Restaurar gravedad al salir
        }
    }
}