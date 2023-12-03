using UnityEditor;
using UnityEngine;

/// <summary> The abstract class for a single application in the extension </summary>
public abstract class SingleExtensionApplication : ScriptableObject
{
    /// <summary> The window that this application is displayed in. </summary>
    private EditorWindow window;

    /// <summary> The display name of a single application. </summary>
    public abstract string DisplayName { get; }

    /// <summary> The help box of a single application. </summary>
    protected static HelpBox helpBox = HelpBox.GetInstance();

    /// <summary> The default space between GUI elements. </summary>
    protected int defaultSpace = 10;

    private const string HighlightButtonRessourcePath = "HighlightButton";

    /// <summary> Initializes the application. </summary>
    /// <param name="window"> The window that this application is displayed in.</param>
    public void Initialize(EditorWindow window)
    {
        this.window = window;
    }

    /// <summary> Abstract method that renders the GUI of the application. </summary>
    public abstract void OnGUI();

    /// <summary> Abstract method that is called when the application is enabled. </summary>
    public virtual void OnEnable() { }

    /// <summary> Method that repaints the window. </summary>
    public void Repaint() => window.Repaint();

    /// <summary> Method that focuses the window. </summary>
    public void Focus() => window.Focus();

    /// <summary> Method that shows the window. </summary>
    public void Show()
    {
        window.Show();
    }

    /// <summary> Method that creates a rich text style for colored text. </summary>
    /// <returns> Returns a GUIStyle that can be used to display colored text. </returns>
    protected GUIStyle CreateRichTextStyle()
    {
        return new GUIStyle(GUI.skin.textArea) { richText = true, wordWrap = true, };
    }

    /// <summary> Method that creates a code style for displaying code. </summary>
    /// <returns> Returns a GUIStyle that can be used to display code. </returns>
    protected GUIStyle CreateCodeStyle()
    {
        return new GUIStyle(EditorStyles.textArea)
        {
            font = EditorStyles.miniFont,
            fontSize = 12,
            wordWrap = true,
            normal = { textColor = Color.white },
            hover = { textColor = Color.white },
            richText = true,
        };
    }

    /// <summary> Method that creates a highlight button style for displaying buttons. Loads the style from a ressource file (HighlightButton.guiskin) </summary>
    /// <returns> Returns a GUIStyle that can be used to display buttons. </returns>
    protected GUIStyle CreateHighlightButtonStyle()
    {
        GUISkin highightButtonSkin = Resources.Load<GUISkin>(HighlightButtonRessourcePath);
        GUIStyle defaultStyle = new(GUI.skin.button) { fontStyle = FontStyle.Bold };
        GUIStyle hightLightButtonStyle;
        if (highightButtonSkin != null && highightButtonSkin.button != null)
        {
            hightLightButtonStyle = new(highightButtonSkin.button);
        }
        else
        {
            hightLightButtonStyle = defaultStyle;
        }

        return hightLightButtonStyle;
    }

    /// <summary> Method that creates a highlight button style for displaying buttons. Loads the style from a ressource file (HighlightButton.guiskin) </summary>
    /// <returns> Returns the helpbox of the application. </returns>
    public HelpBox GetHelpBox() => helpBox;

    /// <summary> Method that renders the help box of the application. </summary>
    protected void RenderHelpBox()
    {
        if (string.IsNullOrEmpty(helpBox.HBMessage))
            return;
        EditorGUILayout.HelpBox(helpBox.HBMessage, helpBox.HBMessageType);
        helpBox.RenderProgressBar();
    }

    /// <summary> Method that renders the help box of the application. </summary>
    /// <param name="progress"> The intended progress of the progress bar. </param>
    protected void ShowProgressBar(float progress)
    {
        helpBox.UpdateIntendedProgress(progress);
        EditorApplication.update += helpBox.UpdateProgressBar;
        Repaint();
    }

    /// <summary> Method that finishes the progress bar of the help box and closes it after a delay. </summary>
    /// <param name="milliSeconds"> The delay in milliseconds. </param>
    protected void FinishProgressBarWithDelay(int milliSeconds = 1500)
    {
        helpBox.FinishProgressBarWithDelay(milliSeconds);
        EditorApplication.update -= helpBox.UpdateProgressBar;
        Repaint();
    }

    /// <summary> Method to add a default space between GUI elements. </summary>
    protected void AddDefaultSpace() => GUILayout.Space(defaultSpace);

    /// <summary> Method to Reset the keyboard control. It is used, if a user should focus out of a textfield. </summary>
    protected void ResetKeyboardControl() => GUIUtility.keyboardControl = 0;

    /// <summary> Method that closes the window. </summary>
    public void Close() => window.Close();
}
