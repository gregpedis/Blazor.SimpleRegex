using SimpleRegex;
using SimpleRegex.Parsing;
using SimpleRegex.Parsing.Nodes;
using SimpleRegex.Scanning;

var shouldPrintTokens = true;
var shouldPrintTree = true;

while (true)
{
	Console.Write("> ");
	try
	{
		var tokens = GetTokens();
		PrintTokens(tokens);

		var tree = new Parser(tokens).ParseExpression();
		PrintTree(tree);

		PrintTitle("REGEX");
		var regex = Interpreter.Interpret(tree);
		Console.WriteLine(regex);
	}
	catch (ScanningException ex)
	{
		Console.WriteLine(ex.Message);
	}
	catch (ParsingException ex)
	{
		Console.WriteLine(ex.Message);
	}
	catch (InterpreterException ex)
	{
		Console.WriteLine(ex.Message);
	}
}

static List<Token> GetTokens()
{
	var input = Console.ReadLine();
	var scanner = new Scanner(input);
	var tokens = scanner.ScanTokens();
	return tokens;
}

void PrintTokens(List<Token> tokens)
{
	if (shouldPrintTokens)
	{
		PrintTitle("TOKENS");
		foreach (var token in tokens)
		{
			Console.WriteLine(token);
		}
	}
}

void PrintTree(Expr tree)
{
	if (shouldPrintTree)
	{
		PrintTitle("AST");
		Console.WriteLine(tree);
	}
}

void PrintTitle(string message)
{
	var before = Console.ForegroundColor;
	Console.ForegroundColor = ConsoleColor.Green;
	Console.WriteLine(message);
	Console.WriteLine(new string('=', message.Length));
	Console.ForegroundColor = before;
}
