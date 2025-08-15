using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ObjectSpawner : MonoBehaviour
{
    public List<GameObject> objectPrefabs;
    public float placementDistance = 7f;
    public Material previewMaterial;
    public GameObject budenContainer;
    public AgentManager am;
    public GodmodeController godmodeController;
    
    public XRRayInteractor rayInteractor;
    private GameObject selectedObject = null;

    private GameObject currentPreview;
    private int selectedIndex = -1;

    public Transform handTransform;
    public Transform cameraTransform;

    public InputActionReference moveAction;
    public InputActionReference rotateAction;
    public InputActionReference confirmAction;
    public InputActionReference grabAction;

    private bool isPlacing = false;
    public bool IsPlacing => isPlacing;

    private Vector3 placementPosition;
    private float placementRotationY = 0f;
    private float rotationSpeed = 100f;

    private bool hasUsedMoveInput = false;
    private float lastRotationInput = 0f;

    [SerializeField] private AudioClip placementSoundClip;

    void Start()
    {
        budenContainer = GameObject.Find("BudenContainer");    
        am = GameObject.Find("AgentManager").GetComponent<AgentManager>();
        cameraTransform = Camera.main.transform;
        godmodeController = FindObjectOfType<GodmodeController>();

        if (godmodeController == null)
            Debug.LogError("GodmodeController wurde nicht gefunden!");
        
        if (rayInteractor == null)
            Debug.LogError("RayInteractor wurde nicht zugewiesen!");
        
        grabAction.action.started += _ => TrySelectObject();
        grabAction.action.canceled += _ => DeselectObject();
    }

    public void StartSpawning(int index)
    {
        if (index < 0 || index >= objectPrefabs.Count) return;

        isPlacing = true;
        selectedIndex = index;
        CreatePreviewObject();

        if (godmodeController != null && godmodeController.IsGodmodeActive())
        {
            godmodeController.StartPlacingObject();
        }

        Vector3 forwardOffset = cameraTransform.forward * placementDistance;
        placementPosition = new Vector3(forwardOffset.x, 0, forwardOffset.z) + cameraTransform.position;
        placementRotationY = cameraTransform.eulerAngles.y;
    }

    void Update()
    {
        if (!isPlacing && selectedObject == null) return;

        Vector3 handOffset = handTransform.position + handTransform.forward * placementDistance;
        Vector3 newPosition = new Vector3(handOffset.x, 0, handOffset.z);
        float handRotationY = handTransform.eulerAngles.y;

        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
        float rotationInput = rotateAction.action.ReadValue<float>();

        if (Mathf.Abs(rotationInput) > 0.01f) 
        {
            lastRotationInput = rotationInput;
        }

        // **Bewegung auch für bestehende Buden ermöglichen**
        if (moveInput.magnitude > 0.01f) 
{
    Vector3 forward = cameraTransform.forward;
    Vector3 right = cameraTransform.right;

    forward.y = 0; 
    right.y = 0;
    forward.Normalize();
    right.Normalize();

    Vector3 movement = (right * moveInput.x + forward * moveInput.y) * Time.deltaTime * 5f;

    if (isPlacing && currentPreview != null)
    {
        placementPosition += movement;
        hasUsedMoveInput = true;
    }
    else if (selectedObject != null)
    {
        Rigidbody rb = selectedObject.GetComponent<Rigidbody>();
        XRGrabInteractable grabInteractable = selectedObject.GetComponent<XRGrabInteractable>();

        if (rb != null)
        {
            bool wasKinematic = rb.isKinematic;
            rb.isKinematic = false; // Physik-Kollisionen ausschalten, um Bewegung zu erlauben
            selectedObject.transform.position += movement;
            rb.isKinematic = wasKinematic; // Nach Bewegung zurücksetzen
        }
        else
        {
            selectedObject.transform.position += movement;
        }

        // Falls das Objekt ein XR Grab Interactable hat, verhindern, dass es beim Greifen "gezogen" wird
        if (grabInteractable != null)
        {
            grabInteractable.movementType = XRBaseInteractable.MovementType.VelocityTracking; // Keine feste Bindung an den Controller
        }
    }
}

        else if (!hasUsedMoveInput)
        {
            placementPosition = newPosition;
        }

        // **Rotation für neue oder bestehende Bude**
        if (rotateAction.action.WasPressedThisFrame()) 
{
    float rotationStep = (lastRotationInput > 0) ? 5f : -5f; 

    if (isPlacing && currentPreview != null)
    {
        placementRotationY += rotationStep;
    }
    else if (selectedObject != null)
    {
        float currentRotation = selectedObject.transform.eulerAngles.y;
        float newRotation = Mathf.Round((currentRotation + rotationStep) / 5f) * 5f; // Auf nächste 5° runden
        selectedObject.transform.rotation = Quaternion.Euler(0, newRotation, 0);
    }

}
else if (Mathf.Abs(rotationInput) > 0.01f) 
{
    float continuousRotation = rotationInput * rotationSpeed * Time.deltaTime;
    
    if (isPlacing && currentPreview != null)
    {
        placementRotationY += continuousRotation;
    }
    else if (selectedObject != null)
    {
        selectedObject.transform.rotation *= Quaternion.Euler(0, continuousRotation, 0);
    }

}


        // **Vorschau-Objekt aktualisieren**
        if (isPlacing && currentPreview != null)
        {
            currentPreview.transform.position = placementPosition;
            currentPreview.transform.rotation = Quaternion.Euler(0, placementRotationY, 0);
        }

        if (confirmAction.action.triggered && isPlacing)
        {
            PlaceObject();
            isPlacing = false;
        }
    }

    void CreatePreviewObject()
    {
        if (currentPreview != null) Destroy(currentPreview);
        
        currentPreview = Instantiate(objectPrefabs[selectedIndex], budenContainer.transform);
        SetMaterialTransparent(currentPreview);
    }

    void PlaceObject()
    {
        GameObject newObj = Instantiate(objectPrefabs[selectedIndex], 
            placementPosition, 
            Quaternion.Euler(0, placementRotationY, 0), 
            budenContainer.transform);
        newObj.GetComponent<Buden>().SetTypeIndex(selectedIndex);
        am.AddBude(newObj.GetComponent<Buden>());

        //play Placement Sound Effect
        SoundFXManager.instance.PlaySoundFXClip(placementSoundClip, transform, 1f);

        Destroy(currentPreview);
        selectedIndex = -1;
        hasUsedMoveInput = false;

        if (godmodeController != null && godmodeController.IsGodmodeActive())
        {
            godmodeController.StopPlacingObject();
        }
    }

void SetMaterialTransparent(GameObject obj)
{
    Transform budeTransform = obj.transform.Find("Bude"); // Sucht das Unterobjekt „Bude“
    if (budeTransform != null)
    {
        Renderer budeRenderer = budeTransform.GetComponent<Renderer>();
        if (budeRenderer != null)
        {
            budeRenderer.material = previewMaterial;
        }
    }
}


    // **Objekt per Ray Interactor auswählen**
    void TrySelectObject()
{
    if (rayInteractor == null) 
    {
        Debug.LogWarning("RayInteractor ist nicht zugewiesen!");
        return;
    }

    RaycastHit hit;
    if (rayInteractor.TryGetCurrent3DRaycastHit(out hit))
    {
        Debug.Log("Raycast hat etwas getroffen: " + hit.collider.gameObject.name);

        Transform parent = hit.collider.transform;
        while (parent.parent != null && !parent.name.Contains("Stand"))
        {
            parent = parent.parent;
        }

        if (parent.name.Contains("Stand"))
        {
            selectedObject = parent.gameObject;
            Debug.Log("Objekt erfolgreich ausgewählt: " + selectedObject.name);

            XRGrabInteractable grabInteractable = selectedObject.GetComponent<XRGrabInteractable>();
            if (grabInteractable != null)
            {
                grabInteractable.movementType = XRBaseInteractable.MovementType.VelocityTracking; // Verhindert Ziehen zum Controller
                grabInteractable.attachTransform = selectedObject.transform; // Bleibt an Ort und Stelle
            }
        }
    }
}

void DeselectObject()
{
    if (selectedObject != null)
    {
        Debug.Log("Objekt losgelassen: " + selectedObject.name);

        XRGrabInteractable grabInteractable = selectedObject.GetComponent<XRGrabInteractable>();
        Rigidbody rb = selectedObject.GetComponent<Rigidbody>();
        XRGazeAssistance gazeAssist = selectedObject.GetComponent<XRGazeAssistance>();

   if (grabInteractable != null)
{
    Transform tempAttach = new GameObject("TempAttachPoint").transform;
    tempAttach.position = grabInteractable.transform.position + Vector3.up * 0.5f; // 0.5m über dem Objekt
    grabInteractable.attachTransform = tempAttach;
}

{
    try
    {
        gazeAssist.enabled = false;
    }
    catch (System.Exception e)
    {
        Debug.LogWarning("Fehler beim Deaktivieren von XRGazeAssistance: " + e.Message);
    }
}


        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        StartCoroutine(ResetGrabInteractable(grabInteractable, 0.1f));
    }

    selectedObject = null;
}

private System.Collections.IEnumerator ResetGrabInteractable(XRGrabInteractable grabInteractable, float delay)
{
    if (grabInteractable != null)
    {
        yield return new WaitForSeconds(delay);
        grabInteractable.interactionLayers = -1; // Objekt wieder für Interaktionen aktivieren
    }
}

private System.Collections.IEnumerator ResetInteractionLayer(XRGrabInteractable grabInteractable, float delay)
{
    yield return new WaitForSeconds(delay);
    if (grabInteractable != null)
    {
        grabInteractable.interactionLayers = -1; // Reaktiviert das Objekt für Interaktionen
    }
}
}


