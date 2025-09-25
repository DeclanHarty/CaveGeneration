using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticNoiseGenerator
{
    public static float[] GenerateStaticNoise(int width, int height){
        float[] staticNoise = new float[width * height];

        for(int y = 0; y < height; y++){
            for(int x = 0; x < width; x++){
                staticNoise[y * width + x] = Random.Range(0f, 1f);
            }
        }

        return staticNoise;
    }

    public static int[,] GenerateStaticBWNoise(int width, int height, float cutoff){
        int[,] staticNoise = new int[width,height];

        // O(width * height)
        for(int y = 0; y < height; y++){  // O(height)
            for(int x = 0; x < width; x++){ // O(width)
                staticNoise[x,y] = Random.Range(0f, 1f) <= cutoff ? 1 : 0;
            }
        }

        return staticNoise;
    }
}
