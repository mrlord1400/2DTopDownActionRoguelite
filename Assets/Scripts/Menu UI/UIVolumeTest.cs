using UnityEngine;
using UnityEngine.UI;

public class UIVolumeTest : MonoBehaviour
{
    [Header("Kéo Canvas (chứa AudioSource) vào đây:")]
    public AudioSource bgmSource;

    // Hàm này sẽ được gọi khi em kéo thanh Music
    public void OnMusicVolumeChanged(float value)
    {
        // In ra màn hình Console để UI Designer test
        Debug.Log("Music Volume đang ở mức: " + value);

        // Thay đổi trực tiếp âm lượng của nhạc nền
        if (bgmSource != null)
        {
            bgmSource.volume = value;
        }
    }

    // Hàm này sẽ được gọi khi em kéo thanh SFX (Tiếng động)
    public void OnSFXVolumeChanged(float value)
    {
        Debug.Log("SFX Volume đang ở mức: " + value);
        
        // Coder: Thêm logic AudioMixer cho SFX vào đây!
    }
}