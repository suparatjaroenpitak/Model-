#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ToyotaFortuner.Editor
{
    public static class ToyotaFortunerSetupMenu
    {
        private const string Root = "Assets/Vehicle/ToyotaFortuner";
        private const string ProfilePath = Root + "/Materials/ToyotaFortunerAssetProfile.asset";

        [MenuItem("Tools/Toyota Fortuner/Add Runtime Setup To Selected")]
        public static void AddRuntimeSetupToSelected()
        {
            if (Selection.activeGameObject == null)
            {
                EditorUtility.DisplayDialog("Toyota Fortuner", "Select the imported FBX/GLB root first.", "OK");
                return;
            }

            GameObject selected = Selection.activeGameObject;
            ToyotaFortunerRuntimeSetup setup = selected.GetComponent<ToyotaFortunerRuntimeSetup>();
            if (setup == null)
            {
                setup = Undo.AddComponent<ToyotaFortunerRuntimeSetup>(selected);
            }

            ToyotaFortunerAssetProfile profile = AssetDatabase.LoadAssetAtPath<ToyotaFortunerAssetProfile>(ProfilePath);
            if (profile == null)
            {
                EnsureFolder(Root + "/Materials");
                profile = ScriptableObject.CreateInstance<ToyotaFortunerAssetProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
                AssetDatabase.SaveAssets();
            }

            SerializedObject serializedSetup = new SerializedObject(setup);
            serializedSetup.FindProperty("profile").objectReferenceValue = profile;
            serializedSetup.FindProperty("modelRoot").objectReferenceValue = selected.transform;
            serializedSetup.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(selected);
            Selection.activeObject = selected;
            Debug.Log("Toyota Fortuner runtime setup added. Assign generated materials to the profile before pressing Apply Setup.");
        }

        [MenuItem("Tools/Toyota Fortuner/Create Folder Structure")]
        public static void CreateFolderStructure()
        {
            EnsureFolder(Root + "/Runtime");
            EnsureFolder(Root + "/Editor");
            EnsureFolder(Root + "/Models");
            EnsureFolder(Root + "/Meshes");
            EnsureFolder(Root + "/Textures/PBR");
            EnsureFolder(Root + "/Textures/DetailDecal");
            EnsureFolder(Root + "/Textures/Generated");
            EnsureFolder(Root + "/Materials");
            EnsureFolder(Root + "/Prefabs");
            AssetDatabase.Refresh();
            Debug.Log("Toyota Fortuner folder structure is ready.");
        }

        private static void EnsureFolder(string folder)
        {
            string[] parts = folder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
#endif
