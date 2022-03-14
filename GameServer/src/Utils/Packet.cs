using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GameServer {
    public enum ServerPacket {
        Welcome = 1,

        PlayerSpawn,
        PlayerPosition,
        PlayerRotation,

        PlayerChatReceived,
        ServerMessage,
    }

    public enum ClientPacket {
        WelcomeReceived = 1,

        PlayerMovement,
        PlayerChatMessage,
    }

    public class Packet : IDisposable {
        private List<byte> _buffer = new();
        private byte[] _readableBuffer;
        private int _readPos = 0;

        public Packet() {}

        public Packet(int id) {
            Write(id);
        }

        public Packet(ServerPacket id) {
            Write((int)id);
        }

        public Packet(ClientPacket id) {
            Write((int)id);
        }

        public Packet(byte[] data) {
            SetBytes(data);
        }


        public void SetBytes(byte[] data) {
            Write(data);
            _readableBuffer = _buffer.ToArray();
        }

        public void WriteLength() {
            _buffer.InsertRange(0, BitConverter.GetBytes(_buffer.Count));
        }

        public void InsertInt(int value) {
            _buffer.InsertRange(0, BitConverter.GetBytes(value));
        }

        public byte[] Buffer       { get { return _readableBuffer = _buffer.ToArray(); } }
        public int    Length       { get { return _buffer.Count; } }
        public int    UnreadLength { get { return Length - _readPos; } }

        public void Reset(bool shouldReset = true) {
            if(!shouldReset) {
                _readPos -= 4;
                return;
            }

            _buffer.Clear();
            _readableBuffer = null;
            _readPos = 0;
        }

        public void Write(byte value)   { _buffer.Add(value); }
        public void Write(byte[] value) { _buffer.AddRange(value); }
        public void Write(short value)  { _buffer.AddRange(BitConverter.GetBytes(value)); }
        public void Write(bool value)   { _buffer.AddRange(BitConverter.GetBytes(value)); }
        public void Write(int value)    { _buffer.AddRange(BitConverter.GetBytes(value)); }
        public void Write(long value)   { _buffer.AddRange(BitConverter.GetBytes(value)); }
        public void Write(float value)  { _buffer.AddRange(BitConverter.GetBytes(value)); }
        public void Write(string value) { Write(value.Length); _buffer.AddRange(Encoding.ASCII.GetBytes(value)); }

        public void Write(Vector2 value)    { Write(value.X); Write(value.Y); }
        public void Write(Vector3 value)    { Write(value.X); Write(value.Y); Write(value.Z); }
        public void Write(Vector4 value)    { Write(value.X); Write(value.Y); Write(value.Z); Write(value.W); }
        public void Write(Quaternion value) { Write(value.X); Write(value.Y); Write(value.Z); Write(value.W); }


        public byte ReadByte(bool moveReadPos = true) {
            if(_buffer.Count <= _readPos) {
                throw new Exception($"Could not read value at read position {_readPos} as type \"byte\"!");
            }

            byte value = _readableBuffer[_readPos];
            if(moveReadPos) _readPos += sizeof(byte);
            return value;
        }

        public byte[] ReadBytes(int length, bool moveReadPos = true) {
            if(_buffer.Count <= _readPos) {
                throw new Exception($"Could not read value at read position {_readPos} as type \"byte[]\"!");
            }

            byte[] value = _buffer.GetRange(_readPos, length).ToArray();
            if(moveReadPos) _readPos += length;
            return value;
        }

        public bool ReadBool(bool moveReadPos = true) {
            if(_buffer.Count <= _readPos) {
                throw new Exception($"Could not read value at read position {_readPos} as type \"bool\"!");
            }

            bool value = BitConverter.ToBoolean(_readableBuffer, _readPos);
            if(moveReadPos) _readPos += sizeof(bool);
            return value;
        }

        public short ReadShort(bool moveReadPos = true) {
            if(_buffer.Count <= _readPos) {
                throw new Exception($"Could not read value at read position {_readPos} as type \"short\"!");
            }

            short value = BitConverter.ToInt16(_readableBuffer, _readPos);
            if(moveReadPos) _readPos += sizeof(short);
            return value;
        }

        public int ReadInt(bool moveReadPos = true) {
            if(_buffer.Count <= _readPos) {
                throw new Exception($"Could not read value at read position {_readPos} as type \"int\"!");
            }

            int value = BitConverter.ToInt32(_readableBuffer, _readPos);
            if(moveReadPos) _readPos += sizeof(int);
            return value;
        }

        public long ReadLong(bool moveReadPos = true) {
            if(_buffer.Count <= _readPos) {
                throw new Exception($"Could not read value at read position {_readPos} as type \"long\"!");
            }

            long value = BitConverter.ToInt64(_readableBuffer, _readPos);
            if(moveReadPos) _readPos += sizeof(long);
            return value;
        }

        public float ReadFloat(bool moveReadPos = true) {
            if(_buffer.Count <= _readPos) {
                throw new Exception($"Could not read value at read position {_readPos} as type \"float\"!");
            }

            float value = BitConverter.ToSingle(_readableBuffer, _readPos);
            if(moveReadPos) _readPos += sizeof(float);
            return value;
        }

        public string ReadString(bool moveReadPos = true) {
            try {
                int len = ReadInt();
                string value = Encoding.ASCII.GetString(_readableBuffer, _readPos, len);
                if(moveReadPos && value.Length > 0) _readPos += len;
                return value;
            } catch {
                throw new Exception($"Could not read value at read position {_readPos} as type \"string\"!");
            }
        }

        public Vector2 ReadVector2(bool moveReadPos = true) {
            var x = ReadFloat(moveReadPos);
            var y = ReadFloat(moveReadPos);
            return new Vector2(x, y);
        }

        public Vector3 ReadVector3(bool moveReadPos = true) {
            var x = ReadFloat(moveReadPos);
            var y = ReadFloat(moveReadPos);
            var z = ReadFloat(moveReadPos);
            return new Vector3(x, y, z);
        }

        public Vector4 ReadVector4(bool moveReadPos = true) {
            var x = ReadFloat(moveReadPos);
            var y = ReadFloat(moveReadPos);
            var z = ReadFloat(moveReadPos);
            var w = ReadFloat(moveReadPos);
            return new Vector4(x, y, z, w);
        }

        public Quaternion ReadQuaternion(bool moveReadPos = true) {
            var x = ReadFloat(moveReadPos);
            var y = ReadFloat(moveReadPos);
            var z = ReadFloat(moveReadPos);
            var w = ReadFloat(moveReadPos);
            return new Quaternion(x, y, z, w);
        }

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing) {
            if(_disposed) {
                return;
            }

            if(disposing) {
                _buffer = null;
                _readableBuffer = null;
                _readPos = 0;
            }
            
            _disposed = true;
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}