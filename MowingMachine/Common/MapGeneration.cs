using System;
using System.Drawing;
using System.Drawing.Imaging;
using MowingMachine.Models;
using SimplexNoise;

namespace MowingMachine.Common
{
    public static class MapGeneration
    {
        public static int[][] GenerateMapGetMap(int mapSize, int seed)
        {
            const float scale = 0.1f;

            int[][] mapSample = new int[mapSize][];
            Noise.Seed = seed;
            
            float[,] noiseValues = Noise.Calc2D(mapSize, mapSize, scale);

            for (int x = 0; x < mapSample.Length; x++)
            {
                mapSample[x] = new int[mapSize];
                for (int y = 0; y < mapSample.Length; y++)
                {
                    var value = (int)noiseValues[x, y];
                    mapSample[x][y] = (int) IntToFieldType(value);
                }
            }

            return mapSample;
        }

        private static FieldType IntToFieldType(int value)
        {
            if (value > 200) return FieldType.Water;
            if (value > 150) return FieldType.CobbleStone;
            if (value > 100) return FieldType.Sand;
            
            return FieldType.Grass;
        }
    }
}