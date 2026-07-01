#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SoftSquareGenerator
{
    [MenuItem("Tools/Generate Soft Square PNG")]
    public static void Generate()
    {
        int size = 256;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

        float softEdgeRadius = 40f; // Increase for softer edges, decrease for sharper edges

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Calculate distance to closest edge
                float distToLeft = x;
                float distToRight = size - 1 - x;
                float distToBottom = y;
                float distToTop = size - 1 - y;

                float minDist = Mathf.Min(Mathf.Min(distToLeft, distToRight), Mathf.Min(distToBottom, distToTop));

                // Calculate fading alpha based on distance to the edge
                float alpha = Mathf.Clamp01(minDist / softEdgeRadius);

                // Write pixel (solid black with fading alpha)
                texture.SetPixel(x, y, new Color(0f, 0f, 0f, alpha));
            }
        }

        texture.Apply();

        // Save the texture physically as a PNG into your Assets folder
        byte[] bytes = texture.EncodeToPNG();
        string path = Path.Combine(Application.dataPath, "SoftSquare.png");
        File.WriteAllBytes(path, bytes);

        AssetDatabase.Refresh();

        // Automatically configure the file as an editable Sprite
        string relativePath = "Assets/SoftSquare.png";
        TextureImporter importer = AssetImporter.GetAtPath(relativePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        Debug.Log("SoftSquare.png generated successfully at: " + relativePath);
    }
}
#endif