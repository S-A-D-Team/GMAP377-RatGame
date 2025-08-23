using UnityEngine;
using TMPro; 
using System.Collections;

public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    [Header("Popup Settings")]
    public GameObject popupPrefab;
    public Transform popupContainer;
    public float popupDuration = 5f;

    [Header("Default Narration Texts")]
    [TextArea] public string humanDefaultText;
    [TextArea] public string catDefaultText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowPopup(string message)
    {
        if (popupPrefab == null || popupContainer == null)
        {
            Debug.LogError("Popup prefab or container is missing!");
            return;
        }

        GameObject popup = Instantiate(popupPrefab, popupContainer);
        popup.transform.SetAsFirstSibling(); // New one goes on bottom

        TextMeshProUGUI popupText = popup.GetComponentInChildren<TextMeshProUGUI>(true);
        if (popupText != null)
        {
            popupText.text = message;
        }
        else
        {
            Debug.LogWarning("Popup prefab missing TextMeshProUGUI component.");
        }

        StartCoroutine(FadeAndDestroy(popup));
    }

    public void ShowCatPopup(string customText = null)
    {
        ShowPopup(customText ?? catDefaultText);
        Debug.Log("Cat popup shown");
    }

    public void ShowHumanPopup(string customText = null)
    {
        ShowPopup(customText ?? humanDefaultText);
        Debug.Log("Human popup shown");
    }

    private IEnumerator FadeAndDestroy(GameObject popup)
    {
        CanvasGroup cg = popup.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            Debug.LogError("Missing CanvasGroup on popup prefab!");
            yield break;
        }

        yield return new WaitForSeconds(popupDuration);

        float fadeTime = 0.5f;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            cg.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cg.alpha = 0f;
        Destroy(popup);
    }
}
