using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Geode.Extension;
using Geode.Habbo.Messages;
using Geode.Network;
using Geode.Network.Protocol;

namespace LTDHelper;

/// <summary>
/// G-Earth extension for LTDHelper, built on GService (G-Earth-Geode 1.4.1-beta).
/// </summary>
[Module("LTDHelper", "Bastiian787", "Limited Auto-Buyer for Habbo.")]
public class LTDExtension : GService
{
    private readonly Dictionary<ushort, TaskCompletionSource<DataInterceptedEventArgs?>> _waiters = new();
    private readonly object _waitersLock = new object();

    /// <summary>Raised on each intercepted packet (incoming and outgoing).</summary>
    public event Action<DataInterceptedEventArgs>? OnDataIntercepted;

    /// <summary>Raised when the G-Earth connection reports a critical error.</summary>
    public event Action<string>? OnCriticalErrorEvent;

    /// <summary>Raised once the extension is connected to G-Earth and messages are resolved.</summary>
    public event Action? OnConnectedEvent;

    public override void OnConnected(HPacket packet)
    {
        base.OnConnected(packet);
        OnConnectedEvent?.Invoke();
    }

    public override void OnDataIntercept(DataInterceptedEventArgs data)
    {
        base.OnDataIntercept(data);

        // Signal any pending WaitForPacketAsync waiters.
        if (!data.IsOutgoing)
        {
            TaskCompletionSource<DataInterceptedEventArgs?>? tcs = null;
            lock (_waitersLock)
            {
                if (_waiters.TryGetValue(data.Packet.Id, out tcs))
                    _waiters.Remove(data.Packet.Id);
            }

            if (tcs != null)
            {
                data.Packet.Position = 0;
                tcs.TrySetResult(data);
            }
        }

        OnDataIntercepted?.Invoke(data);
    }

    public override void OnCriticalError(string error)
    {
        OnCriticalErrorEvent?.Invoke(error);
    }

    /// <summary>
    /// Waits for the next incoming packet whose header ID matches <paramref name="message"/>.
    /// Returns <c>null</c> if the timeout expires before the packet arrives.
    /// Note: only one waiter per message ID is supported at a time.
    /// </summary>
    public async Task<DataInterceptedEventArgs?> WaitForPacketAsync(HMessage message, int timeoutMs)
    {
        var tcs = new TaskCompletionSource<DataInterceptedEventArgs?>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        lock (_waitersLock)
        {
            _waiters[message.Id] = tcs;
        }

        var timeoutTask = Task.Delay(timeoutMs);
        var completed = await Task.WhenAny(tcs.Task, timeoutTask).ConfigureAwait(false);

        if (completed == timeoutTask)
        {
            // Timed out — remove the waiter so OnDataIntercept does not signal it later.
            lock (_waitersLock)
            {
                if (_waiters.TryGetValue(message.Id, out var existing) && existing == tcs)
                    _waiters.Remove(message.Id);
            }
            return null;
        }

        return await tcs.Task.ConfigureAwait(false);
    }
}
