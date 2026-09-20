using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement: MonoBehaviour
{

    PlayerInput playerInput;
    InputAction moveAction;
    [SerializeField] float speed = 5;
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
    }

    void Update()
    {
        // Freno global mientras haya diálogo abierto (fogata o tapera). No depende del inspector.
        if (FogataMonologue.DialogueOpen || TaperaDialogue.DialogueOpen)
            return;
        MovePlayer();
    }

    public void AddSpeed(float amount)
    {
        speed += amount;
    }


    CapsuleCollider bodyCollider;

    void Awake()
    {
        bodyCollider = GetComponent<CapsuleCollider>();
    }

    void MovePlayer()
    {
        if (moveAction == null) return;
        Vector2 direction = moveAction.ReadValue<Vector2>();
        Vector3 delta = new Vector3(direction.x, 0, direction.y) * speed * Time.deltaTime;
        if (delta == Vector3.zero) return;

        if (bodyCollider == null)
        {
            transform.position += delta;
            return;
        }

        // Intento completo, si choca pruebo por ejes para deslizar por el tronco.
        if (CanOccupy(transform.position + delta))
        {
            transform.position += delta;
        }
        else if (CanOccupy(transform.position + new Vector3(delta.x, 0, 0)))
        {
            transform.position += new Vector3(delta.x, 0, 0);
        }
        else if (CanOccupy(transform.position + new Vector3(0, 0, delta.z)))
        {
            transform.position += new Vector3(0, 0, delta.z);
        }
    }

    bool CanOccupy(Vector3 targetPos)
    {
        float radius = bodyCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
        float height = Mathf.Max(bodyCollider.height, radius * 2f);
        Vector3 center = targetPos + bodyCollider.center;
        float half = Mathf.Max(0f, height * 0.5f - radius);
        Vector3 p1 = center + Vector3.up * half;
        Vector3 p2 = center - Vector3.up * half;

        var hits = Physics.OverlapCapsule(p1, p2, radius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            if (h == null || h == bodyCollider) continue;
            if (h.transform.IsChildOf(transform)) continue;
            // Ignora el piso.
            if (h.gameObject.name.ToLower().Contains("piso")) continue;
            return false;
        }
        return true;
    }
}
