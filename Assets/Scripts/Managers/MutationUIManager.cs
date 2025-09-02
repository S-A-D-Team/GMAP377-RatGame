using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct MutationIcon
{
    public string mutationName;
    public Sprite icon;
}

public class MutationUIManager : MonoBehaviour
{
    public static MutationUIManager Instance;

    [Header("UI Setup")]
    public Transform mutationContainer;
    public GameObject mutationPrefab; // UI image prefab

    [Header("Mutation Icons")]
    public List<MutationIcon> mutationIcons = new List<MutationIcon>();

    private Dictionary<string, GameObject> activeMutations = new Dictionary<string, GameObject>();
    private Dictionary<string, Sprite> iconLookup = new Dictionary<string, Sprite>();

    private void Awake()
    {
        Instance = this;

        // Build quick lookup dictionary from the serialized list
        foreach (var entry in mutationIcons)
        {
            if (!iconLookup.ContainsKey(entry.mutationName))
                iconLookup.Add(entry.mutationName, entry.icon);
        }
    }

    public void AddMutationUI(string mutationName)
    {
        if (activeMutations.ContainsKey(mutationName))
            return; // Already shown

        if (!iconLookup.TryGetValue(mutationName, out Sprite icon))
        {
            Debug.LogWarning($"No icon found for mutation: {mutationName}");
            return;
        }

        GameObject uiElement = Instantiate(mutationPrefab, mutationContainer);
        uiElement.name = mutationName;

        // Apply icon
        Image img = uiElement.GetComponent<Image>();
        if (img != null)
            img.sprite = icon;

        activeMutations.Add(mutationName, uiElement);
    }

    public void RemoveMutationUI(string mutationName)
    {
        if (activeMutations.TryGetValue(mutationName, out GameObject uiElement))
        {
            Destroy(uiElement);
            activeMutations.Remove(mutationName);
        }
    }
}
