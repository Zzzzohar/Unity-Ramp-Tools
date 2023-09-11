using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class GradientCreator : EditorWindow
{
    [MenuItem("*Athena*/ArtistTools/GradientCreator")]
    private static void ShowWindow()
    {
        var window = GetWindow<GradientCreator>();
        window.titleContent = new GUIContent("GradientCreator");
        window.Show();
    }

    ///<绘制面板>
    private GradientCreatorData.WidthSize _GradientWidth = GradientCreatorData.WidthSize._128;//每一条渐变的宽度
    private int _GradientHeight = 2;//每一条渐变的高度
    public GradientCreatorData _GradientData;
    private Texture2D _GradientMap;
    string _GradientName = "Modify here";
    public GradientCreatorData.Format index;
    int FormatIndex = 0;
    private string _Format = ".tga";
    
    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        _GradientData = EditorGUILayout.ObjectField("GradientData", _GradientData, typeof(GradientCreatorData), false) as GradientCreatorData;
        if (EditorGUI.EndChangeCheck())//结束检查是否有修改
        {
            AnalyzeData(_GradientData);
        }


        GradientListGUI();

        _GradientMap = Create(_Gradient, (int)_GradientWidth, _Gradient.Count * _GradientHeight);
        SceneView.RepaintAll();
        DrawGUI();
        SaveTextureAndSaveData();
    }

    public void AnalyzeData(GradientCreatorData data)
    {
        if (!data) return;
        _GradientWidth = data._GradientWidth;
        _GradientName = data._GradientName;
        _Gradient = data._Gradient;
        _Gradient = _GradientData._Gradient;
    }
    

    ///<绘制渐变控制列表>
    [SerializeField]
    public List<Gradient> _Gradient = new List<Gradient>();
    protected SerializedObject _serializedObject;    
    protected SerializedProperty _assetLstProperty;  
    private void GradientListGUI()//绘制列表
    {
        if (_Gradient.Count <= 0)
        {
            Gradient temp = new Gradient();
            _Gradient.Add(temp);
        }
        _serializedObject.Update();
        EditorGUILayout.PropertyField(_assetLstProperty, true);//显示属性 //第二个参数必须为true，否则无法显示子节点即List内容

    }
    //绘制GUI
    private void DrawGUI()
    {
        _GradientWidth = (GradientCreatorData.WidthSize)EditorGUILayout.EnumPopup("每条渐变宽度(像素)",_GradientWidth, GUILayout.Width(800));
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("纹理名称", GUILayout.Width(100));

        _GradientName = EditorGUILayout.TextArea(_GradientName);

        index = (GradientCreatorData.Format)EditorGUILayout.EnumPopup(index, GUILayout.Width(100));
        _Format = ".tga";
        if (index == GradientCreatorData.Format.TGA)
        {
            _Format = ".tga";
        }else if (index == GradientCreatorData.Format.PNG)
        {
            _Format = ".tga";
        }else
            _Format = ".jpg";
        EditorGUILayout.EndHorizontal();
    }
    ///<存储纹理>
    private void SaveTextureAndSaveData()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save Texture"))
        {
            string path = EditorUtility.SaveFolderPanel("Select an output path", _GradientData?_GradientData.path : "", "");
            string subPath = path.Substring(Application.dataPath.Length - 6);
            // Save the texture to disk
            byte[] pngData = _GradientMap.EncodeToTGA();
            if (index == GradientCreatorData.Format.TGA)
                pngData = _GradientMap.EncodeToTGA();
            if (index == GradientCreatorData.Format.PNG)
                pngData = _GradientMap.EncodeToPNG();
            if (FormatIndex == 2)
                pngData = _GradientMap.EncodeToJPG();

            
            File.WriteAllBytes(path + "/" + _GradientName + _Format, pngData);
            AssetDatabase.Refresh();
            
            TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(subPath + "/" + _GradientName + _Format);
            TextureImporterSettings setting = new TextureImporterSettings();
            if (importer != null)
            {
                importer.ReadTextureSettings(setting);
                setting.wrapMode = TextureWrapMode.Clamp;
                setting.mipmapEnabled = false;
                importer.SetTextureSettings(setting);
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            if (_GradientData && path == _GradientData.path)
            {
                _GradientData._Gradient = _Gradient;
                _GradientData._GradientName = _GradientName;
                _GradientData._GradientWidth = _GradientWidth;
                _GradientData.format = index;
                _GradientData.path = path;
            }
            else
            {
                GradientCreatorData data = ScriptableObject.CreateInstance<GradientCreatorData>();
                data._Gradient = _Gradient;
                data._GradientName = _GradientName;
                data._GradientWidth = _GradientWidth;
                data.format = index;
                data.path = path;
                AssetDatabase.CreateAsset(data, subPath + "/" + _GradientName + ".asset");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                _GradientData = data;
            }
            
        }
        
        EditorGUILayout.EndHorizontal();
    }

    ///<序列化属性列表>
    private void OnEnable()
    {
        _serializedObject = new SerializedObject(this);//使用当前类初始化
        _assetLstProperty = _serializedObject.FindProperty("_Gradient");
    }

    ///<生成纹理函数>
    Texture2D Create(List<Gradient> Gradient, int width = 32, int height = 1)
    {
        var _GradientMap = new Texture2D(width, height, TextureFormat.ARGB32, false);
        _GradientMap.filterMode = FilterMode.Bilinear;
        float inv = 1f / (width - 1);

        int eachHeight = height / 1;
        if (Gradient.Count != 0)
        {
            eachHeight = height / Gradient.Count;
        }

        int howMany = 0;
        while (howMany != Gradient.Count)
        {
            int start = height - eachHeight * howMany - 1;
            int end = start - eachHeight;
            for (int y = start; y > end; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    var t = x * inv;
                    Color col = Gradient[howMany].Evaluate(t);
                    _GradientMap.SetPixel(x, y, col);
                }
            }
            howMany++;
        }
        _GradientMap.Apply();
        return _GradientMap;
    }
}