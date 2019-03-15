using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureUtil {

    public static Texture2D FromColor(Color color) {
        Texture2D texture = new Texture2D(1, 1);
        for (int x = 0; x < texture.width; x++) {
            for (int y = 0; y < texture.height; y++) {
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
        return texture;
    }

}
