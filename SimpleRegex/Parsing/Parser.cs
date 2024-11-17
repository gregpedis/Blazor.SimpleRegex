using SimpleRegex.Parsing.Nodes;
using SimpleRegex.Scanning;
using System.Reflection.Metadata.Ecma335;

namespace SimpleRegex.Parsing;

public class Parser(List<Token> tokens)
{
	private static readonly HashSet<TokenType> SIMPLE_QUANTIFIER_TOKENS =
	[
		TokenType.MAYBE,
		TokenType.MAYBE_MANY,
		TokenType.MANY,
	];

	private static readonly HashSet<TokenType> PRECISE_QUANTIFIER_TOKENS =
	[
		TokenType.EXACTLY,
		TokenType.AT_LEAST,
		TokenType.BETWEEN
	];

	private static readonly HashSet<TokenType> QUANTIFIER_TOKENS =
	[
		..SIMPLE_QUANTIFIER_TOKENS,
		..PRECISE_QUANTIFIER_TOKENS,
	];

	private readonly List<Token> tokens = tokens;
	private int current = 0;

	// for REPL Expression parsing
	public Expr ParseExpression() =>
		Or();

	#region EXPRESSIONS
	// or -> concat ( "|" concat )*
	private Or Or()
	{
		var or = new Or([Concat()]);
		while (Match(TokenType.OR))
		{
			or.Operands.Add(Concat());
		}
		return or;
	}

	// concat -> lazy ( "+" lazy )*
	private Concat Concat()
	{
		var first = Lazy();
		var or = new Concat([first]);
		while (Match(TokenType.CONCAT))
		{
			or.Operands.Add(Lazy());
		}
		return or;
	}

	// lazy -> "lazy(" quantifier ")" | quantifier | factor
	private Expr Lazy()
	{
		if (Match(TokenType.LAZY))
		{
			Consume(TokenType.LEFT_PAREN, "Expect '(' after lazy");
			var result = new Lazy(Quantifier());
			Consume(TokenType.RIGHT_PAREN, "Expect ')' after argument");
			return result;
		}
		else if (CheckAny(QUANTIFIER_TOKENS))
		{
			return Quantifier();
		}
		else
		{
			return Factor();
		}
	}

	// quantifier -> simple_quantifier | precise_quantifier
	private Expr Quantifier() =>
		SIMPLE_QUANTIFIER_TOKENS.Contains(Peek().Type)
			? SimpleQuantifier()
			: PreciseQuantifier();

	// simple_quantifier -> ("maybe" | "maybemany" | "many") "(" or ")"
	private Expr SimpleQuantifier()
	{
		var quantifier = Advance();
		Consume(TokenType.LEFT_PAREN, $"Expect '(' after {quantifier.Lexeme}");
		var argument = Or();
		Consume(TokenType.RIGHT_PAREN, $"Expect ')' after {quantifier.Lexeme}'s argument");

		return quantifier.Type switch
		{
			TokenType.MAYBE => new Maybe(argument),
			TokenType.MAYBE_MANY => new MaybeMany(argument),
			TokenType.MANY => new Many(argument),
			_ => throw Error(quantifier, "Expect simple quantifier"),
		};
	}

	// precise_quantifier -> exactly_or_atleast | between
	private Expr PreciseQuantifier() =>
		Check(TokenType.EXACTLY) || Check(TokenType.AT_LEAST)
			? ExactlyOrAtLeast()
			: Between();

	// exactly_or_atleast -> ("exactly" | atleast) "(" or "," number ")"
	private Expr ExactlyOrAtLeast()
	{
		var quantifier = Advance();
		Consume(TokenType.LEFT_PAREN, $"Expect '(' after {quantifier.Lexeme}");
		var argument = Or();
		Consume(TokenType.COMMA, $"Expect ',' after {quantifier.Lexeme} first argument");
		Consume(TokenType.NUMBER, $"Expect number as {quantifier.Lexeme}'s second argument");
		var number = Previous().Number.Value;
		Consume(TokenType.RIGHT_PAREN, $"Expect ')' after {quantifier.Lexeme}'s second argument");

		return quantifier.Type switch
		{
			TokenType.EXACTLY => new Exactly(argument, number),
			TokenType.AT_LEAST => new AtLeast(argument, number),
			_ => throw Error(quantifier, "Expect EXACTLY or ATLEAST"),
		};
	}

	// between-> "between" "(" or "," number "," number ")"
	private Between Between()
	{
		var quantifier = Peek();
		Consume(TokenType.BETWEEN, $"Expect '{nameof(TokenType.BETWEEN)}'");
		Consume(TokenType.LEFT_PAREN, $"Expect '(' after {quantifier.Lexeme}");

		var argument = Or();

		Consume(TokenType.COMMA, $"Expect ',' after {quantifier.Lexeme}'s first argument");
		Consume(TokenType.NUMBER, $"Expect number as {quantifier.Lexeme}'s MIN argument");
		var min = Previous().Number.Value;

		Consume(TokenType.COMMA, $"Expect ',' after {quantifier.Lexeme}'s second argument");
		Consume(TokenType.NUMBER, $"Expect number as {quantifier.Lexeme}'s MAX argument");
		var max = Previous().Number.Value;

		Consume(TokenType.RIGHT_PAREN, $"Expect ')' after {quantifier.Lexeme}'s last argument");
		return new Between(argument, min, max);
	}

	// factor -> grouping | term
	private Expr Factor() =>
		Check(TokenType.LEFT_PAREN)
			? Grouping()
			: Term();

	// grouping -> "(" or ")"
	private Grouping Grouping()
	{
		Consume(TokenType.LEFT_PAREN, "Expect '(' before grouping");
		var or = Or();
		Consume(TokenType.RIGHT_PAREN, "Expect ')' after grouping");
		return new(or);
	}

	// term -> any | start | end | ws | digit | word | boundary | nl | cr | tab | null | literal
	private Expr Term()
	{
		var token = Peek();
		Expr term = token.Type switch
		{
			TokenType.ANY => Any.Instance,
			TokenType.START => Start.Instance,
			TokenType.END => End.Instance,

			TokenType.WHITESPACE => Whitespace.Instance,
			TokenType.DIGIT => Digit.Instance,
			TokenType.WORD => Word.Instance,
			TokenType.BOUNDARY => Boundary.Instance,
			TokenType.NEWLINE => NewLine.Instance,
			TokenType.CR => Cr.Instance,
			TokenType.TAB => Tab.Instance,
			TokenType.NULL => Null.Instance,

			TokenType.LITERAL => new Literal(token.Lexeme[1..^1]), // delete the quotes. "abc" becomes abc.
			_ => throw Error(token, "Expect Term")
		};
		Advance();
		return term;
	}

	#endregion

	#region HELPERS
	private bool Match(params TokenType[] types)
	{
		if (Array.Exists(types, Check))
		{
			Advance();
			return true;
		}
		return false;
	}

	private Token Consume(TokenType type, string message)
	{
		if (Check(type))
		{
			return Advance();
		}

		throw Error(Peek(), message);
	}

	private bool Check(TokenType type)
	{
		if (IsAtEnd()) return false;
		return Peek().Type == type;
	}

	private bool CheckAny(HashSet<TokenType> type)
	{
		if (IsAtEnd()) return false;
		return type.Contains(Peek().Type);
	}

	private Token Advance()
	{
		if (!IsAtEnd())
		{
			current++;
		}
		return Previous();
	}

	private Token Peek() =>
		tokens[current];

	private Token Previous() =>

		tokens[current - 1];

	private bool IsAtEnd() =>
	  Peek().Type == TokenType.EOF;

	private static ParsingException Error(Token token, string message) =>
		new(token, message);
	#endregion
}