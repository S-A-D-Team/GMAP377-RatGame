using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Mutation/Mutation Library")]
public class MutationLibrary : ScriptableObject
{
    [Tooltip(@"
        All Mutation prefabs must have an IMutation-implementing component on them.
        The prefab and component must also have the same name.
        These prefabs must be located in Assets/Prefabs/Mutations
        ")]
    public List<GameObject> mutationPrefabs;
}
