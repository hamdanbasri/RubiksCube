using UnityEngine;
using System.Collections.Generic;

public class RubiksCubeInteraction : MonoBehaviour
{
    public float rotationSpeed = 90f;
    public float dragThreshold = 50f;

    private Quaternion targetRotation;
    private bool isRotating = false;

    private Vector3 dragStartPosition;
    private Vector3 dragEndPosition;

    private Vector3 activeFaceNormal; // Normal of the face being interacted with
    private List<Transform> cubesInGroup = new List<Transform>(); // Dynamically detected cubes to rotate

    private Vector3 cubeSize = new Vector3(1f, 1f, 1f); // Approximate size of a cube for neighbor detection

    void Update()
    {
        if (isRotating) return;

        if (Input.GetMouseButtonDown(0))
        {
            // Raycast to detect the face being touched
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                dragStartPosition = Input.mousePosition;

                // Determine the face normal
                activeFaceNormal = hit.normal;

                Debug.Log($"Active Face Normal: {activeFaceNormal}");
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            dragEndPosition = Input.mousePosition;
            Vector3 delta = dragEndPosition - dragStartPosition;

            if (delta.magnitude >= dragThreshold)
            {
                Vector3 rotationAxis = DetectRotationAxis(delta, activeFaceNormal);

                if (rotationAxis != Vector3.zero)
                {
                    // Identify cubes dynamically before starting rotation
                    cubesInGroup = GetCubesInGroup(activeFaceNormal);

                    if (cubesInGroup.Count > 0)
                    {
                        ParentCubes();
                        StartGroupRotation(rotationAxis, 90f);
                    }
                }
            }

            dragStartPosition = Vector3.zero;
            dragEndPosition = Vector3.zero;
        }

        if (isRotating)
        {
            RotateGroup();
        }
    }

    private List<Transform> GetCubesInGroup(Vector3 faceNormal)
    {
        List<Transform> group = new List<Transform>();

        Collider[] allCubes = Physics.OverlapBox(transform.position, cubeSize * 1.5f); // Detect nearby cubes

        foreach (Collider cube in allCubes)
        {
            // Check alignment with the selected face's plane
            Vector3 relativePosition = cube.transform.position - transform.position;
            if (Mathf.Abs(Vector3.Dot(relativePosition, faceNormal)) < 0.1f)
            {
                group.Add(cube.transform);
            }
        }

        Debug.Log($"Cubes in Group: {group.Count}");
        return group;
    }

    private Vector3 DetectRotationAxis(Vector3 swipeDelta, Vector3 faceNormal)
    {
        Vector2 swipeDirection = new Vector2(swipeDelta.x, swipeDelta.y).normalized;

        // Determine axis based on face normal and swipe direction
        if (Vector3.Dot(faceNormal, Vector3.up) > 0.9f || Vector3.Dot(faceNormal, Vector3.down) > 0.9f)
        {
            // Swiping on the top/bottom face
            return Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y)
                ? Vector3.up // Horizontal swipe
                : Vector3.forward; // Vertical swipe
        }
        else if (Vector3.Dot(faceNormal, Vector3.forward) > 0.9f || Vector3.Dot(faceNormal, Vector3.back) > 0.9f)
        {
            // Swiping on the front/back face
            return Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y)
                ? Vector3.right // Horizontal swipe
                : Vector3.up; // Vertical swipe
        }
        else if (Vector3.Dot(faceNormal, Vector3.right) > 0.9f || Vector3.Dot(faceNormal, Vector3.left) > 0.9f)
        {
            // Swiping on the left/right face
            return Mathf.Abs(swipeDirection.x) > Mathf.Abs(swipeDirection.y)
                ? Vector3.forward // Horizontal swipe
                : Vector3.up; // Vertical swipe
        }

        return Vector3.zero;
    }

    private void ParentCubes()
    {
        foreach (Transform cube in cubesInGroup)
        {
            cube.SetParent(transform);
        }
    }

    private void UnparentCubes()
    {
        foreach (Transform cube in cubesInGroup)
        {
            cube.SetParent(null);
        }

        cubesInGroup.Clear();
    }

    private void StartGroupRotation(Vector3 axis, float angle)
    {
        targetRotation = Quaternion.AngleAxis(angle, axis) * transform.rotation;
        isRotating = true;

        Debug.Log("Starting group rotation");
    }

    private void RotateGroup()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
        {
            transform.rotation = targetRotation;
            isRotating = false;

            Debug.Log("Rotation complete");

            UnparentCubes();
        }
    }
}
