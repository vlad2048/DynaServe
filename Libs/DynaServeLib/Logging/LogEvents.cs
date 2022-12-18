namespace DynaServeLib.Logging;

public record LogEvt(
    string Message,
    string[] CssLinks,
    string Dom,
    string? FullLog
);
