using System;
using UnityEngine;
using UnityEditor;

public static class RectHelper
{
	/// <summary>
    /// Returns smaller version of given Rect 
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="padding"></param>
    /// <returns></returns>
	public static Rect Padding(this Rect rect, float padding)
	{
		rect.position += new Vector2(padding, padding);
		rect.size -= new Vector2(padding, padding) * 2f;
		return rect;
	}

	/// <summary>
    /// Split given rect on same height rows
    /// </summary>
    /// <param name="rect"></param>
	/// <param name="count"></param>
    /// <returns></returns>
	public static Rect[] GetRows(this Rect rect, int count)
	{
		Rect[] rows = new Rect[count];
		float width = rect.width;
		float height = rect.height / count;
		Vector2 position = rect.position;

		for (int i = 0; i < rows.Length; i++)
		{
			rows[i] = new Rect(position + new Vector2(0, height * i), new Vector2(width, height));
		}

		return rows;
	}

    /// <summary>
    /// Values from splitOps will be used as widths of columns
    /// In case if value = -1 it means that column width will expand
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="splitOps"></param>
    /// <returns></returns>
    public static Rect[] GetColumns(this Rect rect, float[] splitOps)
	{
		float widthTotal = 0.0f;
		int autoWidthCnt = splitOps.Length;
		foreach (var value in splitOps)
		{
			widthTotal += value;
			autoWidthCnt -= Mathf.CeilToInt(Mathf.Clamp01(value));
		}

		float width = (rect.width - widthTotal) / autoWidthCnt;
		float height = rect.height;
		Vector2 position = Vector2.zero;

		Rect[] column = new Rect[splitOps.Length];

		float lastWidth = 0.0f;

		for (var i = 0; i < column.Length; i++)
		{
			var rectWidth = 0.0f;
			if (splitOps[i] <= 0)
			{
				rectWidth = width;
			}
			else
			{
				rectWidth = splitOps[i];
			}

			column[i] = new Rect(position + new Vector2(lastWidth, 0), new Vector2(rectWidth, height));
			lastWidth += rectWidth;
		}

		return column;
	}
}

public class MultiLayerTextureDrawer : MaterialPropertyDrawer
{
	private static Texture _borderTex => EditorGUIUtility.IconContent("OL box@2x").image;
	private static Texture _checkerTex => EditorGUIUtility.IconContent("textureCheckerDark").image;

	private static Shader _shader => Shader.Find("Hidden/MultiLayerTexturePreview");
	private static readonly int mainTex = Shader.PropertyToID("_MainTex");
	private static readonly int checkerTex = Shader.PropertyToID("_CheckerTex");
	private static readonly int mode = Shader.PropertyToID("_Mode");

	private GUIStyle buttonStyleNormal => new GUIStyle()
	{
		fixedHeight = 12f,
		fontSize = 8,
		contentOffset = new Vector2(2, -1),
		alignment = TextAnchor.MiddleLeft,
		border = new RectOffset(1, 1, 0, 0),
		normal = new GUIStyleState() {background = (Texture2D) EditorGUIUtility.IconContent("mini btn mid").image}
	};

	private GUIStyle buttonStyleActive => new GUIStyle()
	{
		fixedHeight = 12f,
		fontSize = 8,
		contentOffset = new Vector2(2, -1),
		alignment = TextAnchor.MiddleLeft,
		border = new RectOffset(1, 1, 1, 2),
		normal = new GUIStyleState() {background = (Texture2D) EditorGUIUtility.IconContent("mini btn mid focus").image, textColor = Color.green}
	};

    public int _previewMode;
    private Material _previewMaterial;

    /// <summary>
    /// Values in this array will be used as widths of columns for GUI layout
    /// In case if value = -1 it means that column will expand
    /// </summary>
    private static readonly float[] columnSplitOps = new[]
	{
		-1.0f, 20.0f, 64.0f,
	};

	private static readonly GUIContent[] modeGuiContents = new[]
	{
		new GUIContent("RGB"), new GUIContent("R"), new GUIContent("G"), new GUIContent("B"), new GUIContent("A"),
    };

	public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
	{
		return 64.0f;
	}

	public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
	{
		if (_previewMaterial == null)
		{
			_previewMaterial = new Material(_shader);
			_previewMaterial.SetTexture(mainTex, prop.textureValue);
			_previewMaterial.SetTexture(checkerTex, _checkerTex);
			_previewMaterial.SetFloat(mode, _previewMode);
        }

		/*
		var target = (Material) editor.target;
		var offset = target.GetTextureOffset(prop.name);

		EditorGUIUtility.labelWidth = 60f;
		Rect rect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth + 2f, 18f);

		EditorGUI.LabelField(rect, "Offset");

		rect.x += EditorGUIUtility.labelWidth + 2f;
		rect.width = position.width - rect.width;

		EditorGUI.Vector2Field(rect, GUIContent.none, offset);
		*/

		var columns = position.GetColumns(columnSplitOps);

        GUI.BeginGroup(position);

        var labelRect = new Rect(columns[0]) {height = 18f};
       
        GUI.Label(labelRect, label, EditorStyles.boldLabel);
        labelRect.position += new Vector2(76f, 0);
        editor.TexturePropertyMiniThumbnail(labelRect, prop, String.Empty, String.Empty);
        
        
        columns[0].size -= new Vector2(3, 3);
        editor.TextureScaleOffsetProperty(columns[0], prop);
		
        DrawPreview(columns[2], prop.textureValue);

		var buttonRects = columns[1].GetRows(5);
		for (int i = 0; i < 5; i++)
		{
			var rect = buttonRects[i];
			var content = modeGuiContents[i];

			if (i == _previewMode)
			{
				GUI.Box(rect, content, buttonStyleActive);
				continue;
			}

			if (GUI.Button(rect, content, buttonStyleNormal))
			{
				_previewMode = i;
			}
		}
		

        GUI.EndGroup();
	}

	void DrawPreview(Rect rect, Texture texture)
	{
		EditorGUI.DrawPreviewTexture(rect, _borderTex);
		_previewMaterial.SetFloat(mode, _previewMode);

		if (texture != null)
			EditorGUI.DrawPreviewTexture(rect.Padding(3), texture, _previewMaterial);
    }
}
