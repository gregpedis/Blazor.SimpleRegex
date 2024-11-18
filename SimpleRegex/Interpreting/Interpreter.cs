using System.Text;
using SimpleRegex.Parsing.Nodes;

namespace SimpleRegex.Interpreting;

internal static class Interpreter
{
	private static readonly HashSet<char> CHARACTERS_TO_ESCAPE =
	[
		'(', ')',
		'[', ']',
		'[', ']',
		'.', '^', '$' ,
		'\\',
		'?', '*', '+',
		'|',
	];

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
			Quote => "\"\"",

			_ => throw Error($"Invalid expression '{expression}'")
		};

	private static string Interpret(Or or) =>
		string.Join('|', or.Operands.Select(Interpret));

	private static string Interpret(Concat concat) =>
		string.Join("", concat.Operands.Select(Interpret));

	private static string Interpret(Lazy lazy) =>
		Interpret(lazy.Value) + '?';

	// TODO: Cannot use quantifiers on anchors. Throw interpreter exception. Applies to [maybe, manyMany, many, exactly, atLeast, between]. Maybe fix it in the parser.
	private static string Interpret(Maybe maybe) =>
		Parenthesize(Interpret(maybe.Value)) + '?';

	private static string Interpret(MaybeMany maybeMany) =>
		Parenthesize(Interpret(maybeMany.Value)) + '*';

	private static string Interpret(Many many) =>
		Parenthesize(Interpret(many.Value)) + '+';

	private static string Interpret(Exactly exactly) =>
		Parenthesize(Interpret(exactly.Left)) + '{' + exactly.Right + '}';

	private static string Interpret(AtLeast atLeast) =>
		Parenthesize(Interpret(atLeast.Left)) + '{' + atLeast.Right + ",}";

	private static string Interpret(Between between) =>
		Parenthesize(Interpret(between.Value)) + '{' + between.Min + ',' + between.Max + '}';

	private static string Interpret(Grouping grouping) =>
		$"({Interpret(grouping.Value)})";

	private static string Interpret(Literal literal) =>
		Escape(literal.Value);

	#region HELPERS

	// TODO: Do the anchor filtering a bit better
	private static string Parenthesize(string value)
	{
		// Length<=1 is ok if it is not an anchor.
		if (value.Length <= 1)
		{
			return value is not ("^" or "$") ? value : $"({value})";
		}
		// Length==2 is ok if it starts with the escape character "\" and is not an anchor.
		else if (value.Length == 2)
		{
			return value[0] == '\\' && value[1] is not ('b' or 'B') ? value : $"({value})"; // TODO: There is also ["\G", "\A", "\z"]
		}
		// Length>2 is ok if it is a group or a character class.
		else
		{
			if (value.StartsWith('(') && value.IndexOf(')') == value.Length - 1) // the expression is a single group.
			{
				return value;
			}
			else if (value.StartsWith('[') && value.IndexOf(']') == value.Length - 1) // the expression is a single character class.
			{
				return value;
			}
			else
			{
				return $"({value})";
			}
		}
	}

	private static string Escape(string value)
	{
		var sb = new StringBuilder();
		foreach (var c in value)
		{
			if (CHARACTERS_TO_ESCAPE.Contains(c))
			{
				sb.Append('\\');
			}
			sb.Append(c);
		}
		return sb.ToString();
	}

	private static InterpreterException Error(string message) =>
		new(message);
	#endregion
}

