using UnityEngine;
using UnityEngine.InputSystem;

// Una leña recogible con E. Sin collider: no bloquea, solo se junta por cercanía.
// Si hay un diálogo abierto, ignora la E (esa pulsación es del diálogo).
public class LenaPickup : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] LenaManager manager;
    [SerializeField] Transform player;

    [Header("Ajustes")]
    [SerializeField] float pickupDistance = 5f;

    bool collected;

    void Awake()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (collected || manager == null || player == null)
            return;

        if (FogataMonologue.DialogueOpen)
            return;

        // Sin el primer diálogo de la fogata no se puede recolectar.
        if (!FogataMonologue.FirstDialogueDone)
            return;

        if (Vector3.Distance(player.position, transform.position) > pickupDistance)
            return;

        if (ClickHelper.WasClickedOn(transform))
            Collect();
    }

    void Collect()
    {
        collected = true;
        gameObject.SetActive(false);
        manager.AddOne();
    }
}
