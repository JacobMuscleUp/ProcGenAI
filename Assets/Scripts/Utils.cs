using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static Color ModifyAlpha(Color _color, float _alpha)
    {
        return new Color(_color[0], _color[1], _color[2], _alpha);
    }

    public static void ModifyAlpha(Renderer _renderer, float _alpha)
    {
        _renderer.material.color = Utils.ModifyAlpha(_renderer.material.color, _alpha);
    }
}
