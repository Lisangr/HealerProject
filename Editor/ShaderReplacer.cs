using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ShaderReplacer : EditorWindow
{
    [MenuItem("Tools/Replace Shaders (ComplexLit/Lit to SimpleLit)")]
    public static void ReplaceShaders()
    {
        // Ўейдеры дл€ поиска и замены (обычные материалы)
        var shadersToReplace = new List<string>
        {
            "Universal Render Pipeline/Complex Lit",
            "Universal Render Pipeline/Lit"
        };

        // ÷елевой шейдер дл€ обычных материалов
        Shader targetShader = Shader.Find("Universal Render Pipeline/Simple Lit");
        if (targetShader == null)
        {
            Debug.LogError("Simple Lit shader not found!");
            return;
        }

        // ÷елевой шейдер дл€ материалов с частицами
        Shader particlesTargetShader = AssetDatabase.LoadAssetAtPath<Shader>("Packages/com.unity.render-pipelines.universal/Shaders/Particles/ParticlesSimpleLit.shader");
        if (particlesTargetShader == null)
        {
            Debug.LogError("ParticlesSimpleLit shader not found!");
            return;
        }

        // ѕоиск всех материалов в проекте
        string[] materialGuids = AssetDatabase.FindAssets("t:Material");
        int replacedCount = 0;

        foreach (string guid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

            // «амена дл€ обычных материалов
            if (shadersToReplace.Contains(material.shader.name))
            {
                Undo.RecordObject(material, "Change Shader");
                material.shader = targetShader;
                EditorUtility.SetDirty(material);
                replacedCount++;

                Debug.Log($"Replaced shader in: {path}", material);
            }
            // «амена дл€ материалов с шейдером частиц
            else if (AssetDatabase.GetAssetPath(material.shader) == "Packages/com.unity.render-pipelines.universal/Shaders/Particles/ParticlesLit.shader")
            {
                Undo.RecordObject(material, "Change Shader");
                material.shader = particlesTargetShader;
                EditorUtility.SetDirty(material);
                replacedCount++;

                Debug.Log($"Replaced particles shader in: {path}", material);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Complete! Replaced {replacedCount} materials.");
    }
}
