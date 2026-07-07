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
        private const string CircleSpritePath = "Assets/ChaiEmpire/Resources/ChaiEmpire/chai-circle.png";

        [MenuItem("Chai Empire/Rebuild Main Scene")]
        public static void Build()
        {
            Directory.CreateDirectory("Assets/ChaiEmpire/Scenes");
            EnsureCircleSpriteAsset();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "ChaiEmpire";

            GameObject camera = new GameObject("Main Camera");
            Camera cameraComponent = camera.AddComponent<Camera>();
            cameraComponent.clearFlags = CameraClearFlags.SolidColor;
            cameraComponent.backgroundColor = new Color(0.05f, 0.16f, 0.17f);
            cameraComponent.orthographic = true;
            cameraComponent.orthographicSize = 5f;
            camera.transform.position = new Vector3(0, 0, -10);
            camera.tag = "MainCamera";
            camera.AddComponent<AudioListener>();

            GameObject app = new GameObject("Chai Empire App");
            ChaiGamePresenter presenter = app.AddComponent<ChaiGamePresenter>();
            presenter.RebuildPersistentPreview();
            BuildScenePreviewStall(app.transform);

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

        private static void BuildScenePreviewStall(Transform parent)
        {
            Sprite circle = AssetDatabase.LoadAssetAtPath<Sprite>(CircleSpritePath);
            if (circle == null)
            {
                return;
            }

            GameObject preview = new GameObject("Scene Preview Chai Stall");
            preview.transform.SetParent(parent, false);
            preview.transform.localPosition = Vector3.zero;

            CreatePreviewSprite(preview.transform, "Preview Back Wall", circle, new Vector2(0, 1.2f), new Vector2(5.6f, 2.2f), new Color(1f, 0.86f, 0.55f));
            CreatePreviewSprite(preview.transform, "Preview Counter", circle, new Vector2(0, -1.15f), new Vector2(6.2f, 0.58f), new Color(0.45f, 0.23f, 0.12f));
            CreatePreviewSprite(preview.transform, "Preview Stove", circle, new Vector2(0, -0.56f), new Vector2(1.9f, 0.82f), new Color(0.17f, 0.18f, 0.17f));
            CreatePreviewSprite(preview.transform, "Preview Flame Outer", circle, new Vector2(0, -0.2f), new Vector2(0.7f, 0.72f), new Color(0.93f, 0.43f, 0.16f));
            CreatePreviewSprite(preview.transform, "Preview Kettle Body", circle, new Vector2(0, 0.48f), new Vector2(2.15f, 1.18f), new Color(0.78f, 0.86f, 0.83f));
            CreatePreviewSprite(preview.transform, "Preview Kettle Belly", circle, new Vector2(0, 0.34f), new Vector2(1.55f, 0.76f), new Color(0.12f, 0.42f, 0.42f));
            CreatePreviewSprite(preview.transform, "Preview Kettle Handle", circle, new Vector2(-1.28f, 0.64f), new Vector2(0.74f, 0.98f), new Color(0.12f, 0.42f, 0.42f));
            CreatePreviewSprite(preview.transform, "Preview Kettle Spout", circle, new Vector2(1.24f, 0.64f), new Vector2(0.96f, 0.34f), new Color(0.12f, 0.42f, 0.42f));
            CreatePreviewSprite(preview.transform, "Preview Steam A", circle, new Vector2(-0.38f, 1.36f), new Vector2(0.17f, 0.62f), new Color(1f, 0.95f, 0.82f, 0.7f));
            CreatePreviewSprite(preview.transform, "Preview Steam B", circle, new Vector2(0.12f, 1.5f), new Vector2(0.16f, 0.74f), new Color(1f, 0.95f, 0.82f, 0.58f));
            CreatePreviewSprite(preview.transform, "Preview UPI QR Prop", circle, new Vector2(-2.28f, -0.52f), new Vector2(0.72f, 0.92f), new Color(0.96f, 0.95f, 0.88f));
            CreatePreviewSprite(preview.transform, "Preview Customer A", circle, new Vector2(2.05f, -0.46f), new Vector2(0.48f, 0.9f), new Color(0.18f, 0.50f, 0.31f));
            CreatePreviewSprite(preview.transform, "Preview Customer B", circle, new Vector2(2.62f, -0.42f), new Vector2(0.5f, 1f), new Color(0.93f, 0.43f, 0.16f));
        }

        private static void CreatePreviewSprite(Transform parent, string name, Sprite sprite, Vector2 position, Vector2 scale, Color color)
        {
            GameObject shape = new GameObject(name);
            shape.transform.SetParent(parent, false);
            shape.transform.localPosition = new Vector3(position.x, position.y, 0);
            shape.transform.localScale = new Vector3(scale.x, scale.y, 1);

            SpriteRenderer renderer = shape.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
        }

        private static void EnsureCircleSpriteAsset()
        {
            Directory.CreateDirectory("Assets/ChaiEmpire/Resources/ChaiEmpire");

            if (!File.Exists(CircleSpritePath))
            {
                const int size = 64;
                const float radius = (size - 1) * 0.5f;
                Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
                {
                    name = "chai-circle",
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };

                Color32[] pixels = new Color32[size * size];
                Color32 clear = new Color32(255, 255, 255, 0);
                Color32 fill = new Color32(255, 255, 255, 255);

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dx = x - radius;
                        float dy = y - radius;
                        pixels[y * size + x] = (dx * dx + dy * dy) <= radius * radius ? fill : clear;
                    }
                }

                texture.SetPixels32(pixels);
                texture.Apply();
                File.WriteAllBytes(CircleSpritePath, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
            }

            AssetDatabase.ImportAsset(CircleSpritePath, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(CircleSpritePath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }

            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }
    }
}
