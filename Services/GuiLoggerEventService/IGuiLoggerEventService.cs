using System;
using Microsoft.Extensions.Logging;

namespace FoxHollow.FHM.Shared.Services;

public interface IGuiLoggerEventService
{
    GuiLogMessageEvent RegisterLogDestination(GuiLogMessageEvent callback);

    void WriteLog(string level, string message);
}