using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

// Interacción mínima de la tranquera: click en el sprite cerca → sonido → 1s → rancho.
// Sin UI, sin Canvas, sin billboard.
public class TranqueraSwitch : MonoBehaviour
{
    [Header("Jugador (vacío = busca tag Player)")]
    [SerializeField] Transform player;
    [SerializeField] float interactDistance = 3.5f;

    [Header("Audio (vacío = solo espera y cambia)")]
    [SerializeField] AudioClip openClip;
    [SerializeField] AudioSource audioSource;

    [Header("Destino")]
    [SerializeField] string nextSceneName = "MainGameRancho";
    [SerializeField] float waitAfterSound = 1f;

    bool used;

    void Awake()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (used || player == null)
            return;

        if (Vector3.Distance(player.position, transform.position) > interactDistance)
            return;

        if (ClickHelper.WasClickedOn(transform))
            StartCoroutine(OpenRoutine());
    }

    IEnumerator OpenRoutine()
    {
        used = true;

        if (audioSource != null && openClip != null)
            audioSource.PlayOneShot(openClip);

        if (waitAfterSound > 0f)
            yield return new WaitForSeconds(waitAfterSound);

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }
}
