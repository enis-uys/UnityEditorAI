using UnityEditor;
using UnityEngine;
using System;
using System.Threading.Tasks;

using System.Timers;

public class HelpBox
{
    private static HelpBox instance;

    private MessageType helpBoxMessageType;

    private string helpBoxMessage;

    private DateTime lastFunctionCallTime;
    private int timerThresholdInMilliseconds = 1000;

    public static HelpBox GetInstance()
    {
        if (instance == null)
        {
            instance = new HelpBox();
        }
        return instance;
    }

    public MessageType HelpBoxMessageType
    {
        get => helpBoxMessageType;
        set => helpBoxMessageType = value;
    }

    public string HelpBoxMessage
    {
        get => helpBoxMessage;
        set => helpBoxMessage = value;
    }

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

    public void UpdateHelpBoxMessageAndType(string message, MessageType messageType)
    {
        int timerDelayInMilliseconds = TimerDelayInMilliseconds();

        Task.Delay(timerDelayInMilliseconds)
            .ContinueWith(_ =>
            {
                HelpBoxMessage = message;
                HelpBoxMessageType = messageType;
                UpdateLastFunctionCalltime();
            });
    }

    public void UpdateHelpBoxMessage(string message)
    {
        int timerDelayInMilliseconds = TimerDelayInMilliseconds();

        Task.Delay(timerDelayInMilliseconds)
            .ContinueWith(_ =>
            {
                HelpBoxMessage = message;
                UpdateLastFunctionCalltime();
            });
    }

    public void AppendHelpBoxMessage(string message)
    {
        int timerDelayInMilliseconds = TimerDelayInMilliseconds();

        Task.Delay(timerDelayInMilliseconds)
            .ContinueWith(_ =>
            {
                HelpBoxMessage += "\n" + message;
                UpdateLastFunctionCalltime();
            });
    }

    public void AppendHelpBoxMessageAndType(string message, MessageType messageType)
    {
        int timerDelayInMilliseconds = TimerDelayInMilliseconds();

        Task.Delay(timerDelayInMilliseconds)
            .ContinueWith(_ =>
            {
                HelpBoxMessage += "\n" + message;
                HelpBoxMessageType = messageType;
                UpdateLastFunctionCalltime();
            });
    }
}
