using TMPro;
using UnityEngine;

// Cuenta las leñas y actualiza el HUD. Al completar, abre el monólogo final
// ("Ya tengo suficiente...") y pide volver a la fogata.
public class LenaManager : MonoBehaviour
{
    [Header("Meta")]
    [SerializeField] int totalToCollect = 5;

    [Header("Referencias")]
    [SerializeField] TMP_Text hudText;
    [SerializeField] FogataMonologue fogataMonologue;

    [Header("Textos")]
    [SerializeField] string finalSpeaker = "PRUDENCIO";
    [SerializeField] string finalText = "Ya tengo suficiente. Debería volver a la fogata.";
    [SerializeField] string hudCompleteText = "Volvé a la fogata";

    int count;
    bool completed;
    bool hudShown;

    void Awake()
    {
        UpdateHud();
    }

    void Update()
    {
        // El HUD aparece recién después del primer diálogo automático de la fogata.
        if (!hudShown && hudText != null && FogataMonologue.FirstDialogueDone)
        {
            hudShown = true;
            hudText.gameObject.SetActive(true);
        }
    }

    public void AddOne()
    {
        if (completed)
            return;

        count++;

        if (count >= totalToCollect)
        {
            completed = true;
            UpdateHud();
            if (fogataMonologue != null)
            {
                fogataMonologue.onPanelClosed += HideHud;
                fogataMonologue.ShowExternalLine(finalSpeaker, finalText);
            }
            else
            {
                HideHud();
            }
        }
        else
        {
            UpdateHud();
        }
    }

    void UpdateHud()
    {
        if (hudText == null)
            return;
        hudText.text = completed ? hudCompleteText : "Leña " + count + "/" + totalToCollect;
    }

    void HideHud()
    {
        if (fogataMonologue != null)
            fogataMonologue.onPanelClosed -= HideHud;
        if (hudText != null)
            hudText.gameObject.SetActive(false);
    }
}
