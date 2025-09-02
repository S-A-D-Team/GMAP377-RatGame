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
    public float moveUpDistance = 80f;
    public float moveUpTime = 0.5f;

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

    private void Start()
    {
        //StartTestPopups();
    }

    public void ShowPopup(string message)
    {
        if (popupPrefab == null || popupContainer == null)
        {
            Debug.LogError("Popup prefab or container is missing!");
            return;
        }

        // Move existing popups up
        foreach (Transform child in popupContainer)
        {
            StartCoroutine(MoveUp(child, moveUpDistance, moveUpTime));
        }

        GameObject popup = Instantiate(popupPrefab, popupContainer);
        popup.transform.SetAsLastSibling(); // New popup starts at bottom

        TextMeshProUGUI popupText = popup.GetComponentInChildren<TextMeshProUGUI>(true);
        if (popupText != null)
        {
            popupText.text = message;
        }

        StartCoroutine(FadeAndDestroy(popup));
    }

    private IEnumerator MoveUp(Transform popup, float distance, float duration)
    {
        Vector3 start = popup.localPosition;
        Vector3 end = start + Vector3.up * distance;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            popup.localPosition = Vector3.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        popup.localPosition = end;
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
        if (cg == null) yield break;

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

    public void StartTestPopups()
    {
        StartCoroutine(SpawnTestPopups());
    }

    private IEnumerator SpawnTestPopups()
    {
        int counter = 1;
        while (true)
        {
            ShowPopup("Test Popup " + counter);
            counter++;
            yield return new WaitForSeconds(2f);
        }
    }
}
