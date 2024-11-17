using SimpleRegex.Parsing.Nodes;

namespace SimpleRegex.Parsing;

public static class Interpreter
{
	public static string Interpret(Expr expression) =>
		expression switch
		{
			Or or => Interpret(or),
			Concat concat => Interpret(concat),

			Lazy lazy => Interpret(lazy),
			Maybe maybe => Interpret(maybe),
			MaybeMany maybeMany => Interpret(maybeMany),
			Many many => Interpret(many),

			Exactly exactly => Interpret(exactly),
			AtLeast atLeast => Interpret(atLeast),
			Between between => Interpret(between),

			Grouping grouping => Interpret(grouping),
			Literal literal => Interpret(literal),

			Any => ".",
			Start => "^",
			End => "$",
			Whitespace => @"\s",
			Digit => @"\d",
			Word => @"\w",
			Boundary => @"\b",
			NewLine => @"\n",
			Cr => @"\r",
			Tab => @"\t",
			Null => @"\0",

			_ => throw Error($"Invalid expression '{expression}'")
		};
	// TODO: this might need stripping of quotes.

	private static string Interpret(Or or) =>
		string.Join('|', or.Operands.Select(Interpret));

	private static string Interpret(Concat concat) =>
		string.Join("",concat.Operands.Select(Interpret));

	private static string Interpret(Lazy lazy) =>
		$"{Interpret(lazy.Value)}?";

	private static string Interpret(Maybe maybe) =>
		$"({Interpret(maybe.Value)})?";													// TODO: Do not always parenthesize

	private static string Interpret(MaybeMany maybeMany) =>
		$"({Interpret(maybeMany.Value)})*";												// TODO: Do not always parenthesize

	private static string Interpret(Many many)											// TODO: Same as above, extract the logic maybe
	{
		var inner = Interpret(many.Value);
		return inner.Length == 1
			? $"{inner}+"
			: $"({inner})+";
	}

	private static string Interpret(Exactly exactly) =>
		$$"""({{Interpret(exactly.Left)}}){{{exactly.Right}}}""";						// TODO: Do not always parenthesize

	private static string Interpret(AtLeast atLeast) =>
		$$"""({{Interpret(atLeast.Left)}}){{{atLeast.Right}},}""";						// TODO: Do not always parenthesize

	private static string Interpret(Between between) =>
		$$"""({{Interpret(between.Value)}}){{{between.Min}},{{between.Max}}}""";		// TODO: Do not always parenthesize

	private static string Interpret(Grouping grouping) =>								// TODO: Do not always parenthesize
		$"({Interpret(grouping.Value)})";

	private static string Interpret(Literal literal) =>									// TODO: Escape escape '\' characters
		literal.Value;

	#region HELPERS
	// TODO
	private static string Parenthesize(string x)
	{
		return x;
	}

	// TODO:
	private static string Escape(string x)
	{
		return x;
	}

	private static InterpreterException Error(string message) =>
		new(message);
	#endregion
}
