using System.Collections;
using UnityEngine;

public class EnterNewMap : MonoBehaviour
{
    [SerializeField] private string nextMapName = "Map2";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartCoroutine(PlayerFall(collision.gameObject));
        }
    }

    private IEnumerator PlayerFall(GameObject player)
    {
        var playerMovement = player.GetComponent<MonoBehaviour>(); // script thật của bạn
        if (playerMovement != null) playerMovement.enabled = false;

        var rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.isKinematic = true;
        }

        Vector3 targetPosition = transform.position;
        float duration = 1f;
        float elapsed = 0f;

        Vector3 startPosition = player.transform.position;
        Vector3 startScale = player.transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            player.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            player.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            yield return null;
        }

        // gọi loading manager
        LoadingProgress.Instance.LoadScene(nextMapName);
    }
}