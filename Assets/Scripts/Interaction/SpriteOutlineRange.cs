using UnityEngine;

// Contorno por shader en el sprite cuando el jugador está en rango.
// Usa MaterialPropertyBlock: no rompe el batching entre objetos que comparten material.
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteOutlineRange : MonoBehaviour
{
    [Header("Jugador (vacío = busca tag Player)")]
    [SerializeField] Transform player;
    [SerializeField] float range = 5f;

    [Header("Condiciones (como la recolección)")]
    [SerializeField] bool requireFirstDialogue = true;
    [SerializeField] bool hideWhenDialogueOpen = true;

    [Header("Contorno")]
    [SerializeField] Color outlineColor = Color.white;
    [SerializeField] float outlineSize = 0.015f;

    static readonly int OutlineOnId = Shader.PropertyToID("_OutlineOn");
    static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
    static readonly int OutlineSizeId = Shader.PropertyToID("_OutlineSize");

    SpriteRenderer sr;
    MaterialPropertyBlock mpb;
    bool shown;

    void Awake()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        sr = GetComponent<SpriteRenderer>();
        mpb = new MaterialPropertyBlock();
        ApplyProps();
        SetOutline(false);
    }

    void Update()
    {
        bool v = player != null && Vector3.Distance(player.position, transform.position) <= range;
        if (v && requireFirstDialogue && !FogataMonologue.FirstDialogueDone)
            v = false;
        if (v && hideWhenDialogueOpen && FogataMonologue.DialogueOpen)
            v = false;

        if (v != shown)
        {
            shown = v;
            SetOutline(v);
        }
    }

    void ApplyProps()
    {
        if (sr == null) return;
        sr.GetPropertyBlock(mpb);
        mpb.SetColor(OutlineColorId, outlineColor);
        mpb.SetFloat(OutlineSizeId, outlineSize);
        sr.SetPropertyBlock(mpb);
    }

    void SetOutline(bool on)
    {
        if (sr == null) return;
        sr.GetPropertyBlock(mpb);
        mpb.SetFloat(OutlineOnId, on ? 1f : 0f);
        sr.SetPropertyBlock(mpb);
    }
}
