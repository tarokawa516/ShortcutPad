using System.ComponentModel;
using System.Runtime.InteropServices;

namespace ShortcutPad;

internal static class ShortcutSender
{
    internal const ushort Shift = 0x10;
    internal const ushort Control = 0x11;

    private const uint InputKeyboard = 1;
    private const uint KeyUp = 0x0002;

    public static void SendChord(ushort key, params ushort[] modifiers)
    {
        var inputs = new List<Input>();

        foreach (var modifier in modifiers)
        {
            inputs.Add(CreateKeyboardInput(modifier, keyUp: false));
        }

        inputs.Add(CreateKeyboardInput(key, keyUp: false));
        inputs.Add(CreateKeyboardInput(key, keyUp: true));

        for (var index = modifiers.Length - 1; index >= 0; index--)
        {
            inputs.Add(CreateKeyboardInput(modifiers[index], keyUp: true));
        }

        var sent = SendInput((uint)inputs.Count, inputs.ToArray(), Marshal.SizeOf<Input>());
        if (sent != inputs.Count)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), "キー操作を送信できませんでした。");
        }
    }

    private static Input CreateKeyboardInput(ushort virtualKey, bool keyUp) => new()
    {
        Type = InputKeyboard,
        Data = new InputUnion
        {
            Keyboard = new KeyboardInput
            {
                VirtualKey = virtualKey,
                Flags = keyUp ? KeyUp : 0
            }
        }
    };

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint inputCount, Input[] inputs, int inputSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public uint Type;
        public InputUnion Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public MouseInput Mouse;
        [FieldOffset(0)] public KeyboardInput Keyboard;
        [FieldOffset(0)] public HardwareInput Hardware;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseInput
    {
        public int X;
        public int Y;
        public uint MouseData;
        public uint Flags;
        public uint Time;
        public UIntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyboardInput
    {
        public ushort VirtualKey;
        public ushort ScanCode;
        public uint Flags;
        public uint Time;
        public UIntPtr ExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HardwareInput
    {
        public uint Message;
        public ushort ParameterLow;
        public ushort ParameterHigh;
    }
}
