using System;
using UnityEditor;
using UnityEngine;

public abstract class SingleExtensionApplication : ScriptableObject
{
    private EditorWindow window;
    public abstract string DisplayName { get; }
    protected HelpBox helpBox = HelpBox.GetInstance();
    protected int defaultSpace = 10;

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

    public HelpBox GetHelpBox()
    {
        return helpBox;
    }

    public void ShowProgressBar(float progress)
    {
        helpBox.UpdateIntendedProgress(progress);
        EditorApplication.update += helpBox.UpdateProgressBar;
        Repaint();
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
