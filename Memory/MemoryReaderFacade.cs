using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NOBApp.Memory
{
    public sealed class MemoryReaderFacade : IDisposable
    {
        private readonly Win32ProcessMemoryReader _reader;
        private readonly nuint _moduleBase;

        public MemoryReaderFacade(int processId, nuint moduleBase)
        {
            _reader = new Win32ProcessMemoryReader(processId);
            _moduleBase = moduleBase;
        }

        public bool IsWin32Available => _reader.IsOpen;

        public Task<int> ReadInt32Async(string address, CancellationToken cancellationToken = default)
            => Task.FromResult(_reader.ReadInt32(Resolve(address)));

        public Task<short> ReadInt16Async(string address, CancellationToken cancellationToken = default)
            => Task.FromResult(_reader.ReadInt16(Resolve(address)));

        public Task<float> ReadSingleAsync(string address, CancellationToken cancellationToken = default)
            => Task.FromResult(_reader.ReadSingle(Resolve(address)));

        public Task<string> ReadStringAsync(string address, int byteLength, Encoding encoding, CancellationToken cancellationToken = default)
        {
            var bytes = _reader.ReadBytes(Resolve(address), byteLength);
            var str = encoding.GetString(bytes);
            int nullIndex = str.IndexOf('\0');
            if (nullIndex >= 0)
            {
                str = str[..nullIndex];
            }
            return Task.FromResult(str.TrimEnd('\0'));
        }

        public Task<string> ReadDataHexAsync(string address, int lengthBytes, CancellationToken cancellationToken = default)
            => Task.FromResult(ReadDataHex(address, lengthBytes));

        public string ReadDataHex(string address, int lengthBytes)
        {
            var bytes = _reader.ReadBytes(Resolve(address), lengthBytes);
            return BitConverter.ToString(bytes, 0, lengthBytes).Replace('-', ' ');
        }

        /// <summary>
        /// 讀取原始 bytes（用於診斷編碼格式）
        /// </summary>
        public byte[] ReadBytes(string address, int lengthBytes)
        {
            return _reader.ReadBytes(Resolve(address), lengthBytes);
        }

        public Task WriteInt32Async(string address, int value, CancellationToken cancellationToken = default)
        {
            _reader.WriteInt32(Resolve(address), value);
            return Task.CompletedTask;
        }

        public Task WriteInt16Async(string address, short value, CancellationToken cancellationToken = default)
        {
            _reader.WriteInt16(Resolve(address), value);
            return Task.CompletedTask;
        }

        public Task WriteSingleAsync(string address, float value, CancellationToken cancellationToken = default)
        {
            _reader.WriteSingle(Resolve(address), value);
            return Task.CompletedTask;
        }

        public Task WriteBytesAsync(string address, ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
        {
            _reader.WriteBytes(Resolve(address), data.Span);
            return Task.CompletedTask;
        }

        public Task WriteStringAsync(string address, string value, Encoding encoding, CancellationToken cancellationToken = default)
        {
            var bytes = encoding.GetBytes(value + '\0');
            _reader.WriteBytes(Resolve(address), bytes);
            return Task.CompletedTask;
        }

        public Task WriteDataHexAsync(string address, string hexString, CancellationToken cancellationToken = default)
        {
            _reader.WriteDataHex(Resolve(address), hexString);
            return Task.CompletedTask;
        }

        private nuint Resolve(string expression)
        {
            return AddressParser.Resolve(expression, _moduleBase, _reader);
        }

        public void Dispose()
        {
            _reader.Dispose();
        }

        private static class AddressParser
        {
            private const string ModulePlaceholder = "<nobolhd.bng>";

            internal static nuint Resolve(string expr, nuint moduleBase, Win32ProcessMemoryReader reader)
            {
                if (string.IsNullOrWhiteSpace(expr)) return moduleBase;
                var cleaned = expr.Replace(" ", string.Empty).ToLowerInvariant();
                return Evaluate(cleaned, moduleBase, reader);
            }

            private static nuint Evaluate(string expr, nuint moduleBase, Win32ProcessMemoryReader reader)
            {
                expr = TrimLeadingPlus(expr);
                if (string.IsNullOrEmpty(expr)) return moduleBase;

                if (expr.StartsWith("["))
                {
                    (var inner, var rest) = ExtractBracket(expr);
                    nuint innerVal = Evaluate(inner, moduleBase, reader);
                    nuint derefVal = reader.ReadPointer(innerVal);
                    return string.IsNullOrEmpty(rest) ? derefVal : Evaluate(rest, moduleBase, reader, derefVal);
                }

                return Evaluate(expr, moduleBase, reader, 0);
            }

            private static nuint Evaluate(string expr, nuint moduleBase, Win32ProcessMemoryReader reader, nuint current)
            {
                expr = TrimLeadingPlus(expr);
                if (string.IsNullOrEmpty(expr)) return current;

                if (expr.StartsWith(ModulePlaceholder, StringComparison.OrdinalIgnoreCase))
                {
                    return Evaluate(expr[ModulePlaceholder.Length..], moduleBase, reader, current + moduleBase);
                }

                if (expr.StartsWith("["))
                {
                    (var inner, var rest) = ExtractBracket(expr);
                    nuint innerVal = Evaluate(inner, moduleBase, reader);
                    nuint derefVal = reader.ReadPointer(innerVal);
                    return Evaluate(rest, moduleBase, reader, current + derefVal);
                }

                int idx = 0;
                while (idx < expr.Length && IsHex(expr[idx])) idx++;
                string numHex = expr[..idx];
                nuint num = string.IsNullOrEmpty(numHex) ? 0 : (nuint)Convert.ToUInt64(numHex, 16);
                current += num;
                string remaining = expr[idx..];
                if (string.IsNullOrEmpty(remaining)) return current;
                return Evaluate(remaining, moduleBase, reader, current);
            }

            private static (string inner, string rest) ExtractBracket(string expr)
            {
                int depth = 0;
                for (int i = 0; i < expr.Length; i++)
                {
                    if (expr[i] == '[') depth++;
                    else if (expr[i] == ']')
                    {
                        depth--;
                        if (depth == 0)
                        {
                            string inner = expr.Substring(1, i - 1);
                            string rest = expr[(i + 1)..];
                            return (inner, rest);
                        }
                    }
                }
                return (expr.Trim('[', ']'), string.Empty);
            }

            private static bool IsHex(char c) =>
                (c >= '0' && c <= '9') ||
                (c >= 'a' && c <= 'f') ||
                (c >= 'A' && c <= 'F');

            private static string TrimLeadingPlus(string expr)
            {
                while (expr.StartsWith("+")) expr = expr[1..];
                return expr;
            }
        }
    }
}
