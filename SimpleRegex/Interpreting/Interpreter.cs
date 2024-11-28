using System.Text;
using SimpleRegex.Parsing.Nodes;
using Range = SimpleRegex.Parsing.Nodes.Range;

namespace SimpleRegex.Interpreting;

internal static class Interpreter
{
	private static readonly HashSet<char> CHARACTERS_TO_ESCAPE =
	[
		'(', ')',
		'[', ']',
		'[', ']',
		'.', '^', '$' ,
		'\\', '-',
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

			Match match => Interpret(match),
			NotMatch notMatch => Interpret(notMatch),
			Capture capture => Interpret(capture),
			NamedCapture namedCapture => Interpret(namedCapture),

			AnyOf anyOf => Interpret(anyOf),
			NotAnyOf notAnyOf => Interpret(notAnyOf),
			Range range => Interpret(range),

			Literal literal => Interpret(literal),

			Any => ".",
			Start => "^",
			End => "$",
			Whitespace => @"\s",
			Digit => @"\d",
			NotDigit => @"\D",
			Word => @"\w",
			NotWord => @"\W",
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

	private static string Interpret(Match match) =>
		$"(?:{Interpret(match.Value)})";

	private static string Interpret(NotMatch notMatch) =>
		$"(?!{Interpret(notMatch.Value)})";

	private static string Interpret(Capture capture) =>
		$"({Interpret(capture.Value)})";

	private static string Interpret(NamedCapture namedCapture)
	{
		var name = Interpret(namedCapture.Right);
		var value = Interpret(namedCapture.Left);
		if (name.Length == 0)
		{
			throw Error($"Named capture group (?<{name}>{value})'s name requires at least one character");
		}
		else if (name.All(x => x == '_' || char.IsDigit(x) || char.IsAsciiLetter(x)))
		{
			return $"(?<{name}>{value})";
		}
		else
		{
			throw Error($"Named capture group (?<{name}>{value})'s name can only comprise of ASCII alphanumeric characters (A-Z, a-z, 0-9) and '_'");
		}
	}

	private static string Interpret(AnyOf anyOf)
	{
		var value = string.Join("", anyOf.Operands.Select(Interpret));
		// This is a hack. "start" needs to be escaped only at the first position, or the character class is reversed.
		value = value.StartsWith('^') ? $"\\{value}" : value;

		if (value.Length == 0)
		{
			throw Error($"Character class {anyOf} requires at least one character");
		}
		else
		{
			return $"[{value}]";
		}
	}

	private static string Interpret(NotAnyOf notAnyOf) =>
		$"[^{string.Join("", notAnyOf.Operands.Select(Interpret))}]";

	private static string Interpret(Range range)
	{
		if (range.Left.Value.Length != 1)
		{
			throw Error($"Invalid expression '{range.Left.Value}'. Range's left argument is expected to be a single character");
		}
		if (range.Right.Value.Length != 1)
		{
			throw Error($"Invalid expression '{range.Right.Value}'. Range's right argument is expected to be a single character");
		}

		return $"{Interpret(range.Left)}-{Interpret(range.Right)}";
	}

	private static string Interpret(Literal literal) =>
		Escape(literal.Value);

	#region HELPERS
	private static string Parenthesize(string value)
	{
		// Length<=1 is ALWAYS ok as anchors are NOT quantifiable.
		if (value.Length <= 1)
		{
			return value;
		}
		// Length==2 is ok if it starts with the escape character "\", as as anchors are NOT quantifiable.
		else if (value.Length == 2)
		{
			return value[0] == '\\' ? value : $"({value})";
		}
		// Length>2 is ok if it is a single group or a character class.
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

