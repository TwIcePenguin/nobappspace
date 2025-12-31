using System;
using System.Buffers;
using System.Buffers.Binary;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace NOBApp.Memory
{
	internal sealed class Win32ProcessMemoryReader : IDisposable
	{
		private readonly SafeProcessHandle _processHandle;

		internal Win32ProcessMemoryReader(int processId)
		{
			_processHandle = NativeMethods.OpenProcess(
				ProcessAccessFlags.VMRead | ProcessAccessFlags.VMOperation | ProcessAccessFlags.VMWrite,
				false,
				processId);

			if (_processHandle.IsInvalid)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to open process for memory access");
			}
		}

		internal bool IsOpen => !_processHandle.IsInvalid && !_processHandle.IsClosed;

		internal int ReadInt32(nuint address)
		{
			Span<byte> buffer = stackalloc byte[4];
			return TryRead(address, buffer) ? BinaryPrimitives.ReadInt32LittleEndian(buffer) : 0;
		}

		internal short ReadInt16(nuint address)
		{
			Span<byte> buffer = stackalloc byte[2];
			return TryRead(address, buffer) ? BinaryPrimitives.ReadInt16LittleEndian(buffer) : (short)0;
		}

		internal float ReadSingle(nuint address)
		{
			Span<byte> buffer = stackalloc byte[4];
			return TryRead(address, buffer) ? BitConverter.ToSingle(buffer) : 0f;
		}

		internal byte[] ReadBytes(nuint address, int length)
		{
			var buffer = new byte[length];
			TryRead(address, buffer);
			return buffer;
		}

		internal string ReadDataHex(nuint address, int length)
		{
			var bytes = ReadBytes(address, length);
			return BitConverter.ToString(bytes, 0, length).Replace('-', ' ');
		}

		internal nuint ReadPointer(nuint address)
		{
			int size = IntPtr.Size;
			Span<byte> buffer = stackalloc byte[16];
			var slice = buffer.Slice(0, size);
			if (!TryRead(address, slice)) return 0;
			return size == 8 ? (nuint)BitConverter.ToUInt64(slice) : (nuint)BitConverter.ToUInt32(slice);
		}

		internal void WriteInt32(nuint address, int value)
		{
			Span<byte> buffer = stackalloc byte[4];
			BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
			TryWrite(address, buffer);
		}

		internal void WriteInt16(nuint address, short value)
		{
			Span<byte> buffer = stackalloc byte[2];
			BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
			TryWrite(address, buffer);
		}

		internal void WriteSingle(nuint address, float value)
		{
			Span<byte> buffer = stackalloc byte[4];
			BitConverter.TryWriteBytes(buffer, value);
			TryWrite(address, buffer);
		}

		internal void WriteBytes(nuint address, ReadOnlySpan<byte> data)
		{
			TryWrite(address, data);
		}

		internal void WriteDataHex(nuint address, string hex)
		{
			var bytes = ParseHexString(hex);
			if (bytes.Length == 0) return;
			TryWrite(address, bytes);
		}

		private bool TryRead(nuint address, Span<byte> buffer)
		{
			if (!IsOpen) return false;
			var temp = ArrayPool<byte>.Shared.Rent(buffer.Length);
			try
			{
				if (!NativeMethods.ReadProcessMemory(_processHandle, (IntPtr)address, temp, buffer.Length, out var bytesRead) || bytesRead == 0)
					return false;
				temp.AsSpan(0, buffer.Length).CopyTo(buffer);
				return true;
			}
			finally
			{
				ArrayPool<byte>.Shared.Return(temp);
			}
		}

		private bool TryWrite(nuint address, ReadOnlySpan<byte> data)
		{
			if (!IsOpen) return false;
			var temp = data.ToArray();
			return NativeMethods.WriteProcessMemory(_processHandle, (IntPtr)address, temp, temp.Length, out var bytesWritten) && bytesWritten > 0;
		}

		public void Dispose()
		{
			if (!_processHandle.IsClosed)
			{
				_processHandle.Dispose();
			}
		}

		private static byte[] ParseHexString(string hex)
		{
			if (string.IsNullOrWhiteSpace(hex))
			{
				return Array.Empty<byte>();
			}

			Span<char> cleaned = stackalloc char[hex.Length];
			int len = 0;
			foreach (char c in hex)
			{
				if (c != ' ')
				{
					cleaned[len++] = c;
				}
			}

			if (len % 2 != 0)
			{
				Debug.WriteLine($"WriteDataHex: invalid hex length {len}");
				return Array.Empty<byte>();
			}

			byte[] bytes = new byte[len / 2];
			for (int i = 0; i < len; i += 2)
			{
				bytes[i / 2] = Convert.ToByte(new string(cleaned.Slice(i, 2)), 16);
			}
			return bytes;
		}

		private static class NativeMethods
		{
			[DllImport("kernel32.dll", SetLastError = true)]
			internal static extern SafeProcessHandle OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);

			[DllImport("kernel32.dll", SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			internal static extern bool ReadProcessMemory(SafeProcessHandle hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

			[DllImport("kernel32.dll", SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			internal static extern bool WriteProcessMemory(SafeProcessHandle hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);
		}
	}

	[Flags]
	internal enum ProcessAccessFlags : uint
	{
		VMRead = 0x0010,
		VMWrite = 0x0020,
		VMOperation = 0x0008,
		QueryInformation = 0x0400,
		All = 0x001F0FFF
	}
}
