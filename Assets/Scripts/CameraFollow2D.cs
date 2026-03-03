using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public Transform target;
    public float smooth = 8f;
    public Vector3 offset = new Vector3(0f, 1f, -10f);

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, smooth * Time.deltaTime);
    }
}