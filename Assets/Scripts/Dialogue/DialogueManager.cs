using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    [Header("UI (arrastrar desde Escena0)")]
    [SerializeField] TMP_Text speakerText;
    [SerializeField] TMP_Text bodyText;
    [SerializeField] Image fadeImage;
    [SerializeField] GameObject dialogPanel;

    [Header("Contenido Escena 0")]
    [SerializeField] List<DialogueLine> lines = new List<DialogueLine>();

    [Header("Ajustes")]
    [SerializeField] float charsPerSecond = 40f;
    [SerializeField] float startDelay = 2f;
    [SerializeField] float fadeDuration = 1f;
    [SerializeField] string nextSceneName = "Maingame";
    [SerializeField] bool startOnAwake = true;

    public UnityEvent onDialogueFinished;

    int currentIndex = -1;
    Coroutine typeRoutine;
    bool isTyping;
    bool isFinished;
    bool waitingDelay;
    string fullText = "";

    void Awake()
    {
        // Si la lista viene vacía del inspector, cargamos los 6 diálogos por defecto.
        if (lines == null || lines.Count == 0)
        {
            lines = new List<DialogueLine>
            {
                new DialogueLine { speaker = "PRUDENCIO", text = "Ya pensaba yo que iba a tener que dormir a campo abierto nomás. Aunque con el chucho que me ha entrado… dormir no iba a poder." },
                new DialogueLine { speaker = "DON CELESTINO", text = "Suerte ha tenido usted de encontrarse con el ranchito de Doña Dominga. Una vez caída la noche… no está para andar solito. Aunque peor es andar mal acompañado por un haragán." },
                new DialogueLine { speaker = "TIBURCIO", text = "Ya le he pedido perdón, don Celestino. La tropilla se me disparó y no hubo modo de volverla a juntar." },
                new DialogueLine { speaker = "DON CELESTINO", text = "Sí, sí… la tropilla asustada. Encima de haragán, embustero." },
                new DialogueLine { speaker = "PRUDENCIO", text = "Yo le creo lo que dice el muchacho. Venía yo a caballo hasta que también se asustó y salió disparado. Tuve que andar a pie hasta que vi lo que vi…" },
                new DialogueLine { speaker = "DON CELESTINO", text = "¡Déjese de andar con vueltas y cuente de una vez lo que le ha pasado!" },
            };
        }

        if (fadeImage != null)
            SetFadeAlpha(0f);

        // Si no se arrastró el panel, lo deducimos del padre del texto.
        if (dialogPanel == null)
        {
            if (bodyText != null && bodyText.transform.parent != null)
                dialogPanel = bodyText.transform.parent.gameObject;
            else if (speakerText != null && speakerText.transform.parent != null)
                dialogPanel = speakerText.transform.parent.gameObject;
        }

        ClearTexts();
        SetPanelVisible(false);
    }

    void Start()
    {
        if (startOnAwake)
            StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        waitingDelay = true;
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);
        waitingDelay = false;
        SetPanelVisible(true);
        ShowNextLine();
    }

    void ClearTexts()
    {
        if (speakerText != null) speakerText.text = "";
        if (bodyText != null) bodyText.text = "";
        fullText = "";
    }

    void SetPanelVisible(bool visible)
    {
        if (dialogPanel != null)
            dialogPanel.SetActive(visible);
    }

    void Update()
    {
        if (isFinished || waitingDelay)
            return;

        if (WasAdvancePressed())
            Advance();
    }

    bool WasAdvancePressed()
    {
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.spaceKey.wasPressedThisFrame ||
                kb.enterKey.wasPressedThisFrame ||
                kb.eKey.wasPressedThisFrame ||
                kb.numpadEnterKey.wasPressedThisFrame)
                return true;
        }

        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            return true;

        var touch = Touchscreen.current;
        if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
            return true;

        return false;
    }

    public void Advance()
    {
        if (isTyping)
        {
            CompleteLine();
            return;
        }

        ShowNextLine();
    }

    void ShowNextLine()
    {
        currentIndex++;

        if (lines == null || currentIndex >= lines.Count)
        {
            FinishDialogue();
            return;
        }

        var line = lines[currentIndex];
        if (speakerText != null)
            speakerText.text = line.speaker;

        fullText = line.text ?? "";
        if (typeRoutine != null)
            StopCoroutine(typeRoutine);
        typeRoutine = StartCoroutine(TypeLine(fullText));
    }

    IEnumerator TypeLine(string text)
    {
        isTyping = true;
        if (bodyText != null)
            bodyText.text = "";

        float interval = charsPerSecond > 0 ? 1f / charsPerSecond : 0.01f;
        int count = 0;

        while (count < text.Length)
        {
            count++;
            if (bodyText != null)
                bodyText.text = text.Substring(0, count);
            yield return new WaitForSeconds(interval);
        }

        isTyping = false;
        typeRoutine = null;
    }

    void CompleteLine()
    {
        if (typeRoutine != null)
            StopCoroutine(typeRoutine);
        typeRoutine = null;
        isTyping = false;
        if (bodyText != null)
            bodyText.text = fullText;
    }

    void FinishDialogue()
    {
        if (isFinished)
            return;
        isFinished = true;
        onDialogueFinished?.Invoke();
        StartCoroutine(FadeAndLoad());
    }

    IEnumerator FadeAndLoad()
    {
        if (fadeImage != null)
        {
            float t = 0f;
            // Asegura que tape todo aunque el Rect esté raro.
            fadeImage.raycastTarget = false;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                SetFadeAlpha(Mathf.Clamp01(t / fadeDuration));
                yield return null;
            }
            SetFadeAlpha(1f);
        }

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

    void SetFadeAlpha(float a)
    {
        if (fadeImage == null) return;
        Color c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
    }
}
