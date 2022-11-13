using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MakeSprite : AssetPostprocessor {

	void OnPreprocessTexture()
	{
		if (assetPath.Contains("Submodule")) return;

        if (assetPath.Contains("Sprites"))
        {
            Debug.Log("MakeSprite: Importing and making new sprite...");
            TextureImporter imp = (TextureImporter)assetImporter;
            imp.textureType = TextureImporterType.Sprite;
            imp.maxTextureSize = 2048;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.spritePixelsPerUnit = 100;
        }

        if (assetPath.Contains("Pixel Art"))
		{
			Debug.Log("MakeSprite: Importing and making new sprite...");
			TextureImporter imp = (TextureImporter)assetImporter;
			imp.textureType = TextureImporterType.Sprite;
			if(imp.filterMode != FilterMode.Point) imp.filterMode = FilterMode.Point;
			imp.maxTextureSize = 2048;
			imp.textureCompression = TextureImporterCompression.Uncompressed;
			imp.spritePixelsPerUnit = 100;
		}
	}
}
