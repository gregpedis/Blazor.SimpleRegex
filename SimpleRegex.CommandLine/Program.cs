using SimpleRegex;
using SimpleRegex.Scanning;
using SimpleRegex.Parsing.Nodes;

var shouldPrintTokens = true;
var shouldPrintTree = true;

while (true)
{
	Console.Write("> ");
	var input = Console.ReadLine();
	var compilation = Compiler.Compile(input);

	if (compilation.Success)
	{
		PrintTokens(compilation.Tokens);
		PrintTree(compilation.Tree);
		PrintTitle("REGEX");
		Console.WriteLine(compilation.Regex);
	}
	else
	{
		Console.WriteLine(compilation.Exception.Message);
	}
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
