using UnityEngine;

public class TouchCameraController : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform targetToLookAt;
    public float distanceFromTarget = 10f;
    
    [Header("Orbit Settings")]
    public float rotationSpeed = 1f;
    public float minVerticalAngle = -80f;
    public float maxVerticalAngle = 80f;
    public LayerMask objectsToIgnore;

    private float currentRotationX = 0f;
    private float currentRotationY = 0f;
    private bool isDragging = false;
    private Vector2 lastInputPosition;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        if (targetToLookAt == null)
        {
            Debug.LogWarning("No target assigned to camera controller!");
            return;
        }

        // Initialize camera position and rotation
        InitializeCamera();
    }

    private void InitializeCamera()
    {
        // Start at a 45-degree angle above and behind the target
        currentRotationX = 180f; // Facing forward
        currentRotationY = 30f;  // Looking down at 30 degrees
        
        UpdateCameraPosition();
    }

    private void Update()
    {
        if (targetToLookAt == null) return;

        #if UNITY_EDITOR || UNITY_STANDALONE
            HandleMouseInput();
        #else
            HandleTouchInput();
        #endif

        UpdateCameraPosition();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (!Physics.Raycast(ray, out hit, Mathf.Infinity, ~objectsToIgnore))
            {
                isDragging = true;
                lastInputPosition = Input.mousePosition;
            }
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            Vector2 currentMousePosition = Input.mousePosition;
            HandleRotation(currentMousePosition);
            lastInputPosition = currentMousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    Ray ray = mainCamera.ScreenPointToRay(touch.position);
                    RaycastHit hit;
                    
                    if (!Physics.Raycast(ray, out hit, Mathf.Infinity, ~objectsToIgnore))
                    {
                        isDragging = true;
                        lastInputPosition = touch.position;
                    }
                    break;

                case TouchPhase.Moved:
                    if (isDragging)
                    {
                        HandleRotation(touch.position);
                        lastInputPosition = touch.position;
                    }
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    isDragging = false;
                    break;
            }
        }
    }

    private void HandleRotation(Vector2 currentInputPosition)
    {
        Vector2 inputDelta = currentInputPosition - lastInputPosition;
        
        // Update rotation values with scaled input
        currentRotationX += inputDelta.x * rotationSpeed;
        currentRotationY -= inputDelta.y * rotationSpeed;
        
        // Clamp vertical rotation
        currentRotationY = Mathf.Clamp(currentRotationY, minVerticalAngle, maxVerticalAngle);
    }

    private void UpdateCameraPosition()
    {
        if (targetToLookAt == null) return;

        // Convert angles to radians
        float horizontalRadians = currentRotationX * Mathf.Deg2Rad;
        float verticalRadians = currentRotationY * Mathf.Deg2Rad;

        // Calculate new position using spherical coordinates
        Vector3 offset = new Vector3(
            Mathf.Sin(horizontalRadians) * Mathf.Cos(verticalRadians),
            Mathf.Sin(verticalRadians),
            Mathf.Cos(horizontalRadians) * Mathf.Cos(verticalRadians)
        );

        // Apply distance and set position
        Vector3 newPosition = targetToLookAt.position + offset * distanceFromTarget;

        // Verify the position is valid before applying
        if (!float.IsNaN(newPosition.x) && !float.IsNaN(newPosition.y) && !float.IsNaN(newPosition.z))
        {
            transform.position = newPosition;
            transform.LookAt(targetToLookAt.position);
        }
        else
        {
            Debug.LogWarning("Invalid camera position calculated. Skipping update.");
        }
    }
}