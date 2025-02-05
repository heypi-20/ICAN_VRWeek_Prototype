using UnityEngine;

/// <summary>
/// Detect and snap to the nearest surface of another object with the specified tag,
/// aligning both position and rotation while allowing a small tolerance.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SurfaceSnapper : MonoBehaviour
{
    [Header("Snap Configuration")]
    [Tooltip("Tag de l'objet cible à connecter.")]
    public string targetTag = "SnapObject";

    [Tooltip("Distance maximale autorisée pour activer le snap.")]
    public float snapThreshold = 0.1f;

    [Tooltip("Valeur de décalage pour ajuster la position du rayon par rapport au centre de la face.")]
    public float cornerOffset = 0.05f;

    [Tooltip("Permet une légère tolérance d'alignement (en degrés).")]
    public float rotationTolerance = 5f;

    private BoxCollider boxCollider;
    private Vector3[] raycastDirections;

    private void Start()
    {
        boxCollider = GetComponentInChildren<BoxCollider>();

        /*
         * Définir les directions des rayons qui vont être tirés depuis chaque coin des faces.
         * On calcule les 4 coins de chaque face principale (+X, -X, +Y, -Y, +Z, -Z).
         */
        raycastDirections = new Vector3[]
        {
            Vector3.right, Vector3.left, Vector3.up, Vector3.down, Vector3.forward, Vector3.back
        };
    }

    private void Update()
    {
        foreach (Vector3 direction in raycastDirections)
        {
            CastSnapRay(direction);
        }
    }

    private void CastSnapRay(Vector3 direction)
    {
        // Calculer la position de départ du rayon depuis le centre, avec un décalage vers les coins
        Vector3 boxCenter = boxCollider.bounds.center;
        Vector3 halfExtents = boxCollider.bounds.extents;
        Vector3 rayOrigin = boxCenter + Vector3.Scale(direction, halfExtents) + (direction * cornerOffset);

        // Dessiner un rayon pour la visualisation (debug)
        Debug.DrawRay(rayOrigin, direction * (halfExtents.magnitude + snapThreshold), Color.red);

        // Lancer le rayon pour vérifier les collisions
        if (Physics.Raycast(rayOrigin, direction, out RaycastHit hit, halfExtents.magnitude + snapThreshold))
        {
            // Vérifier si l'objet touché a le bon Tag
            if (hit.collider.CompareTag(targetTag))
            {
                SnapToSurface(hit, direction);
            }
        }
    }

    private void SnapToSurface(RaycastHit hit, Vector3 direction)
    {
        /*
         * Aligner la position du centre de notre face avec celle de l'autre objet.
         * La position cible est le point d'impact du rayon sur la face opposée.
         */
        Vector3 targetPosition = hit.point - direction * (boxCollider.size.magnitude * 0.5f);

        // Placer notre objet au point de snap calculé
        transform.position = targetPosition;

        /*
         * Aligner la rotation pour que les deux faces soient parallèles.
         * On vérifie si la différence d'orientation est dans la tolérance définie.
         */
        Quaternion targetRotation = Quaternion.LookRotation(-direction);
        if (Quaternion.Angle(transform.rotation, targetRotation) <= rotationTolerance)
        {
            transform.rotation = targetRotation;
        }
    }
}
