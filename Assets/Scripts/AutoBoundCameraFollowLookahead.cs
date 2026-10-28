// AUTO FOLLOW CAMERA
using UnityEngine;
using UnityEngine.Tilemaps;

public class AutoBoundCameraFollowLookahead : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Smoothness")]
    [Range(0.01f, 1f)]
    public float smoothTime = 0.25f;
    private Vector3 velocity = Vector3.zero;

    [Header("Lookahead Settings")]
    [Tooltip("How far ahead the camera should look horizontally")]
    public float lookAheadDistance = 4f;
    [Tooltip("How fast the camera shifts when changing directions")]
    public float lookAheadSpeed = 3f;

    [Header("Auto Bounds Settings")]
    public Tilemap boundsTilemap;

    private float minX, maxX, minY, maxY;
    private Camera cam;
    private float halfHeight;
    private float halfWidth;

    private float currentLookAheadX;
    private float targetLookAheadX;

    // Reference to fetch the flipX state from your player character
    private SpriteRenderer playerSpriteRenderer;

    void Start()
    {
        cam = GetComponent<Camera>();

        if (cam != null && cam.orthographic)
        {
            halfHeight = cam.orthographicSize;
            halfWidth = halfHeight * cam.aspect;
        }

        CalculateMapBounds();
        FindPlayerSpriteRenderer();
    }

    void FindPlayerSpriteRenderer()
    {
        if (target != null)
        {
            // First try to look directly on the assigned Player object
            playerSpriteRenderer = target.GetComponent<SpriteRenderer>();

            // If the SpriteRenderer component is attached to a child object underneath the player, grab that instead
            if (playerSpriteRenderer == null)
            {
                playerSpriteRenderer = target.GetComponentInChildren<SpriteRenderer>();
            }
        }
    }

    void CalculateMapBounds()
    {
        if (boundsTilemap == null) return;

        boundsTilemap.CompressBounds();
        Bounds cellBounds = boundsTilemap.localBounds;

        minX = cellBounds.min.x;
        maxX = cellBounds.max.x;
        minY = cellBounds.min.y;
        maxY = cellBounds.max.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Safety fallback check to gather reference context if missing
        if (playerSpriteRenderer == null)
        {
            FindPlayerSpriteRenderer();
        }

        // 1. Direction Evaluation logic based on flipX states
        float facingDirection = 1f;

        if (playerSpriteRenderer != null)
        {
            // If flipX is true, player is facing LEFT (-1). If false, player is facing RIGHT (1).
            facingDirection = playerSpriteRenderer.flipX ? -1f : 1f;
        }
        else
        {
            // Fallback safety option checking scale metric if renderer isn't detected
            facingDirection = Mathf.Sign(target.localScale.x);
        }

        // 2. Set horizontal destination factor coordinates
        targetLookAheadX = facingDirection * lookAheadDistance;

        // 3. Smooth translation sliding calculation updates
        currentLookAheadX = Mathf.MoveTowards(currentLookAheadX, targetLookAheadX, Time.deltaTime * lookAheadSpeed);

        // 4. Construct focus destination path vector tracking values
        Vector3 targetPosition = new Vector3(target.position.x + currentLookAheadX, target.position.y, 0f) + offset;

        // 5. Force rigid tile boundary edge clamps
        if (boundsTilemap != null)
        {
            float clampedX = Mathf.Clamp(targetPosition.x, minX + halfWidth, maxX - halfWidth);
            float clampedY = Mathf.Clamp(targetPosition.y, minY + halfHeight, maxY - halfHeight);

            targetPosition = new Vector3(clampedX, clampedY, targetPosition.z);
        }

        // 6. Execute follow movement interpolation translation
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
