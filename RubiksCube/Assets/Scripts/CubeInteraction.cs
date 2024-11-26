using UnityEngine;
using System.Collections.Generic;

public class CubeInteraction : MonoBehaviour
{
    public Color selectedColor = Color.red;
    public Color originalColor = Color.white;
    public float rotationSpeed = 90f;
    public float dragThreshold = 50f; // Minimum distance for a drag to register as a rotation

    private static CubeInteraction currentSelectedCube = null;
    private static bool isAnyCubeRotating = false;

    private bool isSelected = false;
    private Renderer cubeRenderer;
    private Quaternion targetRotation;
    private bool isRotating = false;

    private Vector3 dragStartPosition;
    private Vector3 dragEndPosition;
    private List<Transform> childrenDuringRotation = new List<Transform>();

    void Start()
    {
        cubeRenderer = GetComponent<Renderer>();
        cubeRenderer.material.color = originalColor;
        targetRotation = transform.rotation;
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
                    dragStartPosition = Input.mousePosition; // Start tracking drag
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

        if (Input.GetMouseButtonUp(0) && isSelected)
        {
            dragEndPosition = Input.mousePosition;
            Vector3 delta = dragEndPosition - dragStartPosition;

            if (delta.magnitude >= dragThreshold)
            {
                // Detect swipe direction
                Vector2 swipeDirection = new Vector2(delta.x, delta.y).normalized;
                if (swipeDirection.y > 0.5f)
                {
                    StartRotation(90); // Rotate clockwise
                }
                else if (swipeDirection.y < -0.5f)
                {
                    StartRotation(-90); // Rotate counterclockwise
                }
            }

            // Reset drag tracking
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

    private void StartRotation(float angle)
    {
        if (isAnyCubeRotating) return;

        targetRotation *= Quaternion.Euler(Vector3.forward * angle);
        isRotating = true;
        isAnyCubeRotating = true;

        Collider[] colliders = Physics.OverlapBox(transform.position, transform.localScale / 2, transform.rotation);
        foreach (Collider collider in colliders)
        {
            // Do not parent cubes with the CubeInteraction script
            if (collider != null && collider.transform != transform && 
                !childrenDuringRotation.Contains(collider.transform) &&
                collider.GetComponent<CubeInteraction>() == null)
            {
                childrenDuringRotation.Add(collider.transform);
                collider.transform.SetParent(transform);
                Debug.Log($"{collider.gameObject.name} parented to {gameObject.name} at rotation start");
            }
        }

        Debug.Log($"Starting rotation for {gameObject.name}");
    }

    private void OnTriggerStay(Collider other)
    {
        if (isRotating)
        {
            // Do not parent cubes with the CubeInteraction script
            if (!childrenDuringRotation.Contains(other.transform) &&
                other.GetComponent<CubeInteraction>() == null)
            {
                childrenDuringRotation.Add(other.transform);
                other.transform.SetParent(transform);
                Debug.Log($"{other.gameObject.name} parented to {gameObject.name} during rotation");
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
                Debug.Log($"{child.gameObject.name} unparented from {gameObject.name}");
            }
        }
        childrenDuringRotation.Clear();
    }
}
