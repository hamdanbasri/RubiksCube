using UnityEngine;
using System.Collections.Generic;

public class SingleCubeInteraction : MonoBehaviour
{
    public Color selectedColor = Color.red;
    public Color originalColor = Color.white;
    public float rotationSpeed = 90f;
    public float dragThreshold = 50f; // Minimum drag distance for swipe detection

    [Header("Swipe Direction Children")]
    public Transform upChild;
    public Transform downChild;
    public Transform leftChild;
    public Transform rightChild;

    private static SingleCubeInteraction currentSelectedCube = null;
    private static bool isAnyCubeRotating = false;

    private bool isSelected = false;
    private Renderer cubeRenderer;
    private Quaternion targetRotation;
    private bool isRotating = false;

    private Vector3 dragStartPosition;
    private Vector3 dragEndPosition;
    private LineRenderer lineRenderer;

    private List<Transform> childrenDuringRotation = new List<Transform>();
    private Transform currentSwipeChild;

    void Start()
    {
        cubeRenderer = GetComponent<Renderer>();
        cubeRenderer.material.color = originalColor;
        targetRotation = transform.rotation;

        // Add LineRenderer
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
        lineRenderer.enabled = false; // Initially disable
    }

    void Update()
    {
        if (isAnyCubeRotating && !isRotating) return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == transform && !isRotating)
                {
                    SelectCube();
                    dragStartPosition = Input.mousePosition;
                    lineRenderer.enabled = true; // Enable swipe visualization
                    lineRenderer.positionCount = 2;
                    lineRenderer.SetPosition(0, Camera.main.ScreenToWorldPoint(new Vector3(dragStartPosition.x, dragStartPosition.y, 10f)));
                }
                else if (isSelected)
                {
                    DeselectCube();
                }
            }
            else if (isSelected)
            {
                DeselectCube();
            }
        }

        if (Input.GetMouseButton(0) && isSelected)
        {
            // Update swipe visualization
            Vector3 currentMousePosition = Input.mousePosition;
            lineRenderer.SetPosition(1, Camera.main.ScreenToWorldPoint(new Vector3(currentMousePosition.x, currentMousePosition.y, 10f)));
        }

        if (Input.GetMouseButtonUp(0) && isSelected)
        {
            dragEndPosition = Input.mousePosition;
            Vector3 delta = dragEndPosition - dragStartPosition;

            if (delta.magnitude >= dragThreshold)
            {
                DetectSwipeDirection(delta);
            }

            // Reset swipe visualization
            lineRenderer.enabled = false;
            dragStartPosition = Vector3.zero;
            dragEndPosition = Vector3.zero;
        }

        if (isRotating)
        {
            float step = rotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);

            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                transform.rotation = targetRotation;
                isRotating = false;
                isAnyCubeRotating = false;
                UnparentAllChildren();
                Debug.Log($"Rotation completed for {gameObject.name}");
            }
        }
    }

    private void SelectCube()
    {
        if (currentSelectedCube != null && currentSelectedCube != this)
        {
            currentSelectedCube.DeselectCube();
        }

        isSelected = true;
        currentSelectedCube = this;
        cubeRenderer.material.color = selectedColor;
    }

    private void DeselectCube()
    {
        isSelected = false;
        cubeRenderer.material.color = originalColor;

        if (currentSelectedCube == this)
        {
            currentSelectedCube = null;
        }
    }

    private void StartRotation(float angle, Transform swipeChild, Vector3 rotationAxis)
{
    if (isAnyCubeRotating) return;

    targetRotation *= Quaternion.Euler(rotationAxis * angle);
    isRotating = true;
    isAnyCubeRotating = true;

    currentSwipeChild = swipeChild;

    Debug.Log($"Starting rotation for {gameObject.name} around axis {rotationAxis}");

    // Ensure the swipe child is parented and ready to detect other objects
    if (currentSwipeChild != null)
    {
        currentSwipeChild.SetParent(transform);
    }
}

private void DetectSwipeDirection(Vector3 delta)
{
    Vector2 swipeDirection = new Vector2(delta.x, delta.y).normalized;

    if (Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y))
    {
        if (swipeDirection.x > 0.5f)
        {
            Debug.Log("Swipe Right");
            StartRotation(90, rightChild, Vector3.up); // Rotate around Y-axis
        }
        else if (swipeDirection.x < -0.5f)
        {
            Debug.Log("Swipe Left");
            StartRotation(-90, leftChild, Vector3.up); // Rotate around Y-axis
        }
    }
    else
    {
        if (swipeDirection.y > 0.5f)
        {
            Debug.Log("Swipe Up");
            StartRotation(90, upChild, Vector3.forward); // Rotate around X-axis
        }
        else if (swipeDirection.y < -0.5f)
        {
            Debug.Log("Swipe Down");
            StartRotation(-90, downChild, Vector3.forward); // Rotate around X-axis
        }
    }
}


    private void OnTriggerStay(Collider other)
    {
        if (isRotating && currentSwipeChild != null)
        {
            if (!childrenDuringRotation.Contains(other.transform) &&
                other.GetComponent<CubeInteraction>() == null)
            {
                childrenDuringRotation.Add(other.transform);
                other.transform.SetParent(currentSwipeChild);
                //Debug.Log($"{other.gameObject.name} parented to {currentSwipeChild.name} during rotation");
            }
        }
    }

    private void UnparentAllChildren()
    {
        foreach (Transform child in childrenDuringRotation)
        {
            if (child != null)
            {
                child.SetParent(null);
                Debug.Log($"{child.gameObject.name} unparented from {currentSwipeChild.name}");
            }
        }
        childrenDuringRotation.Clear();

        if (currentSwipeChild != null)
        {
            currentSwipeChild.SetParent(null);
        }
    }
}
