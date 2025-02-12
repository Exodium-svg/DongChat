using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;

namespace Common.Utils
{
    public enum CVarType : byte
    {
        Byte = 1,
        Char = 2,
        Int16 = 3,
        UInt16 = 4,
        Int32 = 5,
        UInt32 = 6,
        Int64 = 7,
        UInt64 = 8,
        String = 9
    }
    public readonly struct CVar
    {
        public readonly CVarType cType;
        public readonly object cData;

        public CVar(CVarType cVarType, object cVar)
        {
            this.cType = cVarType;
            this.cData = cVar;
        }
    }
    public sealed class CVars
    {
        readonly ConcurrentDictionary<string, CVar> _cVars = new();
        private void LoadVariables(byte[] cVarData)
        {
            using MemoryStream ms = new MemoryStream(cVarData);

            string signature = ms.ReadString();
            int count = ms.Read<int>();

            for (int i = 0; i < count; i++)
            {
                CVarType cVarType = (CVarType)ms.ReadByte();
                string cVarName = ms.ReadString();

                object cVar = cVarType switch
                {
                    CVarType.Byte => (byte)ms.ReadByte(),
                    CVarType.Char => (char)ms.Read<ushort>(),  // Use UInt16 since .NET chars are 2 bytes (UTF-16)
                    CVarType.Int16 => ms.Read<short>(),
                    CVarType.UInt16 => ms.Read<ushort>(),
                    CVarType.Int32 => ms.Read<int>(),
                    CVarType.UInt32 => ms.Read<uint>(),
                    CVarType.Int64 => ms.Read<long>(),
                    CVarType.UInt64 => ms.Read<ulong>(),
                    _ => throw new InvalidDataException($"Unknown CVarType: {cVarType}")
                };

                _cVars[cVarName] = new CVar(cVarType, cVar);
            }
        }

        public CVars() {}
        public CVars(ReadOnlyMemory<byte> cVarData)
        {
            if(!MemoryMarshal.TryGetArray(cVarData, out ArraySegment<byte> segment))
                segment = new ArraySegment<byte>(cVarData.ToArray());
            
            LoadVariables(segment.Array!);
        }

        public CVars(string path) => LoadVariables(File.ReadAllBytes(path));
        public void SetVar(string key, CVar cVar) => _cVars[key] = cVar;
        public string GetString(string key, string fallback)
        {
            if(_cVars.TryGetValue(key, out CVar cVar))
                return (string)cVar.cData;

            return fallback;
        }
        public T Get<T>(string key, T fallback) where T : struct
        {
            if (!_cVars.TryGetValue(key, out CVar cVar))
                return fallback;

            switch (cVar.cType)
            {
                case CVarType.Byte:
                    if (typeof(T) != typeof(byte))
                        throw new NotSupportedException($"{key} is not of type {typeof(T)}, it is of type {typeof(byte)}");
                    break;

                case CVarType.Char:
                    if (typeof(T) != typeof(char))
                        throw new NotSupportedException($"{key} is not of type {typeof(T)}, it is of type {typeof(char)}");
                    break;

                case CVarType.Int16:
                    if (typeof(T) != typeof(short))
                        throw new NotSupportedException($"{key} is not of type {typeof(T)}, it is of type {typeof(short)}");
                    break;

                case CVarType.UInt16:
                    if (typeof(T) != typeof(ushort))
                        throw new NotSupportedException($"{key} is not of type {typeof(T)}, it is of type {typeof(ushort)}");
                    break;

                case CVarType.Int32:
                    if (typeof(T) != typeof(int))
                        throw new NotSupportedException($"{key} is not of type {typeof(T)}, it is of type {typeof(int)}");
                    break;

                case CVarType.UInt32:
                    if (typeof(T) != typeof(uint))
                        throw new NotSupportedException($"{key} is not of type {typeof(T)}, it is of type {typeof(uint)}");
                    break;

                case CVarType.Int64:
                    if (typeof(T) != typeof(long))
                        throw new NotSupportedException($"{key} is not of type {typeof(T)}, it is of type {typeof(long)}");
                    break;

                case CVarType.UInt64:
                    if (typeof(T) != typeof(ulong))
                        throw new NotSupportedException($"{key} is not of type {typeof(T)}, it is of type {typeof(ulong)}");
                    break;

                default:
                    throw new NotSupportedException($"Unsupported CVarType: {cVar.cType}");
            }

            return (T)cVar.cData;
        }
        public void WriteToStream(Stream stream)
        {
            stream.WriteString("Official CVars"); // TODO: turn into a certificate that can only be read using a public key;
            stream.Write(_cVars.Count);

            foreach (KeyValuePair<string, CVar> cVarPair in _cVars)
            {
                CVar cVar = cVarPair.Value;
                stream.WriteByte((byte)cVar.cType);
                stream.WriteString(cVarPair.Key);

                switch (cVar.cType)
                {
                    case CVarType.Byte:
                        stream.Write((byte)cVar.cData);
                        break;
                    case CVarType.Char:
                        stream.Write((ushort)(char)cVar.cData); // Cast `char` to `ushort` (since .NET `char` is 2 bytes)
                        break;
                    case CVarType.Int16:
                        stream.Write((short)cVar.cData);
                        break;
                    case CVarType.UInt16:
                        stream.Write((ushort)cVar.cData);
                        break;
                    case CVarType.Int32:
                        stream.Write((int)cVar.cData);
                        break;
                    case CVarType.UInt32:
                        stream.Write((uint)cVar.cData);
                        break;
                    case CVarType.Int64:
                        stream.Write((long)cVar.cData);
                        break;
                    case CVarType.UInt64:
                        stream.Write((ulong)cVar.cData);
                        break;
                    case CVarType.String:
                        stream.WriteString((string)cVar.cData);
                        break;
                    default:
                        throw new InvalidDataException($"Unsupported CVarType: {cVar.cType}");
                }
            }
        }
        public byte[] ToArray()
        {
            using MemoryStream memoryStream = new MemoryStream();

            WriteToStream(memoryStream);
            return memoryStream.ToArray();
        }
        public void Save(string output)
        {
            using FileStream fs = File.OpenWrite(output);

            WriteToStream(fs);
            fs.Close();
        }
    }
}
