using System;

namespace scrapsy.Enums
{
    [Flags]
    public enum LoggerLevel
    {
        All = 1,
        Debug = 2,
        Info = 4,
        Warning = 8,
        Severe = 16,
        Off = 32
    }
}