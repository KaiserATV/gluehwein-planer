using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class GodmodeController : MonoBehaviour
{
    public GameObject xrRig;
    public float moveSpeed = 3f;
    public float verticalSpeed = 2f;
    public float transitionDuration = 1f;
    public float groundThreshold = 0.5f; // Höhe für automatisches Beenden des Godmode
    public float cameraStandardHeight = 1.8f; // Standardhöhe der Kamera in Bodenperspektive
    public float godmodeLiftHeight = 5f; // Wie weit man angehoben wird, wenn der Godmode aktiviert wird

    public float vignetteSizeGodmode = 0.85f;  // Standardgröße in Godmode
    public float vignetteSizeDuringReset = 0.65f;  // Vignette beim Zurücksetzen

    private bool isGodmodeActive = false;
    private bool isGrabbingObject = false;
    private bool isPlacingObject = false;
    private bool canExitGodmode = false;

    public InputActionReference verticalMoveAction;
    public InputActionReference horizontalMoveAction;
    public InputActionReference grabAction;

    public TeleportationProvider teleportProvider;
    public LocomotionProvider snapTurnProvider;
    public XRRayInteractor teleportRayInteractor;

    public LocomotionVignetteProvider godmodeVignetteProvider;
    public TunnelingVignetteController tunnelingVignetteController;

    private Vector3 originalPosition;

    [SerializeField] private AudioClip godmodeActivateSound;
    [SerializeField] private AudioClip godmodeDeactivateSound;

    void Start()
    {
        if (grabAction != null)
        {
            grabAction.action.started += _ => StartGrabbing();
            grabAction.action.canceled += _ => StopGrabbing();
        }
    }

    void Update()
{
    if (isGodmodeActive)
    {
        if (!isGrabbingObject && !isPlacingObject)
        {
            MoveGodmode();
        }

        // Nach 2 Sekunden darf der Godmode durch Bodennähe beendet werden
        if (canExitGodmode && xrRig.transform.position.y <= groundThreshold)
        {
            ExitGodmodeAtCurrentPosition();
        }
    }
}


    public IEnumerator ToggleGodmode()
    {
        isGodmodeActive = !isGodmodeActive;

        // Play sound effect
        if (isGodmodeActive)
        {
            if(godmodeActivateSound != null)
                SoundFXManager.instance.PlaySoundFXClip(godmodeActivateSound, transform, 1f);
        }
        else
        {
            if(godmodeDeactivateSound != null)
                SoundFXManager.instance.PlaySoundFXClip(godmodeDeactivateSound, transform, 1f);
        }

        if (teleportRayInteractor != null)
        {
            teleportRayInteractor.enabled = !isGodmodeActive;
        }

        if (isGodmodeActive)
        {
            originalPosition = xrRig.transform.position;
            canExitGodmode = false;

            // Heben des Spielers um die definierte Höhe
            Vector3 targetPosition = originalPosition + Vector3.up * godmodeLiftHeight;
            yield return StartCoroutine(SmoothTransition(targetPosition));

            SetVignetteSize(vignetteSizeGodmode); // Nutzt den Wert aus dem Inspektor

            yield return new WaitForSeconds(2f); // Erst nach 2 Sekunden kann der Godmode beendet werden
            canExitGodmode = true;
        }
        else
        {
            yield return StartCoroutine(TransitionOutOfGodmode());
        }
    }

    IEnumerator TransitionOutOfGodmode()
    {
        SetVignetteSize(vignetteSizeDuringReset); // Vignette beim Zurücksetzen aus dem Inspektor nutzen
        yield return StartCoroutine(SmoothTransition(originalPosition)); // Zurück zum Startpunkt
        tunnelingVignetteController.EndTunnelingVignette(godmodeVignetteProvider); // Vignette deaktivieren

        if (teleportProvider != null) teleportProvider.enabled = true;
        if (snapTurnProvider != null) snapTurnProvider.enabled = true;
    }

    void ExitGodmodeAtCurrentPosition()
    {
        isGodmodeActive = false;
        canExitGodmode = false;

        // Behalte die aktuelle Position, aber setze die Höhe auf Standard-Kamerahöhe
        Vector3 newPosition = xrRig.transform.position;
        newPosition.y = cameraStandardHeight;
        xrRig.transform.position = newPosition;

        tunnelingVignetteController.EndTunnelingVignette(godmodeVignetteProvider); // Vignette sofort deaktivieren

        if (teleportRayInteractor != null)
        {
            teleportRayInteractor.enabled = true;
        }

        if (teleportProvider != null) teleportProvider.enabled = true;
        if (snapTurnProvider != null) snapTurnProvider.enabled = true;
    }

    IEnumerator SmoothTransition(Vector3 targetPosition)
    {
        Vector3 startPosition = xrRig.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            xrRig.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / transitionDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        xrRig.transform.position = targetPosition;
    }

    void MoveGodmode()
    {
        if (!isGrabbingObject && !isPlacingObject)
        {
            float verticalMove = verticalMoveAction.action.ReadValue<float>() * verticalSpeed * Time.deltaTime;
            Vector3 verticalMovement = Vector3.up * verticalMove;

            Vector2 horizontalInput = horizontalMoveAction.action.ReadValue<Vector2>();
            Transform cameraTransform = Camera.main.transform;

            Vector3 moveDirection = (cameraTransform.forward * horizontalInput.y + cameraTransform.right * horizontalInput.x);
            moveDirection.y = 0;
            moveDirection.Normalize();

            Vector3 horizontalMove = moveDirection * moveSpeed * Time.deltaTime;

            xrRig.transform.position += horizontalMove + verticalMovement;
        }
    }

    void SetVignetteSize(float size)
    {
        if (godmodeVignetteProvider == null) return;

        godmodeVignetteProvider.overrideDefaultParameters = true;
        godmodeVignetteProvider.overrideParameters.apertureSize = size;
        godmodeVignetteProvider.overrideParameters.featheringEffect = 0.1f; // Kleinere Übergangszone

        tunnelingVignetteController.BeginTunnelingVignette(godmodeVignetteProvider);
    }

    void StartGrabbing()
    {
        isGrabbingObject = true;
    }

    void StopGrabbing()
    {
        isGrabbingObject = false;
    }

    public void StartPlacingObject()
    {
        isPlacingObject = true;
    }

    public void StopPlacingObject()
    {
        isPlacingObject = false;
    }

    public bool IsGodmodeActive()
{
    return isGodmodeActive;
}

public void ToggleGodmodeFromRadialMenu()
{
    StartCoroutine(ToggleGodmode());
}

}

