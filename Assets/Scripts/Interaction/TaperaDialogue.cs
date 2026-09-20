using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// Susto de la Tapera: cuando está en cámara y el jugador cerca,
// abre solo un diálogo (lista editable en inspector). Al cerrar la
// última línea, el jugador corre un poco más rápido.
// v2: toque de reimportación (sin cambios funcionales).
public class TaperaDialogue : MonoBehaviour
{
    public static bool DialogueOpen { get; private set; }

    [Header("UI (mismo panel de diálogo del rancho)")]
    [SerializeField] GameObject dialogPanel;
    [SerializeField] TMP_Text speakerText;
    [SerializeField] TMP_Text bodyText;

    [Header("Disparo automático (una sola vez)")]
    [SerializeField] Transform player;
    [SerializeField] float triggerDistance = 5f;
    [SerializeField] bool requireOnCamera = true;

    [Header("Líneas (editables en inspector)")]
    [SerializeField] List<DialogueLine> lines = new List<DialogueLine>
    {
        new DialogueLine { speaker = "PRUDENCIO", text = "sorpresa1" },
        new DialogueLine { speaker = "PRUDENCIO", text = "sorpresa2" },
        new DialogueLine { speaker = "PRUDENCIO", text = "sorpresa3" },
    };

    [Header("Recompensa: correr tras el susto")]
    [SerializeField] PlayerMovement playerMovement;
    [SerializeField] float speedBoost = 2f;

    [Header("Ajustes")]
    [SerializeField] float charsPerSecond = 40f;

    Renderer cachedRenderer;
    bool triggered;
    bool boosted;
    bool panelOpen;
    int index;
    bool isTyping;
    string fullText = "";
    Coroutine typeRoutine;

    void Awake()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (playerMovement == null && player != null)
            playerMovement = player.GetComponent<PlayerMovement>();

        cachedRenderer = GetComponent<Renderer>();

        // Blindaje: si la lista llega vacía (ej: inspector sin cargar), usa las 3 por defecto.
        if (lines == null)
            lines = new List<DialogueLine>();
        if (lines.Count == 0)
        {
            lines.Add(new DialogueLine { speaker = "PRUDENCIO", text = "sorpresa1" });
            lines.Add(new DialogueLine { speaker = "PRUDENCIO", text = "sorpresa2" });
            lines.Add(new DialogueLine { speaker = "PRUDENCIO", text = "sorpresa3" });
        }

        ClearTexts();
        SetPanel(false);
    }

    void Update()
    {
        if (panelOpen)
        {
            if (ClickHelper.WasClickPressed())
            {
                if (isTyping) CompleteLine();
                else AdvanceLine();
            }
            return;
        }

        if (triggered || player == null)
            return;

        // Sin pisar otro diálogo (ej: monólogo final de la leña).
        if (FogataMonologue.DialogueOpen)
            return;

        // Distancia al punto más cercano del sprite (no al centro):
        // en un square gigante, tocar el borde ya cuenta como "llegar".
        float distToTapera = cachedRenderer != null
            ? Vector3.Distance(player.position, cachedRenderer.bounds.ClosestPoint(player.position))
            : Vector3.Distance(player.position, transform.position);
        if (distToTapera > triggerDistance)
            return;

        if (requireOnCamera && !IsOnCamera())
            return;

        if (lines.Count == 0)
            return;

        triggered = true;
        OpenLine(0);
    }

    bool IsOnCamera()
    {
        if (cachedRenderer == null)
            return true;
        var cam = Camera.main;
        if (cam == null)
            return false;
        var planes = GeometryUtility.CalculateFrustumPlanes(cam);
        return GeometryUtility.TestPlanesAABB(planes, cachedRenderer.bounds);
    }

    void OpenLine(int i)
    {
        index = i;
        var line = lines[i];
        if (speakerText != null) speakerText.text = line != null ? line.speaker : "";
        fullText = line != null && line.text != null ? line.text : "";

        SetPanel(true);
        panelOpen = true;
        DialogueOpen = true;
        SetMovement(false);

        if (typeRoutine != null) StopCoroutine(typeRoutine);
        typeRoutine = StartCoroutine(TypeLine(fullText));
    }

    void AdvanceLine()
    {
        if (index + 1 < lines.Count)
            OpenLine(index + 1);
        else
            ClosePanel();
    }

    void ClosePanel()
    {
        panelOpen = false;
        DialogueOpen = false;
        SetPanel(false);
        SetMovement(true);

        if (!boosted)
        {
            boosted = true;
            if (playerMovement != null)
                playerMovement.AddSpeed(speedBoost);
        }
    }

    void OnDisable()
    {
        DialogueOpen = false;
        SetMovement(true);
    }

    IEnumerator TypeLine(string text)
    {
        isTyping = true;
        if (bodyText != null) bodyText.text = "";

        float interval = charsPerSecond > 0 ? 1f / charsPerSecond : 0.01f;
        int count = 0;
        while (count < text.Length)
        {
            count++;
            if (bodyText != null) bodyText.text = text.Substring(0, count);
            yield return new WaitForSeconds(interval);
        }

        isTyping = false;
        typeRoutine = null;
    }

    void CompleteLine()
    {
        if (typeRoutine != null) StopCoroutine(typeRoutine);
        typeRoutine = null;
        isTyping = false;
        if (bodyText != null) bodyText.text = fullText;
    }

    void ClearTexts()
    {
        if (speakerText != null) speakerText.text = "";
        if (bodyText != null) bodyText.text = "";
        fullText = "";
    }

    void SetPanel(bool visible)
    {
        if (dialogPanel != null)
            dialogPanel.SetActive(visible);
    }

    void SetMovement(bool enabled)
    {
        if (playerMovement != null)
            playerMovement.enabled = enabled;
    }
}
