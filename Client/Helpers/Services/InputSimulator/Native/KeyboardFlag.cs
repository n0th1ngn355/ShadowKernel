﻿using System;

namespace Client.Helpers.Services.InputSimulator.Native
{
    [Flags]
    internal enum KeyboardFlag : uint
    {
        ExtendedKey = 0x0001,

        KeyUp = 0x0002,

        UTF8 = 0x0004,

        ScanCode = 0x0008
    }
}