using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow2D : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform target;
    public Vector3 offset = new Vector3(2f, 1.5f, -10f);

    [Header("Seguimiento")]
    public float smoothTime = 0.15f;
    public Vector2 deadZone = new Vector2(2f, 1.2f);

    [Header("Zoom (Camara Ortografica)")]
    public float zoom = 7f;
    public float zoomSmooth = 6f;

    private Camera cam;
    private Vector3 velocity;
    private Vector2 followPoint;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        if (target != null)
        {
            followPoint = target.position;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector2 targetPos = target.position;
        Vector2 delta = targetPos - followPoint;

        // La camara solo corrige cuando el jugador sale de esta zona, evitando centrarlo siempre.
        if (Mathf.Abs(delta.x) > deadZone.x)
        {
            followPoint.x = targetPos.x - Mathf.Sign(delta.x) * deadZone.x;
        }

        if (Mathf.Abs(delta.y) > deadZone.y)
        {
            followPoint.y = targetPos.y - Mathf.Sign(delta.y) * deadZone.y;
        }

        Vector3 desired = new Vector3(followPoint.x + offset.x, followPoint.y + offset.y, offset.z);
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);

        if (cam != null && cam.orthographic)
        {
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, zoom, zoomSmooth * Time.deltaTime);
        }
    }
}
