using System.Collections;
using UnityEngine;

public class FlashEffect : MonoBehaviour
{
    private SpriteRenderer sr;
    private Color originalColor;
    private Coroutine flashRoutine;

    void Start()
    {
        // Ưu tiên tìm SpriteRenderer ở con (nếu có) hoặc ở chính bản thân
        sr = GetComponentInChildren<SpriteRenderer>(); 
        if (sr != null)
        {
            originalColor = sr.color;
        }
    }

    public void FlashRed()
    {
        if (sr == null) return;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    public void FadeOutDeath()
    {
        if (sr == null) return;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        sr.color = originalColor;
    }

    private IEnumerator FadeOutRoutine()
    {
        float fadeTime = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            // Giảm dần độ mờ (Alpha) từ 1 về 0
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeTime);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }
    }
}
