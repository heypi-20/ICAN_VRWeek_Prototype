using UnityEngine;

/// <summary>
/// Detects and snaps to the nearest surface of another object, ensuring alignment and continuous attachment
/// until the raycast no longer detects the target object.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SurfaceSnapper : MonoBehaviour
{
    [Header("Snap Configuration")]
    [Tooltip("Tag de l'objet cible à connecter.")]
    public string targetTag = "SnapObject";

    [Tooltip("Distance maximale autorisée pour activer le snap.")]
    public float snapThreshold = 0.1f;

    [Tooltip("Décalage pour ajuster la position des rayons par rapport au centre de la face.")]
    public float cornerOffset = 0.05f;

    [Tooltip("Permet une légère tolérance d'alignement (en degrés).")]
    public float rotationTolerance = 5f;

    private BoxCollider boxCollider;
    private Vector3[][] raycastOrigins;  // Positions des coins des faces
    private Vector3[] faceNormals;       // Normales des faces
    private bool isSnapping = false;

    private void Start()
    {
        boxCollider = GetComponentInChildren<BoxCollider>();

        // Définir les normales des six faces du cube (+X, -X, +Y, -Y, +Z, -Z)
        faceNormals = new Vector3[]
        {
            Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back
        };

        // Initialiser les coins de chaque face (+X, -X, +Y, -Y, +Z, -Z)
        raycastOrigins = new Vector3[6][]
        {
            new Vector3[] { new Vector3(1, 1, 0), new Vector3(1, -1, 0), new Vector3(1, 1, 1), new Vector3(1, -1, 1) },  // +X
            new Vector3[] { new Vector3(-1, 1, 0), new Vector3(-1, -1, 0), new Vector3(-1, 1, 1), new Vector3(-1, -1, 1) }, // -X
            new Vector3[] { new Vector3(1, 1, 0), new Vector3(-1, 1, 0), new Vector3(1, 1, 1), new Vector3(-1, 1, 1) },  // +Y
            new Vector3[] { new Vector3(1, -1, 0), new Vector3(-1, -1, 0), new Vector3(1, -1, 1), new Vector3(-1, -1, 1) }, // -Y
            new Vector3[] { new Vector3(1, 0, 1), new Vector3(-1, 0, 1), new Vector3(1, 1, 1), new Vector3(-1, 1, 1) },  // +Z
            new Vector3[] { new Vector3(1, 0, -1), new Vector3(-1, 0, -1), new Vector3(1, 1, -1), new Vector3(-1, 1, -1) }  // -Z
        };
    }

    private void Update()
    {
        bool hitDetected = false;

        for (int i = 0; i < faceNormals.Length; i++)
        {
            foreach (Vector3 offset in raycastOrigins[i])
            {
                // Lancer un rayon depuis chaque coin de la face
                if (CastSnapRay(offset, faceNormals[i]))
                {
                    hitDetected = true;
                }
            }
        }

        // Si aucune des rayons ne détecte le Tag cible, on arrête le snap
        if (!hitDetected && isSnapping)
        {
            Debug.Log("Snap lost. Disconnecting.");
            isSnapping = false;
        }
    }

    private bool CastSnapRay(Vector3 offset, Vector3 faceNormal)
    {
        Vector3 boxCenter = boxCollider.bounds.center;
        Vector3 halfExtents = boxCollider.bounds.extents;
        Vector3 rayOrigin = boxCenter + Vector3.Scale(offset, halfExtents) + faceNormal * cornerOffset;

        Debug.DrawRay(rayOrigin, faceNormal * (halfExtents.magnitude + snapThreshold), Color.red);

        if (Physics.Raycast(rayOrigin, faceNormal, out RaycastHit hit, halfExtents.magnitude + snapThreshold))
        {
            if (hit.collider.CompareTag(targetTag))
            {
                SnapToSurface(hit, faceNormal);
                return true;
            }
        }

        return false;
    }

    private void SnapToSurface(RaycastHit hit, Vector3 faceNormal)
    {
        if (!isSnapping)
        {
            Debug.Log("Snap initiated.");
            isSnapping = true;
        }

        // Aligner la position pour que le centre de la face soit aligné avec la surface cible
        Vector3 snapPosition = hit.point - faceNormal * (boxCollider.size.magnitude * 0.5f);
        transform.position = Vector3.Lerp(transform.position, snapPosition, Time.deltaTime * 10f);

        // Aligner la rotation pour que la face soit perpendiculaire à la normale de la surface
        Quaternion targetRotation = Quaternion.LookRotation(-hit.normal, Vector3.up);
        if (Quaternion.Angle(transform.rotation, targetRotation) <= rotationTolerance)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }
}
