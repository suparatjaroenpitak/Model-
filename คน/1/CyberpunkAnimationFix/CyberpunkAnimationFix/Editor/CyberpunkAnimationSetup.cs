using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class CyberpunkAnimationSetup
{
    static readonly string[] Names = {"Idle","Walk","Run","Jump","Land"};
    [MenuItem("Tools/Cyberpunk/Create Motion Controller from selected FBX")]
    static void Create()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (!(AssetImporter.GetAtPath(path) is ModelImporter importer))
        { Debug.LogError("Select the rigged FBX in Project first."); return; }
        if (!importer.importAnimation) { importer.importAnimation = true; importer.SaveAndReimport(); }
        var clips = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__preview__")).ToArray();
        AnimationClip Find(string n) => clips.FirstOrDefault(c => string.Equals(c.name,n,StringComparison.OrdinalIgnoreCase) || c.name.EndsWith("|"+n,StringComparison.OrdinalIgnoreCase));
        foreach (var n in Names) if (!Find(n)) { Debug.LogError("Missing clip: " + n + ". Found: " + string.Join(", ", clips.Select(c=>c.name))); return; }
        var output = AssetDatabase.GenerateUniqueAssetPath("Assets/CyberpunkMotion.controller");
        var controller = AnimatorController.CreateAnimatorControllerAtPath(output);
        var machine = controller.layers[0].stateMachine;
        foreach (var n in Names)
        {
            var source = Find(n);
            var clip = UnityEngine.Object.Instantiate(source);
            clip.name = n;
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = n == "Idle" || n == "Walk" || n == "Run";
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.AddObjectToAsset(clip, controller);
            var state = machine.AddState(n);
            state.motion = clip;
            if (n == "Idle") machine.defaultState = state;
            Debug.Log(n + ": " + source.name + " duration=" + source.length + " bindings=" + AnimationUtility.GetCurveBindings(source).Length + " humanoid=" + source.humanMotion);
        }
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Selection.activeObject = controller;
        Debug.Log("Created " + output + ". Assign it to the model Animator.");
    }
}
