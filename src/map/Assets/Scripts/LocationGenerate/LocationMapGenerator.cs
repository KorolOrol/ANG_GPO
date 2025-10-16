using UnityEngine;

public class LocationMapGenerator : MonoBehaviour
{
    public int width = 50;
    public int height = 50;
    public float noiseScale = 10f;

    public Texture2D cityTexture;
    public Renderer mapRenderer;

    public void GenerateLocationMap(Location location)
    {
        Texture2D tex = new Texture2D(width, height);

        float offsetX = Random.Range(0f, 1000f);
        float offsetY = Random.Range(0f, 1000f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float xCoord = x / (float)width * noiseScale + offsetX;
                float yCoord = y / (float)height * noiseScale + offsetY;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                Color col = sample > 0.5f ? Color.gray : Color.green;
                tex.SetPixel(x, y, col);
            }
        }

        tex.Apply();
        mapRenderer.material.mainTexture = tex;
    }

    public void LoadLocationMap(Location location)
    {
        // Загрузи сохранённую карту (если используешь сериализацию)
    }
}
