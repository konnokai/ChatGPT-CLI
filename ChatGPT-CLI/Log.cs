using System.Diagnostics;

public static class Log
{
    enum LogType { Verb, Stream, Info, Warn, Error }
    private static readonly object logLockObj = new();

    public static void New(string text, bool newLine = true)
    {
        lock (logLockObj)
        {
            FormatColorWrite(text, ConsoleColor.Green, newLine);
        }
    }

    public static void Debug(string text, bool newLine = true)
    {
        if (!Debugger.IsAttached)
            return;

        lock (logLockObj)
        {
            FormatColorWrite(text, ConsoleColor.Cyan, newLine);
        }
    }

    public static void Info(string text, bool newLine = true)
    {
        lock (logLockObj)
        {
            FormatColorWrite(text, ConsoleColor.DarkYellow, newLine);
        }
    }

    public static void Warn(string text, bool newLine = true)
    {
        lock (logLockObj)
        {
            FormatColorWrite(text, ConsoleColor.DarkMagenta, newLine);
        }
    }

    public static void Error(string text, bool newLine = true, bool writeLog = true)
    {
        lock (logLockObj)
        {
            FormatColorWrite(text, ConsoleColor.DarkRed, newLine);
        }
    }

    public static void Error(Exception ex, string text, bool newLine = true)
    {
        lock (logLockObj)
        {
            FormatColorWrite(text, ConsoleColor.DarkRed, newLine);
            FormatColorWrite(ex.ToString(), ConsoleColor.DarkRed, newLine);
        }
    }

    public static void FormatColorWrite(string text, ConsoleColor consoleColor = ConsoleColor.Gray, bool newLine = true)
    {
        Console.ForegroundColor = consoleColor;
        if (newLine) Console.WriteLine(text);
        else Console.Write(text);
        Console.ForegroundColor = ConsoleColor.Gray;
    }
}