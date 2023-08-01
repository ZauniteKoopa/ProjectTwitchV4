using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfVision : MonoBehaviour
{
    // View angle
    [Range(0f, 180f)]
    public float viewAngle = 45f;

    // Layer masks
    [SerializeField]
    private LayerMask obstacleMask;

    // Selected player target if he's nearby
    private PlayerStatus nearbyPlayer;

    // FOV Mesh
    [SerializeField]
    [Min(0.01f)]
    private float meshResolution;
    [SerializeField]
    private MeshFilter viewMeshFilter;
    private Mesh viewMesh;

    private bool shown = true;


    // On Start, set up mesh
    private void Start() {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter.mesh = viewMesh;
    }


    // On update
    private void LateUpdate() {
        if (shown) {
            drawFieldOfVision();
        }
    }
    
    
    // Main function to convert degree angle to a trig direction (assuming that the transform forward is 0)
    public Vector3 dirFromAngle(float angleInDeg, bool angleIsGlobal) {
        if (!angleIsGlobal) {
            angleInDeg += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDeg * Mathf.Deg2Rad), 0f, Mathf.Cos(angleInDeg * Mathf.Deg2Rad));
    }


    // Private helper function to see if you can actually see the unit
    //  Post: returns whether or not player is in sight range of enemy AND no objects are blocking
    public bool canSeePlayer() {
        if (nearbyPlayer == null) {
            return false;
        }

        // Angle check
        Vector3 targetPosition = nearbyPlayer.transform.position;
        Vector3 rayDir = targetPosition - transform.position;
        Vector3 flattenRayDir = Vector3.ProjectOnPlane(rayDir, Vector3.up);
        if (Vector3.Angle(flattenRayDir, transform.forward) > viewAngle / 2f) {
            return false;
        }

        // Get information for the ray: you can see the player if there are no barriers between player and enemy
        float rayDist = rayDir.magnitude;
        rayDir.Normalize();
        bool seePlayer = !Physics.Raycast(transform.position, rayDir, rayDist, obstacleMask);

        // Return whether or not ray cast dir met any barriers
        return seePlayer;
    }

    // Event handler function for when player has entered the sense box
    private void OnTriggerEnter(Collider collider) {
        PlayerStatus testPlayer = collider.GetComponent<PlayerStatus>();

        if (testPlayer != null) {
            nearbyPlayer = testPlayer;
        }
    }


    // Event handler function for when player has exited the sense box
    private void OnTriggerExit(Collider collider) {
        PlayerStatus testPlayer = collider.GetComponent<PlayerStatus>();

        if (testPlayer != null) {
            nearbyPlayer = null;
        }
    }


    // Main function to show vision
    public void showVision(bool willShow) {
        shown = willShow;
    }

    
    // Main function to draw the field of view
    private void drawFieldOfVision() {
        // Calculate vertex positions in steps
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float stepAngleSize = viewAngle / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();

        for (int s = 0; s < stepCount; s++) {
            // Calculate angle
            float angle = transform.eulerAngles.y - (viewAngle / 2f) + stepAngleSize * s;

            // Calculate mesh points through raycasting
            Vector3 rayDir = dirFromAngle(angle, true);
            float curDist = transform.lossyScale.x / 2f;
            RaycastHit hitInfo;
            if (Physics.Raycast(transform.position, rayDir, out hitInfo, curDist, obstacleMask)) {
                curDist = hitInfo.distance;
            }

            // Calculate raydest and add
            Vector3 rayDest = (rayDir * curDist) + transform.position;
            viewPoints.Add(rayDest);
        }

        // Draw the triangles (ALL POINTS ARE LOCAL)
        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[viewPoints.Count + 1];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int v = 1; v < vertexCount; v++) {
            vertices[v] = transform.InverseTransformPoint(viewPoints[v - 1]);

            // Don't go out of bounds when setting triangle vertices
            if (v < vertexCount - 2) {
                triangles[v * 3] = 0;
                triangles[v * 3 + 1] = v + 1;
                triangles[v * 3 + 2] = v + 2;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }
}
