using UnityEngine;

/// <summary>Smoothly follows the player after the movement update.</summary>
public sealed class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -10f);
    [SerializeField] private float smoothTime = 0.18f;

    private Vector3 smoothVelocity;

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 destination = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, destination, ref smoothVelocity, smoothTime);
    }
}
