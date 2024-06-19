using System;
using System.IO;
using UnityEngine;

public static class AxegenLogger
{
    public enum DEBUG_TYPE
    {
        INFO,
        WARNING,
        ERROR,
        DEBUG
    }

    private static readonly string LogFileName = "AXEGEN_LOGGER.log";
    private static readonly string TimeFormat = "yyyy-MM-dd HH:mm:ss";
    private static readonly object LockObject = new object();
    private static StreamWriter _streamWriter;

    static AxegenLogger()
    {
        InitializeLogFile();
    }
    private static void InitializeLogFile()
    {
        try
        {
            // Create or overwrite the log file
            _streamWriter = new StreamWriter(LogFileName, false)
            {
                AutoFlush = true
            };
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to initialize logger: {e.Message}");
        }
    }

    public static void Log(string message, DEBUG_TYPE type = DEBUG_TYPE.INFO)
    {
        string logMessage = $"[{DateTime.Now.ToString(TimeFormat)}] [{type}] : {message}";
        Debug.Log(message);

        lock (LockObject)
        {
            _streamWriter?.WriteLine(logMessage);
        }
    }
    public static void LogError(string message) => Log(message, DEBUG_TYPE.ERROR);
    public static void LogWarning(string message) => Log(message, DEBUG_TYPE.WARNING);
    public static void LogDebug(string message) => Log(message, DEBUG_TYPE.DEBUG);

    public static void Close()
    {
        lock (LockObject)
        {
            _streamWriter?.Dispose();
            _streamWriter = null;
        }
    }
}
