#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Linq;
/*
 Static editor utility class to auto-generate mutation libraries by scanning prefab components for mutations
 Uses type reflection from Linq queries to identify any component that implements IMutation
 This took a lot of documentation diving but it will allow the GameManager to automatically have knowledge of ANY mutations added later in development
 The only requirement is that for every new mutation, a dummy prefab is made with the mutation component attached to it
 Hit the refresh button after to apply any additions (or if components get moved around and update references)

 This file may have excessive commenting at first so I can understand and replicate parts of it later
 Any future complaints regarding the comment verbosity (or the personality of the author) can be directed to Jesse "Nuclear Mango" Rivera at jsr97@drexel.edu
*/
public static class MutationLibraryGenerator
{
    const string LIBRARY_PATH = "Assets/Resources/MutationLibrary.asset";

    [MenuItem("Tools/Mutation Library/Refresh")]
    public static void Refresh()
    {
        //Literally the first time I've used var because I thought that was for JavaScript developers *shudders*
        //It's more in line with C++'s auto keyword for type inference at compile time
        var library = AssetDatabase.LoadAssetAtPath<MutationLibrary>(LIBRARY_PATH);
        if (library == null)
        {
            library = ScriptableObject.CreateInstance<MutationLibrary>();
            System.IO.Directory.CreateDirectory("Assets/Resources");
            AssetDatabase.CreateAsset(library, LIBRARY_PATH);
        }

        var globalUIDs = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Mutations" });
        /*
         Chain of Linq queries that does the following:
            1. Convert each unique asset reference from the guid list into a corresponding asset path (Transform the data)
            2. Loads a prefab into memory (but does not instantiate in scene) from each asset path (Make the data available)
            3. Filters the loaded prefabs into a list of prefabs that have at least 1 IMutation-implementing component attached (Filter the available data)

         I was hesitant to settle for this solution because lengthy Linq chains may come with an annoying amount of overhead, especially because the filtering (Where) is done at the end instead of the start
         This will reduce the "personal overhead" of having to manually maintain the GameManager's known mutations, however, so it's probably worth it
        */
        var prefabs = globalUIDs
            .Select(g => AssetDatabase.GUIDToAssetPath(g))
            .Select(p => AssetDatabase.LoadAssetAtPath<GameObject>(p))
            .Where(prefab => prefab.GetComponents<MonoBehaviour>().OfType<IMutation>().Any()).ToList();

        library.mutationPrefabs = prefabs;
        //Tells Unity that the library has been modified (or initialized) so it gets saved when the next line executes
        EditorUtility.SetDirty(library);
        AssetDatabase.SaveAssets();
    }
}
#endif
