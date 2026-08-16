namespace LabFusion.Network;

/// <summary>
/// Manages the reading of received messages.
/// </summary>
public static class MessageManager
{
    /// <summary>
    /// Reads a message that was received on the server and sent from a client.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="sender"></param>
    public static void ReadMessageOnServer(ReadOnlySpan<byte> buffer, ClientPlatformID sender) => ReadMessage(new ReadableMessage()
    {
        Buffer = buffer,
        IsServerHandled = true,
        SenderPlatformID = sender,
    });

    /// <summary>
    /// Reads a message that was received on the client and sent from the server.
    /// </summary>
    /// <param name="buffer"></param>
    public static void ReadMessageOnClient(ReadOnlySpan<byte> buffer) => ReadMessage(new ReadableMessage()
    {
        Buffer = buffer,
        IsServerHandled = false,
        SenderPlatformID = null,
    });

    /// <summary>
    /// Reads a readable message.
    /// <para>
    /// It is not recommended to construct a readable message yourself unless you know what you are doing. 
    /// Instead, you should use <see cref="ReadMessageOnServer(ReadOnlySpan{byte}, ClientPlatformID)"/> when reading on the server
    /// or <see cref="ReadMessageOnClient(ReadOnlySpan{byte})"/> when reading on a client.
    /// </para>
    /// </summary>
    /// <param name="message"></param>
    public static void ReadMessage(ReadableMessage message) => NativeMessageHandler.ReadMessage(message);
}