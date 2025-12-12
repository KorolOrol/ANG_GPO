using System;
using UnityEngine;

namespace MapScripts.LocationGenerate
{
    public class LocationMapGenerator : MonoBehaviour
    {
        public int width = 50;
        public int height = 50;
        public float noiseScale = 10f;

        public Texture2D CityTexture { get; }
        public Renderer mapRenderer;

        public LocationMapGenerator(Texture2D cityTexture)
        {
            CityTexture = cityTexture;
        }

        public void GenerateLocationMap(Location location)
        {
            var tex = new Texture2D(width, height);

            float offsetX = UnityEngine.Random.Range(0f, 1000f);
            float offsetY = UnityEngine.Random.Range(0f, 1000f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float xCoord = x / (float)width * noiseScale + offsetX;
                    float yCoord = y / (float)height * noiseScale + offsetY;
                    float sample = Mathf.PerlinNoise(xCoord, yCoord);
                    var col = sample > 0.5f ? Color.gray : Color.green;
                    tex.SetPixel(x, y, col);
                }
            }

            tex.Apply();
            mapRenderer.material.mainTexture = tex;
        }

        public static void LoadLocationMap(Location location)
        {
            throw new NotImplementedException("Method is not implemented");
        }
    }
}
