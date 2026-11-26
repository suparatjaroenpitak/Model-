#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ModelGame.Vehicle.Editor
{
    [CustomEditor(typeof(FortunerVehicleGenerator))]
    public class FortunerVehicleGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10f);
            FortunerVehicleGenerator generator = (FortunerVehicleGenerator)target;

            if (GUILayout.Button("Generate Procedural Fortuner"))
            {
                Undo.RecordObject(generator, "Generate Fortuner Procedural Model");
                generator.GenerateModel();
                EditorUtility.SetDirty(generator);
            }

            if (GUILayout.Button("Clear Generated Model"))
            {
                Undo.RecordObject(generator, "Clear Fortuner Procedural Model");
                generator.ClearGeneratedModel();
                EditorUtility.SetDirty(generator);
            }

            using (new EditorGUI.DisabledScope(generator.GeneratedRoot == null))
            {
                if (GUILayout.Button("Save Generated Meshes To Meshes Folder"))
                {
                    SaveGeneratedMeshes(generator);
                }
            }

            EditorGUILayout.HelpBox(
                "The reference sheet's triangle counts are production targets. " +
                "This script creates a named procedural proxy; replace its saved meshes with the final sculpt/retopology when available.",
                MessageType.Info);
        }

        private static void SaveGeneratedMeshes(FortunerVehicleGenerator generator)
        {
            const string meshFolder = "Assets/Vehicle/FortunerProcedural/Meshes";
            EnsureFolder("Assets/Vehicle");
            EnsureFolder("Assets/Vehicle/FortunerProcedural");
            EnsureFolder(meshFolder);

            MeshFilter[] meshFilters = generator.GeneratedRoot.GetComponentsInChildren<MeshFilter>(true);
            int savedCount = 0;
            foreach (MeshFilter meshFilter in meshFilters)
            {
                if (meshFilter.sharedMesh == null || AssetDatabase.Contains(meshFilter.sharedMesh))
                {
                    continue;
                }

                Mesh meshCopy = Object.Instantiate(meshFilter.sharedMesh);
                meshCopy.name = generator.name + "_" + meshFilter.gameObject.name + "_Mesh";
                string assetPath = AssetDatabase.GenerateUniqueAssetPath(meshFolder + "/" + meshCopy.name + ".asset");
                AssetDatabase.CreateAsset(meshCopy, assetPath);
                meshFilter.sharedMesh = meshCopy;
                savedCount++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Fortuner Mesh Export", "Saved " + savedCount + " mesh assets.", "OK");
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(folderPath).Replace('\\', '/');
            string folderName = System.IO.Path.GetFileName(folderPath);
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
#endif
