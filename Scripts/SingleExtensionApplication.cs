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
    private protected GUIStyle richTextStyle;

    public void Initialize(EditorWindow window)
    {
        this.window = window;
    }

    public abstract void OnGUI();

    public virtual void Reload() { }

    public void Repaint()
    {
        window.Repaint();
    }

    public void Focus()
    {
        window.Focus();
    }

    public void Show()
    {
        window.Show();
    }

    protected void InitializeRichTextStyle()
    {
        richTextStyle ??= new GUIStyle(GUI.skin.textArea) { richText = true };
    }

    public HelpBox GetHelpBox()
    {
        return helpBox;
    }

    protected void RenderHelpBox()
    {
        EditorGUILayout.HelpBox(helpBox.HBMessage, helpBox.HBMessageType);
        helpBox.RenderProgressBar();
    }

    protected void ShowProgressBar(float progress)
    {
        helpBox.UpdateIntendedProgress(progress);
        EditorApplication.update += helpBox.UpdateProgressBar;
        Repaint();
    }

    protected void AddDefaultSpace()
    {
        GUILayout.Space(defaultSpace);
    }

    protected void ResetKeyboardControl()
    {
        GUIUtility.keyboardControl = 0;
    }

    public void Close()
    {
        window.Close();
    }

    public virtual void OnEnable()
    {
        Reload();
    }

    public virtual void OnDisable() { }

    protected Rect position => window.position;
}
