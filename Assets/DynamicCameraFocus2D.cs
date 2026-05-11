using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Camera))]
public class DynamicCameraFocus2D : MonoBehaviour
{
    [Header("Targets")]
    public List<GameObject> targets = new List<GameObject>();

    [Header("Movement")]
    public float moveSmoothTime = 0.3f;
    public float zoomSmoothTime = 0.3f;

    [Header("Zoom")]
    public float minZoom = 5f;
    public float maxZoom = 20f;
    public float zoomPadding = 2f;

    [Header("Behaviour")]
    public bool followTargets = true;

    [Header("Camera Bounds")]
    public bool useBounds = true;

    [Tooltip("Bottom Left Corner")]
    public Vector2 minBounds;

    [Tooltip("Top Right Corner")]
    public Vector2 maxBounds;

    private Camera cam;

    private Vector3 moveVelocity;
    private float zoomVelocity;

    void Awake()
    {
        cam = GetComponent<Camera>();

        if (!cam.orthographic)
        {
            Debug.LogWarning("DynamicCameraFocus2D works best with an Orthographic camera.");
        }
    }

    void LateUpdate()
    {
        if (!followTargets)
            return;

        List<Transform> activeTargets = GetActiveTargets();

        if (activeTargets.Count == 0)
            return;

        Bounds bounds = GetBounds(activeTargets);

        ZoomCamera(bounds);
        MoveCamera(bounds);
    }

    List<Transform> GetActiveTargets()
    {
        List<Transform> active = new List<Transform>();

        foreach (GameObject obj in targets)
        {
            if (obj != null && obj.activeInHierarchy)
            {
                active.Add(obj.transform);
            }
        }

        return active;
    }

    Bounds GetBounds(List<Transform> activeTargets)
    {
        Bounds bounds = new Bounds(activeTargets[0].position, Vector3.zero);

        foreach (Transform target in activeTargets)
        {
            bounds.Encapsulate(target.position);
        }

        return bounds;
    }

    void MoveCamera(Bounds bounds)
    {
        Vector3 targetPosition = new Vector3(
            bounds.center.x,
            bounds.center.y,
            transform.position.z
        );

        // Clamp camera inside bounds
        if (useBounds)
        {
            float camHeight = cam.orthographicSize;
            float camWidth = camHeight * cam.aspect;

            targetPosition.x = Mathf.Clamp(
                targetPosition.x,
                minBounds.x + camWidth,
                maxBounds.x - camWidth
            );

            targetPosition.y = Mathf.Clamp(
                targetPosition.y,
                minBounds.y + camHeight,
                maxBounds.y - camHeight
            );
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref moveVelocity,
            moveSmoothTime
        );
    }

    void ZoomCamera(Bounds bounds)
    {
        float verticalSize = bounds.size.y * 0.5f;
        float horizontalSize = (bounds.size.x * 0.5f) / cam.aspect;

        float targetZoom = Mathf.Max(verticalSize, horizontalSize);

        targetZoom += zoomPadding;

        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

        // Prevent zooming outside bounds
        if (useBounds)
        {
            float mapWidth = (maxBounds.x - minBounds.x) * 0.5f / cam.aspect;
            float mapHeight = (maxBounds.y - minBounds.y) * 0.5f;

            float maxAllowedZoom = Mathf.Min(mapWidth, mapHeight);

            targetZoom = Mathf.Min(targetZoom, maxAllowedZoom);
        }

        cam.orthographicSize = Mathf.SmoothDamp(
            cam.orthographicSize,
            targetZoom,
            ref zoomVelocity,
            zoomSmoothTime
        );
    }

    public void SetFollow(bool enabled)
    {
        followTargets = enabled;
    }

    public void AddTarget(GameObject target)
    {
        if (!targets.Contains(target))
        {
            targets.Add(target);
        }
    }

    public void RemoveTarget(GameObject target)
    {
        if (targets.Contains(target))
        {
            targets.Remove(target);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!useBounds)
            return;

        Gizmos.color = Color.green;

        Vector3 center = new Vector3(
            (minBounds.x + maxBounds.x) * 0.5f,
            (minBounds.y + maxBounds.y) * 0.5f,
            0f
        );

        Vector3 size = new Vector3(
            maxBounds.x - minBounds.x,
            maxBounds.y - minBounds.y,
            0f
        );

        Gizmos.DrawWireCube(center, size);
    }
}