using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingProgress : MonoBehaviour
{
    public static LoadingProgress Instance;

    [Header("UI")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider progressBar;
    [SerializeField] private LoadingAnimation loadingAnimation;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        loadingScreen.SetActive(true);
        yield return new WaitForSeconds(0.3f);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float fakeTimer = 0f;
        float minFillTime = 1.5f; // thời gian tối thiểu để bar chạy lên 90%

        // Giai đoạn 1: chờ cả 2 — scene thật VÀ fake timer
        while (operation.progress < 0.9f || fakeTimer < minFillTime)
        {
            fakeTimer += Time.deltaTime;

            float realProgress  = operation.progress / 0.9f;     // 0→1 theo Unity
            float fakeProgress  = fakeTimer / minFillTime;        // 0→1 theo thời gian
            float displayed     = Mathf.Min(realProgress, fakeProgress); // lấy cái chậm hơn

            loadingAnimation.UpdateProgress(displayed * 0.9f);    // hiển thị 0→90%
            yield return null;
        }

        // Giai đoạn 2: đợi animation mượt tới 90%
        loadingAnimation.UpdateProgress(0.9f);
        while (loadingAnimation.GetCurrentProgress() < 0.89f)
            yield return null;

        // Giai đoạn 3: fill lên 100%
        loadingAnimation.UpdateProgress(1f);
        while (loadingAnimation.GetCurrentProgress() < 0.99f)
            yield return null;

        yield return new WaitForSeconds(0.1f);
        operation.allowSceneActivation = true;
    }
}
    