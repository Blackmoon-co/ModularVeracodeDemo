namespace VeracodeDemo.Shared;

public sealed record ModuleInfo(
    string Name,
    string BusinessCapability,
    string[] Dependencies);
