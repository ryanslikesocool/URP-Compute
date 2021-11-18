// Developed with love by Ryan Boyer http://ryanjboyer.com <3

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extentions {
    public static Vector4 ToVector4(this Color color) {
        return new Vector4(color.r, color.g, color.b, color.a);
    }
}