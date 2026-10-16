namespace ResolveDependencies.Console;

/// <summary>
/// Folders the application works on. <paramref name="OutputFolder"/> equals
/// <paramref name="InputFolder"/> when the command line gives no output folder.
/// </summary>
public sealed record CommandLineOptions(string InputFolder, string OutputFolder);
