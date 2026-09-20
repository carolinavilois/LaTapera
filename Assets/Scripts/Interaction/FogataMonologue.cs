using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// Monólogo de una línea junto a la fogata, con el mismo formato de Escena 0.
// Primera vez muestra firstLine; visitas siguientes muestran secondLine.
// E/click: completa typewriter, luego cierra el panel.
public class FogataMonologue : MonoBehaviour
{
    [Header("UI diálogo (panel oculto por defecto)")]
    [SerializeField] GameObject dialogPanel;
    [SerializeField] TMP_Text speakerText;
    [SerializeField] TMP_Text bodyText;

    [Header("Jugador (vacío = busca tag Player)")]
    [SerializeField] Transform player;
    [SerializeField] float interactDistance = 5f;

    [Header("Apertura automática (primer diálogo, sin E)")]
    [SerializeField] bool autoOpenFirstLine = true;
    [SerializeField] float autoOpenDistance = 7f;

    [Header("Bloqueo movimiento (vacío = lo busca en el jugador)")]
    [SerializeField] PlayerMovement playerMovement;

    [Header("Textos")]
    [SerializeField] DialogueLine firstLine = new DialogueLine
    {
        speaker = "PRUDENCIO",
        text = "Los paisanos estarán por venir. Mejor llegar con un poco más de leña para el fogón. Voy a buscar por el monte."
    };
    [SerializeField] DialogueLine secondLine = new DialogueLine
    {
        speaker = "PRUDENCIO",
        text = "Aún me falta leña. Voy a seguir buscando."
    };

    [Header("Ajustes")]
    [SerializeField] float charsPerSecond = 40f;

    // Flag global: frena al Player aunque falle la referencia del inspector.
    public static bool DialogueOpen { get; private set; }

    // Primer diálogo automático ya visto (habilita HUD y recolección de leña).
    public static bool FirstDialogueDone { get; private set; }

    bool showingFirstLine;

    // Aviso al cerrar el panel (ej: para ocultar el HUD al terminar el monólogo final).
    public event System.Action onPanelClosed;

    bool shownFirst;
    bool panelOpen;
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

        ClearTexts();
        SetPanel(false);
    }

    void Update()
    {
        if (panelOpen)
        {
            if (WasAdvancePressed())
            {
                if (isTyping) CompleteLine();
                else ClosePanel();
            }
            return;
        }

        if (player == null) return;

        float distToFogata = Vector3.Distance(player.position, transform.position);

        // Primer diálogo: se abre solo al acercarse (más lejos), sin pulsar E.
        if (autoOpenFirstLine && !shownFirst && distToFogata <= autoOpenDistance)
        {
            OpenPanel();
            return;
        }

        if (distToFogata > interactDistance)
            return;

        if (ClickHelper.WasClickedOn(transform))
            OpenPanel();
    }

    void OpenPanel()
    {
        showingFirstLine = !shownFirst;
        var line = shownFirst ? secondLine : firstLine;
        shownFirst = true;

        if (speakerText != null) speakerText.text = line != null ? line.speaker : "";
        fullText = line != null && line.text != null ? line.text : "";

        SetPanel(true);
        panelOpen = true;
        DialogueOpen = true;
        SetMovement(false);

        if (typeRoutine != null) StopCoroutine(typeRoutine);
        typeRoutine = StartCoroutine(TypeLine(fullText));
    }

    void ClosePanel()
    {
        panelOpen = false;
        DialogueOpen = false;
        SetPanel(false);
        SetMovement(true);
        if (showingFirstLine)
        {
            showingFirstLine = false;
            FirstDialogueDone = true;
        }
        onPanelClosed?.Invoke();
        onPanelClosed = null;
    }

    // Muestra una línea externa (ej: monólogo al completar la leña).
    // Reusa panel, typewriter y congelamiento. No toca la secuencia texto1→texto2.
    public void ShowExternalLine(string speaker, string text)
    {
        if (panelOpen)
            return;

        if (speakerText != null) speakerText.text = speaker != null ? speaker : "";
        fullText = text != null ? text : "";
        showingFirstLine = false;

        SetPanel(true);
        panelOpen = true;
        DialogueOpen = true;
        SetMovement(false);

        if (typeRoutine != null) StopCoroutine(typeRoutine);
        typeRoutine = StartCoroutine(TypeLine(fullText));
    }

    void OnDisable()
    {
        DialogueOpen = false;
        SetMovement(true);
    }

    void SetMovement(bool enabled)
    {
        if (playerMovement != null)
            playerMovement.enabled = enabled;
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

    bool WasAdvancePressed()
    {
        // Avance solo con click/tap (sin teclas).
        return ClickHelper.WasClickPressed();
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
}
