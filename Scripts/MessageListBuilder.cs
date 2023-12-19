using System.Collections.Generic;
using UnityEngine;

/// <summary> The message list builder class that is used to build a list of AI api messages to be sent to the AI api </summary>
public class MessageListBuilder
{
    /// <summary> The list of valid roles for a message. These are the roles that the GPT api accepts at the moment. </summary>
    public static readonly List<string> validRoles =
        new() { "system", "assistant", "user", "function" };

    /// <summary> The list of messages that will be built. </summary>
    private readonly List<RequestMessage> messageList = new();

    /// <summary> Adds a message to the message list builder with the given content and role. The default role is "user". </summary>
    /// <param name="content"> The content of the message. </param>
    /// <param name="role"> The role of the message. </param>
    /// <returns> The updated message list builder. </returns>

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

    /// <summary> Adds a message to the message list builder with the given message. It uses a RequestMessage object. If the role is invalid, it will be set to "user". </summary>
    /// <param name="message"> The message to be added. </param>
    /// <returns> The updated message list builder. </returns>
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

    /// <summary>
    /// Adds a list of messages to the message list builder with the given list of messages. It uses a RequestMessage object.
    /// If the role is invalid, it will be set to "user".
    /// </summary>
    /// <param name="addMessageList"> The list of messages to be added.</param>
    /// <returns> The updated message list builder. </returns>
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

    /// <summary> Returns the message at the given index. If the index is out of range, it will return a message with empty content and role "system". </summary>
    /// <param name="index"> The index of the message to be returned. </param>
    /// <returns> The message at the given index. </returns>
    public RequestMessage GetMessageAt(int index)
    {
        if (index < messageList.Count)
        {
            return messageList[index];
        }
        else
            return new RequestMessage("", "system");
    }

    /// <summary> Returns the number of messages in the message list. </summary>
    /// <returns> The number of messages in the message list. </returns>
    public int GetMessageCount()
    {
        return messageList.Count;
    }

    /// <summary> Removes the messages with the given content from the message list. </summary>
    /// <param name="content"> The content of the message to be removed. </param>
    /// <returns> The updated message list builder. </returns>
    public MessageListBuilder RemoveMessage(string content)
    {
        messageList.RemoveAll(message => message.content == content);
        return this;
    }

    /// <summary> Removes the message at the given index from the message list. </summary>
    /// <param name="index"> The index of the message to be removed. </param>
    /// <returns> The updated message list builder. </returns>
    public MessageListBuilder RemoveMessageAt(int index)
    {
        messageList.RemoveAt(index);
        return this;
    }

    /// <summary> Clears the message list. </summary>
    /// <returns> The updated message list builder. </returns>
    public MessageListBuilder ClearMessages()
    {
        messageList.Clear();
        return this;
    }

    /// <summary> Builds the message list. </summary>
    /// <returns> The built message list. </returns>
    public List<RequestMessage> Build()
    {
        return messageList;
    }
}
