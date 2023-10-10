using System;
using UnityEditor;
using UnityEngine;

public abstract class SingleExtensionApplication : ScriptableObject
{
    private EditorWindow window;
    public abstract string DisplayName { get; }
    protected static HelpBox helpBox = HelpBox.GetInstance();
    protected int defaultSpace = 10;
    public abstract bool ShouldLoadEditorPrefs { get; set; }

    //hlb = highlight button
    private protected GUIStyle richTextStyle,
        hlbStyle;
    private protected GUISkin hlbSkin;

    private const string HighlightButtonRessourcePath = "HighlightButton";

    public void Initialize(EditorWindow window)
    {
        this.window = window;
    }

    public abstract void OnGUI();

    public virtual void OnEnable() { }

    public void Repaint() => window.Repaint();

    public void Focus() => window.Focus();

    public void Show()
    {
        window.Show();
    }

    protected void InitializeGuiStyles()
    {
        richTextStyle ??= new GUIStyle(GUI.skin.textArea) { richText = true };
        GUIStyle defaultStyle = new(GUI.skin.button) { fontStyle = FontStyle.Bold };
        hlbSkin = Resources.Load<GUISkin>(HighlightButtonRessourcePath);
        if (hlbSkin != null && hlbSkin.button != null)
        {
            hlbStyle = new(hlbSkin.button);
        }
        else
        {
            hlbStyle = defaultStyle;
        }
    }

    public HelpBox GetHelpBox() => helpBox;

    protected void RenderHelpBox()
    {
        if (string.IsNullOrEmpty(helpBox.HBMessage))
            return;
        EditorGUILayout.HelpBox(helpBox.HBMessage, helpBox.HBMessageType);
        helpBox.RenderProgressBar();
    }

    protected void ShowProgressBar(float progress)
    {
        helpBox.UpdateIntendedProgress(progress);
        EditorApplication.update += helpBox.UpdateProgressBar;
        Repaint();
    }

    protected void AddDefaultSpace() => GUILayout.Space(defaultSpace);

    protected void ResetKeyboardControl() => GUIUtility.keyboardControl = 0;

    public void Close() => window.Close();

    // protected Rect Position => window.position;
}
