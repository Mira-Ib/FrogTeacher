using UnityEditor;

public class TextureAssetPostProcessor : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        var textureImporter = assetImporter as TextureImporter;

        if (textureImporter is { importSettingsMissing: true })
        {
            if (textureImporter.spriteImportMode == SpriteImportMode.Multiple)
            {
                textureImporter.spriteImportMode = SpriteImportMode.Single;
            }
        }
    }
}
