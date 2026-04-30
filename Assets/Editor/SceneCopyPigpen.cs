using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Editor-only utility: copy Pigpen objects from a source scene into the currently open scene.
public static class SceneCopyPigpen
{
    const string sourceScene = "Assets/Scenes/SampleScene Djouhara.unity";

    [MenuItem("Tools/Merge Pigpen From Djouhara Scene")]
    public static void MergePigpen()
    {
        if (!EditorApplication.isPlaying)
        {
            var active = EditorSceneManager.GetActiveScene();
            if (!active.isLoaded)
            {
                EditorUtility.DisplayDialog("Merge Pigpen", "No active scene loaded. Open the destination scene first.", "OK");
                return;
            }

            if (!System.IO.File.Exists(sourceScene))
            {
                EditorUtility.DisplayDialog("Merge Pigpen", "Source scene not found: " + sourceScene, "OK");
                return;
            }

            // Open source scene additively
            var src = EditorSceneManager.OpenScene(sourceScene, OpenSceneMode.Additive);
            int copied = 0;

            // Find objects with PigpenBoardController in the source scene
            var controllers = Object.FindObjectsOfType<PigpenBoardController>();
            foreach (var ctrl in controllers)
            {
                if (ctrl == null) continue;

                // Make sure the object belongs to the source scene
                if (ctrl.gameObject.scene.path != src.path) continue;

                // Duplicate the root GameObject into the active scene
                GameObject root = ctrl.gameObject.transform.root.gameObject;
                GameObject copy = Object.Instantiate(root);
                copy.name = root.name + "_merged";
                SceneManager.MoveGameObjectToScene(copy, active);
                copied++;
            }

            // Optionally copy Pigpen UI panels (find by PigpenPuzzleUI)
            var uis = Object.FindObjectsOfType<PigpenPuzzleUI>();
            foreach (var ui in uis)
            {
                if (ui == null) continue;
                if (ui.gameObject.scene.path != src.path) continue;

                GameObject root = ui.gameObject.transform.root.gameObject;
                // Avoid duplicating if already copied via controller
                bool already = false;
                foreach (Transform t in root.transform)
                {
                    if (t.GetComponent<PigpenBoardController>() != null) { already = true; break; }
                }
                if (already) continue;

                GameObject copy = Object.Instantiate(root);
                copy.name = root.name + "_merged";
                SceneManager.MoveGameObjectToScene(copy, active);
                copied++;
            }

            // Close the source scene
            EditorSceneManager.CloseScene(src, true);

            if (copied > 0)
            {
                EditorSceneManager.MarkSceneDirty(active);
                EditorSceneManager.SaveScene(active);
                EditorUtility.DisplayDialog("Merge Pigpen", $"Copied {copied} GameObject(s) from Djouhara into {active.name}.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Merge Pigpen", "No Pigpen objects found in the source scene.", "OK");
            }
        }
        else
        {
            EditorUtility.DisplayDialog("Merge Pigpen", "Stop Play Mode before running this tool.", "OK");
        }
    }
}
