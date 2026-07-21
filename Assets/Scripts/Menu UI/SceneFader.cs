using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    public static SceneFader instance; // Áp dụng Singleton pattern để gọi từ mọi nơi

    [Header("UI References")]
    public CanvasGroup fadeCanvasGroup; 
    
    [Header("Settings")]
    public float fadeDuration = 0.5f; // Thời gian tối dần/sáng dần

    private void Awake()
    {
        // Đảm bảo chỉ có 1 SceneFader hoạt động
        if (instance == null) 
        {
            instance = this;
        }
    }

    private void Start()
    {
        // Tự động Fade In (sáng dần lên) khi Scene vừa load xong
        StartCoroutine(FadeIn());
    }

    // Hàm public này để cánh cửa gọi khi cần chuyển scene
    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    private IEnumerator FadeIn()
    {
        fadeCanvasGroup.alpha = 1f; // Bắt đầu với màn hình đen thui
        fadeCanvasGroup.blocksRaycasts = true;

        // Giảm dần alpha về 0 (sáng lên)
        while (fadeCanvasGroup.alpha > 0f)
        {
            fadeCanvasGroup.alpha -= Time.deltaTime / fadeDuration;
            yield return null; // Chờ frame tiếp theo
        }

        fadeCanvasGroup.blocksRaycasts = false; // Cho phép Player thao tác
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        fadeCanvasGroup.blocksRaycasts = true; // Chặn thao tác trong lúc mờ đi

        // Tăng dần alpha lên 1 (tối đi)
        while (fadeCanvasGroup.alpha < 1f)
        {
            fadeCanvasGroup.alpha += Time.deltaTime / fadeDuration;
            yield return null;
        }

        // Tối hẳn rồi thì load Scene mới
        SceneManager.LoadScene(sceneName);
    }
}