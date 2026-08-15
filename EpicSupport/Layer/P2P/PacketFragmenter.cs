using System.Buffers;
using Epic.OnlineServices;
using Epic.OnlineServices.P2P;

// Edited but not original
// https://github.com/TrentSterling/fishnet-eos-native/blob/master/Assets/FishNet.Transport.EOSNative/PacketFragmenter.cs

namespace MarrowFusion.Epic;

internal class PacketFragmenter
{
    internal const int HeaderSize = sizeof(uint) + sizeof(ushort) + sizeof(byte);
    internal const int MaxPacketSize = P2PInterface.MAX_PACKET_SIZE;
    internal const int MaxPayloadSize = MaxPacketSize - HeaderSize;

    private uint _nextPacketId;
    private readonly Dictionary<FragmentKey, List<FragmentData>> _pendingFragments = new();
    
    private readonly byte[] _sendBuffer = new byte[MaxPacketSize];
    
    internal FragmentEnumerator Fragment(ArraySegment<byte> data)
    {
        return new FragmentEnumerator(this, data, _nextPacketId++);
    }

    internal struct FragmentEnumerator
    {
        private readonly PacketFragmenter _fragmenter;
        private readonly ArraySegment<byte> _data;
        private readonly uint _packetId;
        private readonly int _totalFragments;
        private int _index;

        internal FragmentEnumerator(PacketFragmenter fragmenter, ArraySegment<byte> data, uint packetId)
        {
            _fragmenter = fragmenter;
            _data = data;
            _packetId = packetId;
            _totalFragments = data.Count == 0 ? 1 : (data.Count + MaxPayloadSize - 1) / MaxPayloadSize;
            _index = -1;
            Current = default;
        }
        
        public FragmentEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            _index++;
            if (_index >= _totalFragments)
            {
                return false;
            }

            var buffer = _fragmenter._sendBuffer;

            if (_data.Count == 0)
            {
                WriteHeader(buffer, _packetId, 0, true);
                Current = new ArraySegment<byte>(buffer, 0, HeaderSize);
                return true;
            }

            int offset = _index * MaxPayloadSize;
            int remaining = _data.Count - offset;
            int payloadLength = Math.Min(MaxPayloadSize, remaining);
            bool isLast = _index == _totalFragments - 1;

            WriteHeader(buffer, _packetId, (ushort)_index, isLast);
            Array.Copy(_data.Array, _data.Offset + offset, buffer, HeaderSize, payloadLength);

            Current = new ArraySegment<byte>(buffer, 0, HeaderSize + payloadLength);
            return true;
        }

        public ArraySegment<byte> Current { get; private set; }
    }
    
    internal static bool NeedsFragmentation(int dataLength)
    {
        return dataLength > MaxPayloadSize;
    }

    private static void WriteHeader(byte[] buffer, uint packetId, ushort fragmentId, bool lastFragment)
    {
        // packetId (little-endian)
        buffer[0] = (byte)packetId;
        buffer[1] = (byte)(packetId >> 8);
        buffer[2] = (byte)(packetId >> 16);
        buffer[3] = (byte)(packetId >> 24);

        // fragmentId (little-endian)
        buffer[4] = (byte)fragmentId;
        buffer[5] = (byte)(fragmentId >> 8);

        // lastFragment
        buffer[6] = lastFragment ? (byte)1 : (byte)0;
    }
    
    internal bool ProcessIncoming(ProductUserId senderId, ArraySegment<byte> data, byte channel, out ArraySegment<byte> payload)
    {
        payload = default;

        if (data.Count < HeaderSize)
        {
            EpicModule.Logger.Warn($"Received packet too small for header: {data.Count} bytes (need {HeaderSize})");
            return false;
        }
        
        uint packetId = BitConverter.ToUInt32(data.Array, data.Offset);
        ushort fragmentId = BitConverter.ToUInt16(data.Array, data.Offset + 4);
        bool lastFragment = data.Array[data.Offset + 6] == 1;

        int payloadLength = data.Count - HeaderSize;
        
        if (fragmentId == 0 && lastFragment)
        {
            payload = new ArraySegment<byte>(data.Array, data.Offset + HeaderSize, payloadLength);
            return true;
        }
        
        byte[] payloadCopy = ArrayPool<byte>.Shared.Rent(payloadLength);
        if (payloadLength > 0)
        {
            Array.Copy(data.Array, data.Offset + HeaderSize, payloadCopy, 0, payloadLength);
        }

        var key = new FragmentKey(senderId, packetId, channel);

        if (!_pendingFragments.TryGetValue(key, out var fragments))
        {
            fragments = new List<FragmentData>();
            _pendingFragments[key] = fragments;
        }

        fragments.Add(new FragmentData(fragmentId, lastFragment, payloadCopy, payloadLength));
        
        byte[] reassembled = TryReassemble(key, fragments);
        if (reassembled == null)
        {
            return false;
        }

        payload = new ArraySegment<byte>(reassembled);
        return true;
    }

    private byte[] TryReassemble(FragmentKey key, List<FragmentData> fragments)
    {
        int expectedCount = -1;
        foreach (var frag in fragments)
        {
            if (frag.IsLast)
            {
                expectedCount = frag.Id + 1;
                break;
            }
        }

        if (expectedCount == -1)
        {
            return null;
        }

        if (fragments.Count != expectedCount)
        {
            return null;
        }
        
        bool[] seen = new bool[expectedCount];
        foreach (var frag in fragments)
        {
            if (frag.Id >= expectedCount)
            {
                ReturnFragmentBuffers(fragments);
                _pendingFragments.Remove(key);
                return null;
            }
            seen[frag.Id] = true;
        }

        for (int i = 0; i < expectedCount; i++)
        {
            if (!seen[i])
            {
                return null;
            }
        }
        
        fragments.Sort((a, b) => a.Id.CompareTo(b.Id));
        
        int totalSize = 0;
        foreach (var frag in fragments)
        {
            totalSize += frag.Length;
        }
        
        byte[] result = new byte[totalSize];
        int offset = 0;
        foreach (var frag in fragments)
        {
            Array.Copy(frag.Payload, 0, result, offset, frag.Length);
            offset += frag.Length;
            ArrayPool<byte>.Shared.Return(frag.Payload);
        }
        
        _pendingFragments.Remove(key);

        return result;
    }
    
    internal void ClearPendingForSender(ProductUserId senderId)
    {
        var keysToRemove = new List<FragmentKey>();
        foreach (var (key, fragments) in _pendingFragments)
        {
            if (key.SenderId == senderId)
            {
                keysToRemove.Add(key);
                ReturnFragmentBuffers(fragments);
            }
        }
        foreach (var key in keysToRemove)
        {
            _pendingFragments.Remove(key);
        }
    }
    
    internal void ClearAll()
    {
        foreach (var fragments in _pendingFragments.Values)
        {
            ReturnFragmentBuffers(fragments);
        }
        _pendingFragments.Clear();
    }

    private static void ReturnFragmentBuffers(List<FragmentData> fragments)
    {
        foreach (var frag in fragments)
        {
            ArrayPool<byte>.Shared.Return(frag.Payload);
        }
    }

    private readonly struct FragmentKey : IEquatable<FragmentKey>
    {
        internal readonly ProductUserId SenderId;
        internal readonly uint PacketId;
        internal readonly byte Channel;

        internal FragmentKey(ProductUserId senderId, uint packetId, byte channel)
        {
            SenderId = senderId;
            PacketId = packetId;
            Channel = channel;
        }

        public bool Equals(FragmentKey other)
        {
            return SenderId == other.SenderId && PacketId == other.PacketId && Channel == other.Channel;
        }

        public override bool Equals(object obj)
        {
            return obj is FragmentKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(SenderId, PacketId, Channel);
        }
    }

    private readonly struct FragmentData
    {
        internal readonly ushort Id;
        internal readonly bool IsLast;
        internal readonly byte[] Payload;
        internal readonly int Length;

        internal FragmentData(ushort id, bool isLast, byte[] payload, int length)
        {
            Id = id;
            IsLast = isLast;
            Payload = payload;
            Length = length;
        }
    }
}