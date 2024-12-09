using SimpleRegex.Interpreting;
using SimpleRegex.Parsing.Nodes;
using SimpleRegex.Parsing;
using SimpleRegex.Scanning;

namespace SimpleRegex;

public static class Compiler
{
	public static CompilationResult Compile(string input)
	{
		try
		{
			var tokens = Scan(input);
			var tree = Parse(tokens);
			var regex = Interpret(tree);
			return new(true, tokens, tree, regex);
		}
		catch (ScanningException ex)
		{
			return new(false, Exception: ex);
		}
		catch (ParsingException ex)
		{
			return new(false, Exception: ex);
		}
		catch (InterpreterException ex)
		{
			return new(false, Exception: ex);
		}
	}

	static List<Token> Scan(string input) =>
		new Scanner(input).ScanTokens();

	static Expr Parse(List<Token> tokens) =>
		new Parser(tokens).Parse();

	static string Interpret(Expr tree) =>
		Interpreter.Interpret(tree);
}

public record class CompilationResult(
	bool Success,
	List<Token> Tokens = null,
	Expr Tree = null,
	string Regex = null,
	Exception Exception = null);
