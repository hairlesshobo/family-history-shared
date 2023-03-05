using System;
using System.Reflection.Metadata;

namespace FoxHollow.FHM.Shared.Services;

public delegate void GuiLogMessageEvent(string logLevel, string message);

public class GuiLoggerEventService : IGuiLoggerEventService
{

    private event GuiLogMessageEvent _onLogMessage;

    public GuiLoggerEventService()
    {
        _onLogMessage += delegate { };
    }

    public GuiLogMessageEvent RegisterLogDestination(GuiLogMessageEvent callback)
    {
        _onLogMessage += callback;

        return callback;
    }

    public void WriteLog(string level, string message)
        => _onLogMessage(level, message);
}