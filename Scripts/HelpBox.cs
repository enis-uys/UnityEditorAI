using UnityEditor;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class HelpBox
{
    private static HelpBox instance;

    private MessageType hbMessageType;

    private string hbMessage;

    private DateTime lastFunctionCallTime;
    private int timerThresholdInMilliseconds = 600;
    private bool shouldDebugLog = false;
    private float pogressBarProgress = 0f;
    private float intendedProgress = 1f;
    private bool isProgressBarActive = false;
    private readonly int progressBarDelayInMilliseconds = 1500;

    public static HelpBox GetInstance()
    {
        if (instance == null)
        {
            instance = new HelpBox();
        }
        return instance;
    }

    public MessageType HBMessageType
    {
        get => hbMessageType;
        set => hbMessageType = value;
    }

    public string HBMessage
    {
        get => hbMessage;
        set => hbMessage = value;
    }
    public bool ShouldDebugLog
    {
        get => shouldDebugLog;
        set => shouldDebugLog = value;
    }
    public float IntendedProgress
    {
        get => intendedProgress;
        set => intendedProgress = value;
    }

    public int ProgressBarDelayInMilliseconds => progressBarDelayInMilliseconds;

    private void UpdateLastFunctionCalltime()
    {
        lastFunctionCallTime = DateTime.Now;
    }

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

    private int TimerDelayInMilliseconds()
    {
        return Mathf.RoundToInt(
            Mathf.Max(
                0f,
                timerThresholdInMilliseconds - (float)TimeSinceLastFunctionInMilliseconds()
            )
        );
    }

    public void UpdateMessage(string message)
    {
        UpdateMessageInternal(message, null);
    }

    public void UpdateMessageAndType(string message, MessageType messageType)
    {
        UpdateMessageInternal(message, messageType);
    }

    private void UpdateMessageInternal(string message, MessageType? messageType)
    {
        int timerDelayInMilliseconds = TimerDelayInMilliseconds();

        Task.Delay(timerDelayInMilliseconds)
            .ContinueWith(_ =>
            {
                HBMessage = message;
                if (messageType.HasValue)
                {
                    HBMessageType = messageType.Value;
                }
                DebugMessageIfShould();
                UpdateLastFunctionCalltime();
            });
    }

    public void AppendMessage(string message)
    {
        int timerDelayInMilliseconds = TimerDelayInMilliseconds();

        Task.Delay(timerDelayInMilliseconds)
            .ContinueWith(_ =>
            {
                HBMessage += "\n" + message;
                DebugMessageIfShould();
                UpdateLastFunctionCalltime();
            });
    }

    public void AppendMessageAndType(string message, MessageType messageType)
    {
        int timerDelayInMilliseconds = TimerDelayInMilliseconds();

        Task.Delay(timerDelayInMilliseconds)
            .ContinueWith(_ =>
            {
                HBMessage += "\n" + message;
                HBMessageType = messageType;
                DebugMessageIfShould();
                UpdateLastFunctionCalltime();
            });
    }

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

    public void SetProgressBarActive(bool isActive)
    {
        isProgressBarActive = isActive;
    }

    public void UpdateIntendedProgress(float progress)
    {
        intendedProgress = Mathf.Clamp01(progress);
    }

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

    public void RenderProgressBar()
    {
        if (isProgressBarActive)
        {
            EditorGUILayout.Space();
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), pogressBarProgress, "Progress");
            EditorGUILayout.Space();
        }
    }

    public void UpdateProgressBar()
    {
        if (!isProgressBarActive)
        {
            SetProgressBarActive(true);
        }
        pogressBarProgress += 0.01f; // Simulate progress by incrementing the value.
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

    public void DebugMessageIfShould()
    {
        if (ShouldDebugLog)
        {
            Debug.Log(HBMessage);
        }
    }
}
