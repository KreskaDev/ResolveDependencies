namespace ResolveDependencies.Console;

/// <summary>
/// Thrown by <see cref="CommandLineParser"/> when the arguments do not form a valid call.
/// </summary>
public sealed class CommandLineException(string message) : Exception(message);
