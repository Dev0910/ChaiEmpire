using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ChaiEmpire.Editor
{
    public static class ChaiContentEditorValidation
    {
        private const string CatalogAssetPath = "Assets/ChaiEmpire/Resources/ChaiEmpire/default-content.json";

        [MenuItem("Chai Empire/Content/Validate Default Catalog")]
        public static void ValidateDefaultCatalog()
        {
            ChaiContentData data = LoadDefaultCatalog();
            Debug.Log("Chai Empire content catalog is valid: " + data.upgrades.Length + " upgrades, " + data.locations.Length + " locations.");
        }

        [MenuItem("Chai Empire/Content/Export Built-In Catalog JSON")]
        public static void ExportBuiltInCatalogJson()
        {
            ChaiContentData data = ChaiContentData.CreateBuiltInDefault();
            IReadOnlyList<string> errors = ChaiContentValidator.Validate(data);
            if (errors.Count > 0)
            {
                throw new InvalidOperationException("Built-in content catalog is invalid: " + string.Join("; ", errors));
            }

            string fullPath = GetCatalogFullPath();
            string directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(fullPath, ChaiContentData.ToJson(data));
            AssetDatabase.ImportAsset(CatalogAssetPath);
            Debug.Log("Chai Empire built-in content exported to " + CatalogAssetPath);
        }

        private static ChaiContentData LoadDefaultCatalog()
        {
            string fullPath = GetCatalogFullPath();
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("Chai Empire content catalog was not found.", CatalogAssetPath);
            }

            string json = File.ReadAllText(fullPath);
            if (!ChaiContentData.TryFromJson(json, out ChaiContentData data, out string error))
            {
                throw new InvalidOperationException("Chai Empire content catalog is invalid: " + error);
            }

            return data;
        }

        private static string GetCatalogFullPath()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "ChaiEmpire/Resources/ChaiEmpire/default-content.json"));
        }
    }
}
