using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float followSpeed = 0.1f;
    [SerializeField] private Vector3 baseOffset;
    private Vector3 currentOffset;
    private Vector3 velocity = Vector3.zero;

    [SerializeField] private float verticalAlignSpeed = 5f;
    [SerializeField] private float landingCatchUpSpeed = 3f;
    [Range(0f, 1f)][SerializeField] private float upwardFollowThreshold = 0.75f;
    [Range(0f, 1f)][SerializeField] private float downwardFollowThreshold = 0.25f;

    [SerializeField] private float chunkTransitionSpeed = 3f;
    [SerializeField] private float highSpeedChunkTransitionSpeed = 8f;
    [SerializeField] private float highSpeedThresholdX = 15f;
    [SerializeField] private float highSpeedThresholdY = 20f;
    [SerializeField] private float highSpeedFollowSpeed = 0.04f;
    [SerializeField] private float highSpeedEnterSpeed = 2f;
    [SerializeField] private float highSpeedExitSpeed = 1f;

    private Camera cam;
    private float targetOffsetY;
    private float groundedYTarget;

    private bool wasGrounded;
    private bool isCatchingUp;

    private Collider2D currentBoundary;
    private Collider2D previousBoundary;
    private float boundaryTransitionTimer;
    private Vector3 targetPosition;
    private Vector3 transitionStartTarget;

    private float highSpeedBlendX;
    private float highSpeedBlendY;

    private float velocityX;
    private float velocityY;

    private float skipTransitionTimer;

    private List<CameraZone> activeZones = new List<CameraZone>();
    private List<Collider2D> activeChunks = new List<Collider2D>();

    private float shakeDuration;
    private float shakeMagnitude;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        currentOffset = baseOffset;
        targetOffsetY = baseOffset.y;
    }

    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            groundedYTarget = PlayerController.Instance.transform.position.y;

            targetPosition = PlayerController.Instance.transform.position + baseOffset;
            transitionStartTarget = targetPosition;

            PlayerState playerState = PlayerController.Instance.GetComponent<PlayerState>();
            if (playerState != null)
            {
                wasGrounded = playerState.isGrounded;
            }

            ScanForActiveChunks();
        }
    }

    void LateUpdate()
    {
        if (PlayerController.Instance == null) return;

        Vector3 playerPos = PlayerController.Instance.transform.position;
        Rigidbody2D playerRb = PlayerController.Instance.rb;

        bool isHighSpeedX = playerRb != null && Mathf.Abs(playerRb.linearVelocity.x) > highSpeedThresholdX;
        bool isHighSpeedY = playerRb != null && Mathf.Abs(playerRb.linearVelocity.y) > highSpeedThresholdY;

        if (isHighSpeedX)
        {
            highSpeedBlendX = Mathf.MoveTowards(highSpeedBlendX, 1f, highSpeedEnterSpeed * Time.deltaTime);
        }
        else
        {
            highSpeedBlendX = Mathf.MoveTowards(highSpeedBlendX, 0f, highSpeedExitSpeed * Time.deltaTime);
        }

        if (isHighSpeedY)
        {
            highSpeedBlendY = Mathf.MoveTowards(highSpeedBlendY, 1f, highSpeedEnterSpeed * Time.deltaTime);
        }
        else
        {
            highSpeedBlendY = Mathf.MoveTowards(highSpeedBlendY, 0f, highSpeedExitSpeed * Time.deltaTime);
        }

        float halfHeight = cam.orthographicSize;
        float upwardWorldOffset = (upwardFollowThreshold - 0.5f) * 2f * halfHeight;
        float downwardWorldOffset = (downwardFollowThreshold - 0.5f) * 2f * halfHeight;

        PlayerState playerState = PlayerController.Instance.GetComponent<PlayerState>();
        if (playerState != null)
        {
            if (playerState.isGrounded)
            {
                if (!wasGrounded)
                {
                    isCatchingUp = true;
                    wasGrounded = true;
                }

                if (isCatchingUp)
                {
                    groundedYTarget = Mathf.Lerp(groundedYTarget, playerPos.y, landingCatchUpSpeed * Time.deltaTime);

                    if (Mathf.Abs(groundedYTarget - playerPos.y) < 0.05f)
                    {
                        groundedYTarget = playerPos.y;
                        isCatchingUp = false;
                    }
                }
                else
                {
                    groundedYTarget = playerPos.y;
                }
            }
            else
            {
                wasGrounded = false;
                isCatchingUp = false;

                float upperLimit = groundedYTarget + currentOffset.y + upwardWorldOffset;
                float lowerLimit = groundedYTarget + currentOffset.y + downwardWorldOffset;

                float verticalVelocity = playerRb != null ? playerRb.linearVelocity.y : 0f;

                if (verticalVelocity > 0.01f) 
                {
                    if (playerPos.y > upperLimit)
                    {
                        groundedYTarget = Mathf.Lerp(groundedYTarget, playerPos.y, landingCatchUpSpeed * Time.deltaTime);

                        float hardLimitY = playerPos.y - currentOffset.y - upwardWorldOffset;
                        if (groundedYTarget < hardLimitY) groundedYTarget = hardLimitY;
                    }
                }
                else if (verticalVelocity < -0.01f) 
                {
                    if (playerPos.y < lowerLimit)
                    {
                        groundedYTarget = Mathf.Lerp(groundedYTarget, playerPos.y, landingCatchUpSpeed * Time.deltaTime);

                        float hardLimitY = playerPos.y - currentOffset.y - downwardWorldOffset;
                        if (groundedYTarget > hardLimitY) groundedYTarget = hardLimitY;
                    }
                }
            }
        }
        else
        {
            groundedYTarget = playerPos.y;
        }

        if (highSpeedBlendY > 0.01f)
        {
            groundedYTarget = Mathf.Lerp(groundedYTarget, playerPos.y, highSpeedBlendY);
        }

        currentOffset.y = Mathf.Lerp(currentOffset.y, targetOffsetY, verticalAlignSpeed * Time.deltaTime);

        Vector3 rawTarget = Vector3.zero;
        rawTarget.z = playerPos.z + baseOffset.z;

        rawTarget.x = Mathf.Lerp(playerPos.x + baseOffset.x, playerPos.x, highSpeedBlendX);
        rawTarget.y = Mathf.Lerp(groundedYTarget + currentOffset.y, playerPos.y, highSpeedBlendY);

        bool isPlayerTransitioning = playerState != null && playerState.hit;
        if (currentBoundary == null && !isPlayerTransitioning)
        {
            ScanForActiveChunks();
        }

        float maxSpeedBlend = Mathf.Max(highSpeedBlendX, highSpeedBlendY);
        float activeTransitionSpeed = Mathf.Lerp(chunkTransitionSpeed, highSpeedChunkTransitionSpeed, maxSpeedBlend);

        if (skipTransitionTimer > 0f)
        {
            skipTransitionTimer -= Time.deltaTime;
            boundaryTransitionTimer = 1f;
        }
        else
        {
            boundaryTransitionTimer += Time.deltaTime * activeTransitionSpeed;
        }

        float t = Mathf.Clamp01(boundaryTransitionTimer);

        if (currentBoundary != null)
        {
            Vector3 clampedTargetNew = ClampTargetToBounds(rawTarget, currentBoundary);

            if (t < 1f)
            {
                Vector3 clampedTargetOld = ClampTargetToBounds(rawTarget, previousBoundary);
                targetPosition = Vector3.Lerp(transitionStartTarget, clampedTargetNew, t);
            }
            else
            {
                targetPosition = clampedTargetNew;
            }
        }
        else
        {
            if (t < 1f)
            {
                targetPosition = Vector3.Lerp(transitionStartTarget, rawTarget, t);
            }
            else
            {
                targetPosition = rawTarget;
            }
        }

        float currentFollowSpeedX = Mathf.Lerp(followSpeed, highSpeedFollowSpeed, highSpeedBlendX);
        float currentFollowSpeedY = Mathf.Lerp(followSpeed, highSpeedFollowSpeed, highSpeedBlendY);

        float newX = Mathf.SmoothDamp(transform.position.x, targetPosition.x, ref velocityX, currentFollowSpeedX);
        float newY = Mathf.SmoothDamp(transform.position.y, targetPosition.y, ref velocityY, currentFollowSpeedY);

        transform.position = new Vector3(newX, newY, transform.position.z);

        if (shakeDuration > 0)
        {
            Vector3 shakeOffset = Random.insideUnitSphere * shakeMagnitude;
            shakeOffset.z = 0;
            transform.position += shakeOffset;
            shakeDuration -= Time.deltaTime;
        }
    }

    private void TransitionToBoundary(Collider2D newBoundary)
    {
        if (currentBoundary != newBoundary)
        {
            previousBoundary = currentBoundary;
            currentBoundary = newBoundary;

            if (skipTransitionTimer > 0f)
            {
                boundaryTransitionTimer = 1f;

                if (PlayerController.Instance != null)
                {
                    Vector3 playerPos = PlayerController.Instance.transform.position;

                    Vector3 rawTarget = Vector3.zero;
                    rawTarget.z = playerPos.z + baseOffset.z;

                    rawTarget.x = Mathf.Lerp(playerPos.x + baseOffset.x, playerPos.x, highSpeedBlendX);
                    rawTarget.y = Mathf.Lerp(groundedYTarget + currentOffset.y, playerPos.y, highSpeedBlendY);

                    if (currentBoundary != null)
                    {
                        targetPosition = ClampTargetToBounds(rawTarget, currentBoundary);
                    }
                    else
                    {
                        targetPosition = rawTarget;
                    }
                    transitionStartTarget = targetPosition;
                }
            }
            else
            {
                transitionStartTarget = targetPosition;
                boundaryTransitionTimer = 0f;
            }
        }
    }

    private void SortActiveChunks()
    {
        if (activeChunks.Count <= 1) return;

      
        activeChunks.Sort((a, b) =>
        {
            if (a == null || b == null) return 0;

            float areaA = a.bounds.size.x * a.bounds.size.y;
            float areaB = b.bounds.size.x * b.bounds.size.y;

            return areaB.CompareTo(areaA);
        });
    }

    public void SetBoundary(Collider2D newBoundary)
    {
        TransitionToBoundary(newBoundary);

        if (!activeChunks.Contains(newBoundary))
        {
            activeChunks.Add(newBoundary);
            if (skipTransitionTimer > 0f)
            {
                SortActiveChunks();
            }
        }

        TransitionToBoundary(activeChunks[activeChunks.Count - 1]);
    }

    public void ClearBoundary(Collider2D oldBoundary)
    {
        if (PlayerController.Instance != null)
        {
            PlayerState playerState = PlayerController.Instance.GetComponent<PlayerState>();
            if (playerState != null && playerState.hit) return; // Keep camera locked [1.2.3]
        }

        if (activeChunks.Contains(oldBoundary))
        {
            activeChunks.Remove(oldBoundary);
        }

        if (activeChunks.Count > 0)
        {
            TransitionToBoundary(activeChunks[activeChunks.Count - 1]);
        }
        else
        {
            TransitionToBoundary(null);
        }
    }

    private void ScanForActiveChunks()
    {
        if (PlayerController.Instance == null) return;

        Vector2 playerPos = PlayerController.Instance.transform.position;
        bool foundAny = false;

        Collider2D[] colliders = Physics2D.OverlapPointAll(playerPos);
        foreach (var col in colliders)
        {
            CameraChunk chunk = col.GetComponent<CameraChunk>();
            if (chunk != null)
            {
                if (!activeChunks.Contains(col))
                {
                    activeChunks.Add(col);
                    foundAny = true;
                }
            }
        }

        if (foundAny)
        {
            if (skipTransitionTimer > 0f)
            {
                SortActiveChunks();
            }
            TransitionToBoundary(activeChunks[activeChunks.Count - 1]);
        }
    }

    private void ForceImmediateChunkScan()
    {
        activeChunks.Clear();
        if (PlayerController.Instance == null) return;

        Vector2 playerPos = PlayerController.Instance.transform.position;
        Collider2D[] colliders = Physics2D.OverlapPointAll(playerPos);
        foreach (var col in colliders)
        {
            CameraChunk chunk = col.GetComponent<CameraChunk>();
            if (chunk != null)
            {
                if (!activeChunks.Contains(col))
                {
                    activeChunks.Add(col);
                }
            }
        }

        if (activeChunks.Count > 0)
        {
            SortActiveChunks();
            currentBoundary = activeChunks[activeChunks.Count - 1];
        }
        else
        {
            currentBoundary = null;
        }
    }

    public void InstantSnap(Collider2D overrideBoundary = null)
    {
        if (PlayerController.Instance == null) return;

        skipTransitionTimer = 1.0f;

        activeChunks.Clear();
        previousBoundary = null;
        boundaryTransitionTimer = 1f;

        if (overrideBoundary != null)
        {
            activeChunks.Add(overrideBoundary);
            currentBoundary = overrideBoundary;
        }
        else
        {
            ForceImmediateChunkScan();
        }

        Vector3 playerPos = PlayerController.Instance.transform.position;
        groundedYTarget = playerPos.y;

        highSpeedBlendX = 0f;
        highSpeedBlendY = 0f;

        Vector3 rawTarget = new Vector3(playerPos.x, groundedYTarget, playerPos.z) + currentOffset;

        if (currentBoundary != null)
        {
            targetPosition = ClampTargetToBounds(rawTarget, currentBoundary);
        }
        else
        {
            targetPosition = rawTarget;
        }

        transitionStartTarget = targetPosition;
        boundaryTransitionTimer = 1f;
        transform.position = targetPosition; 

        velocityX = 0f;
        velocityY = 0f;
    }

    public void RegisterZone(CameraZone zone, float offset)
    {
        if (!activeZones.Contains(zone))
        {
            activeZones.Add(zone);
        }
        targetOffsetY = offset;
    }

    public void UnregisterZone(CameraZone zone)
    {
        if (activeZones.Contains(zone))
        {
            activeZones.Remove(zone);
        }

        if (activeZones.Count > 0)
        {
            targetOffsetY = activeZones[activeZones.Count - 1].GetCustomYOffset();
        }
        else
        {
            targetOffsetY = baseOffset.y;
        }
    }

    public void Shake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }

    private Vector3 ClampTargetToBounds(Vector3 target, Collider2D boundaryCollider)
    {
        if (boundaryCollider == null) return target; 

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float minX = boundaryCollider.bounds.min.x + halfWidth;
        float maxX = boundaryCollider.bounds.max.x - halfWidth;
        float minY = boundaryCollider.bounds.min.y + halfHeight;
        float maxY = boundaryCollider.bounds.max.y - halfHeight;

        if (maxX > minX) target.x = Mathf.Clamp(target.x, minX, maxX);
        else target.x = (boundaryCollider.bounds.min.x + boundaryCollider.bounds.max.x) / 2f;

        if (maxY > minY) target.y = Mathf.Clamp(target.y, minY, maxY);
        else target.y = (boundaryCollider.bounds.min.y + boundaryCollider.bounds.max.y) / 2f;

        return target;
    }
}