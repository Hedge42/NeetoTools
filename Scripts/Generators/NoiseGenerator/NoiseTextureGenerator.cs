using UnityEngine;
using System.IO;
using System;
using Random = UnityEngine.Random;

namespace Neeto
{

#if UNITY_EDITOR
    using UnityEditor;
    [CustomEditor(typeof(NoiseTextureGenerator))]
    public class NoiseTextureGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var _target = target as NoiseTextureGenerator;
            EditorGUI.BeginChangeCheck();

            base.OnInspectorGUI();

            if (EditorGUI.EndChangeCheck())
            {
                _target.GenerateNoiseTexture();
            }


            if (_target.texture != null && GUILayout.Button("Export"))
            {
                _target.Export();
            }

            if (GUILayout.Button("Randomize"))
            {
                _target.RandomizeInputParameters();
                _target.GenerateNoiseTexture();
            }
        }
    }
#endif


    public enum NoiseType { Perlin, Voronoi, Cellular }

    [CreateAssetMenu(menuName = "ScriptableObjects/Noise Texture Generator")]
    public class NoiseTextureGenerator : ScriptableObject
    {

        public int seed;
        public int width = 256;
        public int height = 256;
        public float scale = 20f;
        public Vector2 offset;

        [Range(1, 12)]
        public int octaves = 4;
        public float lacunarity = 2f;
        public float persistence = 0.5f;
        public NoiseType noiseType = NoiseType.Perlin;

        public int gridSize;

        [PreviewTexture]
        public Texture2D texture;



#if UNITY_EDITOR
        private void OnValidate()
        {
            if (texture == null)
                GenerateNoiseTexture();
        }
#endif

        [ContextMenu(nameof(GenerateNoiseTexture))]
        public void GenerateNoiseTexture()
        {
            texture = new Texture2D(width, height);

            switch (noiseType)
            {
                case NoiseType.Perlin:
                    Perlin();
                    break;
                case NoiseType.Cellular:
                    Cellular();
                    break;
                case NoiseType.Voronoi:
                    Voronoi();
                    break;
                default:
                    throw new System.NotImplementedException();
            }

            texture.Apply();

            RemapGrayscaleTexture(ref texture);
        }

#if UNITY_EDITOR
        public void Export()
        {
            byte[] bytes = texture.EncodeToPNG();
            string path = AssetDatabase.GetAssetPath(this);
            path = Path.GetDirectoryName(path) + "/" + name + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png";
            File.WriteAllBytes(path, bytes);

            // Refresh the asset database so the new PNG file shows up in the editor
            AssetDatabase.Refresh();
        }
#endif

        private void Perlin()
        {
            texture = new Texture2D(width, height);

            // Loop through each pixel in the texture
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float sampleX = (float)x / width * scale + offset.x;
                    float sampleY = (float)y / height * scale + offset.y;

                    float noiseValue = 0f;
                    float amplitudeValue = 1f;


                    for (int i = 0; i < octaves; i++)
                    {
                        float frequency = Mathf.Pow(lacunarity, i);
                        amplitudeValue *= persistence;

                        noiseValue += amplitudeValue * Mathf.PerlinNoise(seed + sampleX * frequency, seed + sampleY * frequency);
                    }

                    // Set the pixel color based on the noise value
                    Color pixelColor = new Color(noiseValue, noiseValue, noiseValue, 1f);
                    texture.SetPixel(x, y, pixelColor);
                }

            }

            texture.Apply();
        }
        private void Cellular()
        {
            texture = new Texture2D(width, height);

            // Loop through each pixel in the texture
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var position = new Vector2(x, y);

                    // Scale the position by the frequency
                    position *= scale / Mathf.Max(texture.width, texture.height);

                    // Calculate the noise values for each octave and combine them
                    float result = 0f;
                    float weight = 1f;
                    float signal = Mathf.PerlinNoise(position.x, position.y) * 2f - 1f;
                    float factor = 1f;

                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = position.x * factor + offset.x;
                        float sampleY = position.y * factor + offset.y;
                        float noise = (Mathf.PerlinNoise(sampleX, sampleY) * 2f - 1f) * weight;

                        result += noise * Mathf.Clamp01(signal);
                        weight *= persistence;
                        factor *= lacunarity;
                        signal = Mathf.PerlinNoise(sampleX, sampleY) * 2f - 1f;
                    }

                    // Remap the result to the range [0, 1]
                    var noiseValue = Mathf.InverseLerp(-octaves, octaves, result);

                    // Set the pixel color based on the noise value
                    Color pixelColor = new Color(noiseValue, noiseValue, noiseValue, 1f);

                    texture.SetPixel(x, y, pixelColor);
                }
            }

            texture.Apply();
        }
        private void Voronoi()
        {
            texture = new Texture2D(width, height);

            Random.State originalState = Random.state;

            var density = scale / Mathf.Max(height, width);
            Random.InitState(seed);

            var numPoints = (int)(Mathf.Max(height, width) * density);

            Vector2[] points = new Vector2[numPoints];
            for (int i = 0; i < numPoints; i++)
            {
                points[i] = new Vector2(Random.Range(offset.x, offset.x + width), Random.Range(offset.y, offset.y + height));
            }

            Random.state = originalState;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2 currentPoint = new Vector2(x, y) + offset;

                    float minDist = Mathf.Infinity;
                    for (int i = 0; i < numPoints; i++)
                    {
                        float dist = Vector2.Distance(currentPoint, points[i]);
                        if (dist < minDist)
                        {
                            minDist = dist;
                        }
                    }

                    float noiseValue = minDist / scale;

                    Color color = new Color(noiseValue, noiseValue, noiseValue);
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
        }

        public static void RemapGrayscaleTexture(ref Texture2D grayscaleTexture)
        {
            Color[] pixels = grayscaleTexture.GetPixels();

            float min = float.MaxValue;
            float max = float.MinValue;

            // Find the minimum and maximum values in the texture
            for (int i = 0; i < pixels.Length; i++)
            {
                float grayscaleValue = pixels[i].grayscale;
                if (grayscaleValue < min)
                {
                    min = grayscaleValue;
                }
                if (grayscaleValue > max)
                {
                    max = grayscaleValue;
                }
            }

            // Remap the values between 0 and 1
            for (int i = 0; i < pixels.Length; i++)
            {
                float grayscaleValue = pixels[i].grayscale;
                float remappedValue = Mathf.InverseLerp(min, max, grayscaleValue);
                pixels[i] = new Color(remappedValue, remappedValue, remappedValue);
            }

            // Update the texture with the remapped values
            grayscaleTexture.SetPixels(pixels);
            grayscaleTexture.Apply();
        }

        [ContextMenu(nameof(RandomizeInputParameters))]
        public void RandomizeInputParameters()
        {
            // Randomize the scale and offset within reasonable ranges
            scale = UnityEngine.Random.Range(1f, 100f);
            offset = new Vector2(UnityEngine.Random.Range(-1000f, 1000f), UnityEngine.Random.Range(-1000f, 1000f));

            // Randomize the number of octaves and lacunarity within reasonable ranges
            octaves = UnityEngine.Random.Range(1, 10);
            lacunarity = UnityEngine.Random.Range(1.5f, 3.5f);

            // Randomize the persistence within a reasonable range
            persistence = UnityEngine.Random.Range(0.1f, 0.9f);

            noiseType = (NoiseType)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(NoiseType)).Length);
        }
    }
}