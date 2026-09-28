using UnityEditor;
using UnityEngine;

namespace ColorBlockJam.Art.Editor
{
    /// <summary>
    /// Applies the project's default import settings to assets dropped into the Art folders.
    /// Runs only on the first import, so manual tweaks made later in the Inspector are kept.
    /// </summary>
    public sealed class ArtImportRules : AssetPostprocessor
    {
        private const string UiFolder = "Assets/_Project/Art/UI/";
        private const string TexturesFolder = "Assets/_Project/Art/Textures/";
        private const string ModelsFolder = "Assets/_Project/Art/Models/";

        private const string AndroidPlatform = "Android";
        private const int MaxTextureSize = 2048;

        private void OnPreprocessTexture()
        {
            if (!assetImporter.importSettingsMissing)
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;

            if (assetPath.StartsWith(UiFolder))
            {
                ApplyUiSpriteSettings(importer);
            }
            else if (assetPath.StartsWith(TexturesFolder))
            {
                ApplySurfaceTextureSettings(importer);
            }
        }

        private void OnPreprocessModel()
        {
            if (!assetImporter.importSettingsMissing || !assetPath.StartsWith(ModelsFolder))
            {
                return;
            }

            ApplyModelSettings((ModelImporter)assetImporter);
        }

        private static void ApplyUiSpriteSettings(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;

            // Full rect keeps 9-slice and tiled Image types working.
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);

            ApplyAndroidCompression(importer);
        }

        private static void ApplySurfaceTextureSettings(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Default;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;

            ApplyAndroidCompression(importer);
        }

        private static void ApplyAndroidCompression(TextureImporter importer)
        {
            importer.maxTextureSize = MaxTextureSize;
            importer.textureCompression = TextureImporterCompression.Compressed;

            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                name = AndroidPlatform,
                overridden = true,
                maxTextureSize = MaxTextureSize,
                format = TextureImporterFormat.ASTC_6x6,
                compressionQuality = (int)TextureCompressionQuality.Normal
            });
        }

        private static void ApplyModelSettings(ModelImporter importer)
        {
            importer.importAnimation = false;
            importer.animationType = ModelImporterAnimationType.None;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importVisibility = false;
            importer.importBlendShapes = true;
            importer.isReadable = false;
            importer.addCollider = false;
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;

            // Materials are owned by the project, not by the FBX files.
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
        }
    }
}
