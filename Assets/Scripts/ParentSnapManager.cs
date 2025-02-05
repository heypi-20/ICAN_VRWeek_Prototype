using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Gère le Snap entre plusieurs objets en corrigeant d'abord la rotation.
/// </summary>
public class ParentSnapManager : MonoBehaviour
{
    [Header("Configuration du Snap")]
    [Tooltip("Tag de l'objet cible à connecter.")]
    public string targetTag = "SnapObject";

    [Tooltip("Vitesse du déplacement vers le centre de la zone Trigger.")]
    public float snapSpeed = 5f;

    [Tooltip("Vitesse de la rotation pour corriger l'orientation.")]
    public float rotationSpeed = 2f;

    private List<BoxCollider> triggerZones;
    private Transform snappingObject = null;  // Référence à l'objet en cours de snap
    private Vector3 targetPosition;
    private bool rotationCompleted = false;  // Indique si la rotation est terminée

    private void Start()
    {
        // Rechercher tous les BoxColliders dans les enfants qui sont des zones Trigger
        triggerZones = new List<BoxCollider>(GetComponentsInChildren<BoxCollider>());
    }

    private void Update()
    {
        if (snappingObject != null)
        {
            // Mise à jour dynamique de la position cible
            targetPosition = FindNearestTriggerZone(snappingObject.position).bounds.center;

            if (rotationCompleted)
            {
                // Déplacement progressif vers le centre de la zone Trigger
                snappingObject.position = Vector3.Lerp(snappingObject.position, targetPosition, Time.deltaTime * snapSpeed);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Vérifier si l'objet entrant a le tag correct
        if (other.CompareTag(targetTag) && snappingObject == null)
        {
            // Trouver la zone Trigger la plus proche
            BoxCollider nearestTriggerZone = FindNearestTriggerZone(other.transform.position);
            if (nearestTriggerZone != null)
            {
                snappingObject = other.transform;
                targetPosition = nearestTriggerZone.bounds.center;  // Position cible du snap

                // Lancer la correction de rotation avant de continuer le déplacement
                StartCoroutine(CorrectRotation(snappingObject));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Si l'objet quitte le Trigger, annuler le snap
        if (other.CompareTag(targetTag) && other.transform == snappingObject)
        {
            Debug.Log($"Object {snappingObject.name} disconnected.");
            snappingObject = null;
            rotationCompleted = false;  // Réinitialiser l'état de la rotation
        }
    }

    private BoxCollider FindNearestTriggerZone(Vector3 position)
    {
        BoxCollider nearestZone = null;
        float minDistance = float.MaxValue;

        // Parcourir toutes les zones Trigger pour trouver celle la plus proche
        foreach (BoxCollider zone in triggerZones)
        {
            float distance = Vector3.Distance(position, zone.bounds.center);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestZone = zone;
            }
        }

        return nearestZone;
    }

    private IEnumerator CorrectRotation(Transform target)
    {
        rotationCompleted = false;  // Réinitialiser l'état de la rotation

        // Calculer la rotation corrigée (arrondie à 90 degrés)
        Vector3 currentRotation = target.eulerAngles;
        Vector3 correctedRotation = new Vector3(
            Mathf.Round(currentRotation.x / 90) * 90,
            Mathf.Round(currentRotation.y / 90) * 90,
            Mathf.Round(currentRotation.z / 90) * 90
        );

        Quaternion initialRotation = target.rotation;
        Quaternion targetRotation = Quaternion.Euler(correctedRotation);

        float elapsedTime = 0f;
        while (Quaternion.Angle(target.rotation, targetRotation) > 0.1f)
        {
            elapsedTime += Time.deltaTime * rotationSpeed;
            target.rotation = Quaternion.Lerp(initialRotation, targetRotation, elapsedTime);
            yield return null;
        }

        // Assurez-vous que la rotation finale est correcte
        target.rotation = targetRotation;
        Debug.Log($"Rotation corrected to {correctedRotation}");

        // Rotation terminée, permettre le déplacement
        rotationCompleted = true;
        Debug.Log("Rotation completed, starting snap movement...");
    }
}
