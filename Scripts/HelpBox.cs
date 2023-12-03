using UnityEditor;
using UnityEngine;
using System;
using System.Threading.Tasks;

/// <summary> The help box class that is used to display messages and progress bars to the user </summary>
public class HelpBox
{
    /// <summary> The time of the last function call. </summary>
    private DateTime lastFunctionCallTime;

    /// <summary> The threshold in milliseconds for the timer. </summary>
    private readonly int timerThresholdInMilliseconds = 600;

    /// <summary> The current progress of the progress bar. </summary>
    private float pogressBarProgress = 0f;

    /// <summary> The intended progress of the progress bar. This is the progress, the progressbar will steadily move towards. </summary>
    private float intendedProgress = 1f;

    /// <summary> Whether the progress bar is active. </summary>
    private bool isProgressBarActive = false;

    /// <summary> The delay in milliseconds for the progress bar to disappear. </summary>
    private readonly int progressBarDelayInMilliseconds = 1500;

    /// <summary> The instance of the help box. </summary>
    private static HelpBox instance;

    /// <summary> The singleton constructor of the help box. </summary>
    /// <returns> Returns the instance of the help box. </returns>
    public static HelpBox GetInstance()
    {
        instance ??= new HelpBox();
        return instance;
    }

    /// <summary> The message type of the help box. </summary>
    public MessageType HBMessageType { get; set; }

    /// <summary> The message of the help box. </summary>
    public string HBMessage { get; set; }

    /// <summary> The progress of the progress bar. </summary>
    public float IntendedProgress { get; set; }

    /// <summary> The delay in milliseconds for the progress bar to disappear. </summary>
    public int ProgressBarDelayInMilliseconds => progressBarDelayInMilliseconds;

    /// <summary> Updates the time of the last function call. </summary>
    private void UpdateLastFunctionCalltime()
    {
        lastFunctionCallTime = DateTime.Now;
    }

    /// <summary> Returns the time since the last function call in milliseconds. </summary>
    /// <returns> Returns the time since the last function call in milliseconds. </returns>
    private double TimeSinceLastFunctionInMilliseconds()
    {
        if (lastFunctionCallTime == null)
        {
            return timerThresholdInMilliseconds;
        }
        else
        {
            TimeSpan elapsed = DateTime.Now - lastFunctionCallTime;
            return elapsed.TotalMilliseconds;
        }
    }

    /// <summary> Returns the delay in milliseconds for the timer. This is used to show messages for a certain amount of time. </summary>
    /// <returns> Returns the delay in milliseconds for the timer. </returns>
    private int TimerDelayInMilliseconds()
    {
        return Mathf.RoundToInt(
            Mathf.Max(
                0f,
                timerThresholdInMilliseconds - (float)TimeSinceLastFunctionInMilliseconds()
            )
        );
    }

    /// <summary> Updates the message of the help box. </summary>
    /// <param name="message"> The message to display. </param>
    /// <param name="UpdateMessageType"> The message type of the message. All parameters including this and the following are optional. </param>
    /// <param name="shouldAppend"> Whether the message should be appended to the current message. Default is false. </param>
    /// <param name="debugMessage"> Whether the message should also be logged to the console. Default is false. </param>
    public void UpdateMessage(
        string message,
        MessageType? UpdateMessageType,
        bool shouldAppend = false,
        bool debugMessage = false
    )
    {
        int timerDelayInMilliseconds = TimerDelayInMilliseconds();
        if (UpdateMessageType.HasValue && UpdateMessageType.Value == MessageType.Error)
        {
            timerDelayInMilliseconds = 0;
        }

        Task.Delay(timerDelayInMilliseconds)
            .ContinueWith(_ =>
            {
                if (UpdateMessageType.HasValue)
                {
                    HBMessageType = UpdateMessageType.Value;
                }
                if (debugMessage)
                {
                    DebugMessageIfShould(message, UpdateMessageType ?? HBMessageType);
                }
                if (shouldAppend)
                {
                    HBMessage += "\n" + message;
                }
                else
                {
                    HBMessage = message;
                }

                UpdateLastFunctionCalltime();
            });
    }

    /// <summary> Removes the message of the help box after a certain amount of time. </summary>
    /// <param name="milliSeconds"> The amount of time in milliseconds to wait before removing the message. </param>
    public void RemoveMessage(int milliSeconds)
    {
        Task.Delay(milliSeconds)
            .ContinueWith(_ =>
            {
                HBMessage = "";
                HBMessageType = MessageType.None;
            });
    }

    /// <summary> Sets the progress bar progress. It will be displayed if it is not already active. </summary>
    /// <param name="progress"> The progress to set the progress bar to. </param>
    public void SetProgressBarProgress(float progress)
    {
        pogressBarProgress = progress;
        if (!isProgressBarActive)
        {
            SetProgressBarActive(true);
        }
        else if (progress >= 1.0f)
        {
            FinishProgressBarWithDelay(ProgressBarDelayInMilliseconds);
        }
    }

    /// <summary> Sets the progress bar active or inactive. </summary>
    /// <param name="isActive"> Whether the progress bar should be active. </param>
    public void SetProgressBarActive(bool isActive)
    {
        isProgressBarActive = isActive;
    }

    /// <summary> Updates the intended progress of the progress bar. </summary>
    /// <param name="progress"> The intended progress of the progress bar. </param>
    public void UpdateIntendedProgress(float progress)
    {
        intendedProgress = Mathf.Clamp01(progress);
    }

    /// <summary> Finishes the progress bar after a certain amount of time. </summary>
    /// <param name="delayInMilliseconds"> The amount of time in milliseconds to wait before finishing the progress bar. </param>
    public void FinishProgressBarWithDelay(int delayInMilliseconds)
    {
        Task.Delay(delayInMilliseconds / 2)
            .ContinueWith(_ =>
            {
                pogressBarProgress = 1.0f;
                Task.Delay(delayInMilliseconds)
                    .ContinueWith(_ =>
                    {
                        SetProgressBarActive(false);
                        pogressBarProgress = 0.0f;
                    });
            });
    }

    /// <summary> Renders the progress bar. </summary>
    public void RenderProgressBar()
    {
        if (isProgressBarActive)
        {
            EditorGUILayout.Space();
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), pogressBarProgress, "Progress");
            EditorGUILayout.Space();
        }
    }

    /// <summary> Updates the progress bar. </summary>
    public void UpdateProgressBar()
    {
        if (!isProgressBarActive)
        {
            SetProgressBarActive(true);
        }
        // Simulate progress by incrementing the value.
        pogressBarProgress += 0.002f;
        if (pogressBarProgress >= intendedProgress)
        {
            //Remove from update event
            EditorApplication.update -= UpdateProgressBar;
        }
        if (pogressBarProgress >= 1.0f)
        {
            FinishProgressBarWithDelay(ProgressBarDelayInMilliseconds);
            intendedProgress = 1.0f;
        }
    }

    /// <summary> Method that debugs a message if the shouldDebugLog parameter is true. </summary>
    /// <param name="customMessage"> The message to log. </param>
    /// <param name="messageTypeToLog"> The message type of the message. </param>
    public void DebugMessageIfShould(string customMessage, MessageType messageTypeToLog)
    {
        if (messageTypeToLog == MessageType.Error)
        {
            Debug.LogError(customMessage);
        }
        else if (messageTypeToLog == MessageType.Warning)
        {
            Debug.LogWarning(customMessage);
        }
        else
        {
            Debug.Log(customMessage);
        }
    }
}
