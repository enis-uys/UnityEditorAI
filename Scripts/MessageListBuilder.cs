using System.Collections.Generic;
using UnityEngine;

public class MessageListBuilder
{
    public static readonly List<string> validRoles =
        new() { "system", "assistant", "user", "function" };

    [System.Serializable]
    public class RequestMessage
    {
        public string content;
        public string role;

        public RequestMessage(string content, string role = "user")
        {
            this.content = content;
            this.role = role;
        }
    }

    private readonly List<RequestMessage> messageList = new();

    public MessageListBuilder AddMessage(string content, string role = "user")
    {
        // Check if the message's role is a valid role; if not, set it to "user"
        if (!validRoles.Contains(role))
        {
            Debug.LogWarning("Invalid role detected in message. Setting role to 'user'.");
            role = "user";
        }

        messageList.Add(new RequestMessage(content, role));
        return this;
    }

    public MessageListBuilder AddMessage(RequestMessage message)
    {
        // Check if the message's role is a valid role; if not, set it to "user"
        if (!validRoles.Contains(message.role))
        {
            Debug.LogWarning("Invalid role detected in message. Setting role to 'user'.");
            message.role = "user";
        }
        messageList.Add(message);
        return this;
    }

    public MessageListBuilder AddMessages(List<RequestMessage> addMessageList)
    {
        foreach (var message in addMessageList)
        {
            // Check if the message's role is a valid role; if not, set it to "user"
            if (!validRoles.Contains(message.role))
            {
                Debug.LogWarning("Invalid role detected in message. Setting role to 'user'.");
                message.role = "user";
            }
        }
        messageList.AddRange(addMessageList);
        return this;
    }

    public RequestMessage GetMessageAt(int index)
    {
        return messageList[index];
    }

    public int GetMessageCount()
    {
        return messageList.Count;
    }

    public MessageListBuilder RemoveMessage(string content)
    {
        messageList.RemoveAll(message => message.content == content);
        return this;
    }

    public MessageListBuilder RemoveMessageAt(int index)
    {
        messageList.RemoveAt(index);
        return this;
    }

    public MessageListBuilder ClearMessages()
    {
        messageList.Clear();
        return this;
    }

    public List<RequestMessage> Build()
    {
        return messageList;
    }
}
