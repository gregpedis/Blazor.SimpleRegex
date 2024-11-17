using SimpleRegex.Scanning;

namespace SimpleRegex;

public class ScanningException(int line, string message) : Exception($"Scanning Error: {message} at line {line}.");

public class ParsingException(Token token, string message) : Exception($"Parsing Error: {message} at token {token}.");

public class InterpreterException(string message) : Exception($"Intepretation Error: {message}.");
