using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GradientCreatorData", menuName = "Data/GradientCreatorData", order = 0)]
public class GradientCreatorData : ScriptableObject
{
    [HideInInspector]
    public string path = " ";
    public string _GradientName = "Gradient";
    public enum WidthSize
    {
        _64 = 64,
        _128 = 128,
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
    }

    public WidthSize _GradientWidth = WidthSize._128;//每一条渐变的宽度
    private int _GradientHeight = 2;//每一条渐变的高度
    
    public List<Gradient> _Gradient = new List<Gradient>();
    public enum Format
    {
        TGA, PNG,JPG
    }

    public Format format;
}