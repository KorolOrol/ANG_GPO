using UnityEngine;

public class BiomeGenerator : MonoBehaviour
{
    [Header("Biome Noise Settings")]
    public float biomeNoiseScale = 50f;
    public int biomeOctaves = 4;
    [Range(0, 1)]
    public float biomePersistance = 0.5f;
    [Range(1, 10)]
    public float biomeLacunarity = 2f;
    public int seed = 42;
    public Vector2 offset;

    /// <summary>
    /// Генерирует карту шума для биомов.
    /// </summary>
    public float[,] GenerateBiomeNoiseMap(int width, int height)
    {
        return Noise.GenerateNoiseMap(width, height, seed, biomeNoiseScale, biomeOctaves, biomePersistance, biomeLacunarity, offset);
    }
}
