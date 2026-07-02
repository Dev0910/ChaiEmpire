using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChaiEmpire.Editor
{
    public static class ChaiEmpireSceneBuilder
    {
        private const string ScenePath = "Assets/ChaiEmpire/Scenes/ChaiEmpire.unity";

        [MenuItem("Chai Empire/Rebuild Main Scene")]
        public static void Build()
        {
            Directory.CreateDirectory("Assets/ChaiEmpire/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "ChaiEmpire";

            GameObject camera = new GameObject("Main Camera");
            Camera cameraComponent = camera.AddComponent<Camera>();
            cameraComponent.clearFlags = CameraClearFlags.SolidColor;
            cameraComponent.backgroundColor = new Color(0.05f, 0.16f, 0.17f);
            camera.tag = "MainCamera";

            GameObject app = new GameObject("Chai Empire App");
            app.AddComponent<ChaiGamePresenter>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            PlayerSettings.companyName = "Tapri Labs";
            PlayerSettings.productName = "Chai Empire";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

            Debug.Log("Chai Empire main scene rebuilt at " + ScenePath);
        }
    }
}
