using SimpleRegex.Parsing.Nodes;
using SimpleRegex.Scanning;
using Range = SimpleRegex.Parsing.Nodes.Range;

namespace SimpleRegex.Parsing;

internal class Parser(List<Token> tokens)
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

	private int current = 0;

	public Expr Parse()
	{
		var res = Execution();
		if (IsAtEnd())
		{
			return res;
		}
		else
		{
			throw Error(Peek(), $"Expect {nameof(TokenType.EOF)}");
		}
	}

	#region EXPRESSIONS

	// execution -> assignments or
	private Expr Execution()
	{
		var assignments = Assignments();
		var expression = Or();

		return assignments.Count > 0
			? new Execution(assignments, expression)
			: expression;
	}

	// assignments -> assignment*
	private List<Assignment> Assignments()
	{
		var assignments = new List<Assignment>();
		while (PeekNext().Type == TokenType.EQUALS)
		{
			assignments.Add(Assignment());
		}
		return assignments;
	}

	// assignment -> identifier "=" or
	private Assignment Assignment()
	{
		var identifier = Consume(TokenType.IDENTIFIER, "Expect identifier at the start of assignment");
		Consume(TokenType.EQUALS, "Expect '=' after identifier");
		return new(identifier.Lexeme, Or());
	}

	// or -> concat ( "|" concat )*
	private Expr Or()
	{
		var or = new Or([Concat()]);
		while (Match(TokenType.OR))
		{
			or.Operands.Add(Concat());
		}
		if (or.Operands.Count == 1)
		{
			return or.Operands[0];
		}
		else
		{
			return or;
		}
	}

	// concat -> lazy ( "+" lazy )*
	private Expr Concat()
	{
		var concat = new Concat([Lazy()]);
		while (Match(TokenType.CONCAT))
		{
			concat.Operands.Add(Lazy());
		}

		if (concat.Operands.Count == 1)
		{
			return concat.Operands[0];
		}
		else
		{
			return concat;
		}
	}

	// lazy -> "lazy" "(" quantifier ")" | quantifier | factor
	private Expr Lazy()
	{
		if (Match(TokenType.LAZY))
		{
			Consume(TokenType.LEFT_PAREN, "Expect '(' after lazy");
			if (QUANTIFIER_TOKENS.Contains(Peek().Type))
			{
				var result = new Lazy(Quantifier());
				Consume(TokenType.RIGHT_PAREN, "Expect ')' after argument");
				return result;
			}
			else
			{
				throw Error(Previous(), "Expect quantifier after '('");
			}
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
	private Expr Quantifier()
	{
		if (CheckAny(SIMPLE_QUANTIFIER_TOKENS))
		{
			return SimpleQuantifier();
		}
		else if (CheckAny(PRECISE_QUANTIFIER_TOKENS))
		{
			return PreciseQuantifier();
		}
		else
		{
			throw Error(Peek(), "Expect quantifier");
		}
	}

	// simple_quantifier -> ("maybe" | "maybemany" | "many") "(" or ")"
	private Expr SimpleQuantifier()
	{
		var quantifier = Advance();
		Consume(TokenType.LEFT_PAREN, $"Expect '(' after {quantifier.Lexeme}");
		var argument = VerifyNoAnchor(quantifier, Or());
		Consume(TokenType.RIGHT_PAREN, $"Expect ')' after {quantifier.Lexeme}'s argument");

		return quantifier.Type switch
		{
			TokenType.MAYBE => new Maybe(argument),
			TokenType.MAYBE_MANY => new MaybeMany(argument),
			TokenType.MANY => new Many(argument),
			_ => throw Error(quantifier, "Expect simple quantifier")
		};
	}

	// precise_quantifier -> exactly_or_atleast | between
	private Expr PreciseQuantifier()
	{
		if (Check(TokenType.EXACTLY) || Check(TokenType.AT_LEAST))
		{
			return ExactlyOrAtLeast();
		}
		else if (Check(TokenType.BETWEEN))
		{
			return Between();
		}
		else
		{
			throw Error(Peek(), "Expect precise quantifier");
		}
	}

	// exactly_or_atleast -> ("exactly" | "atleast") "(" or "," number ")"
	private Expr ExactlyOrAtLeast()
	{
		var quantifier = Advance();
		Consume(TokenType.LEFT_PAREN, $"Expect '(' after {quantifier.Lexeme}");
		var argument = VerifyNoAnchor(quantifier, Or());
		Consume(TokenType.COMMA, $"Expect ',' after {quantifier.Lexeme} first argument");
		var number = Consume(TokenType.NUMBER, $"Expect number as {quantifier.Lexeme}'s second argument");
		Consume(TokenType.RIGHT_PAREN, $"Expect ')' after {quantifier.Lexeme}'s second argument");

		return quantifier.Type switch
		{
			TokenType.EXACTLY => new Exactly(argument, number.Number.Value),
			TokenType.AT_LEAST => new AtLeast(argument, number.Number.Value),
			_ => throw Error(quantifier, "Expect 'exactly' or 'atleast'")
		};
	}

	// between-> "between" "(" or "," number "," number ")"
	private Between Between()
	{
		var quantifier = Consume(TokenType.BETWEEN, $"Expect '{nameof(TokenType.BETWEEN)}'");
		Consume(TokenType.LEFT_PAREN, $"Expect '(' after {quantifier.Lexeme}");

		var argument = VerifyNoAnchor(quantifier, Or());

		Consume(TokenType.COMMA, $"Expect ',' after {quantifier.Lexeme}'s first argument");
		var min = Consume(TokenType.NUMBER, $"Expect number as {quantifier.Lexeme}'s MIN argument");

		Consume(TokenType.COMMA, $"Expect ',' after {quantifier.Lexeme}'s second argument");
		var max = Consume(TokenType.NUMBER, $"Expect number as {quantifier.Lexeme}'s MAX argument");

		Consume(TokenType.RIGHT_PAREN, $"Expect ')' after {quantifier.Lexeme}'s last argument");
		return new Between(argument, min.Number.Value, max.Number.Value);
	}

	// factor -> group_construct | character_class | term
	private Expr Factor() =>
		Peek().Type switch
		{
			TokenType.CAPTURE or TokenType.MATCH or TokenType.NOT_MATCH => GroupConstruct(),
			TokenType.ANY_OF or TokenType.NOT_ANY_OF => CharacterClass(),
			_ => Term()
		};

	// group_construct -> match | capture
	private Expr GroupConstruct()
	{
		var group = Peek();
		return group.Type switch
		{
			TokenType.MATCH or TokenType.NOT_MATCH => Match(),
			TokenType.CAPTURE => Capture(),
			_ => throw Error(group, $"Expect '{TokenType.MATCH}' or '{TokenType.NOT_MATCH}' or '{TokenType.CAPTURE}'")
		};
	}

	// match -> ("match" | "notmatch") "(" or ")"
	private Expr Match()
	{
		var match = Advance();
		Consume(TokenType.LEFT_PAREN, $"Expect '(' after {match.Lexeme}");
		var expression = Or();
		Consume(TokenType.RIGHT_PAREN, $"Expect ')' after {match.Lexeme}'s argument");
		return match.Type switch
		{
			TokenType.MATCH => new Match(expression),
			TokenType.NOT_MATCH => new NotMatch(expression),
			_ => throw Error(match, $"Expect '{TokenType.MATCH}' or '{TokenType.NOT_MATCH}'")
		};
	}

	// capture -> "capture" "(" or ("," literal)? ")"
	private Expr Capture()
	{
		var capture = Consume(TokenType.CAPTURE, $"Expect '{nameof(TokenType.CAPTURE)}'");
		Consume(TokenType.LEFT_PAREN, $"Expect '(' after {capture.Lexeme}");

		var expression = Or();
		var name = Match(TokenType.COMMA)
			? Literal(Consume(TokenType.LITERAL, $"Expect literal as {capture.Lexeme}'s second argument"))
			: null;

		Consume(TokenType.RIGHT_PAREN, $"Expect ')' after {capture.Lexeme}'s arguments");
		return name switch
		{
			null => new Capture(expression),
			_ => new NamedCapture(expression, name)
		};
	}

	// character_class -> ("anyof" | "notanyof") "(" anyof_arg ("," anyof_arg)* ")"
	private VariadicExpr CharacterClass()
	{
		var characterClass = Advance();
		Consume(TokenType.LEFT_PAREN, $"Expect '(' after {characterClass.Lexeme}");

		var arguments = new List<Expr> { AnyOfArgument() };
		while (Match(TokenType.COMMA))
		{
			arguments.Add(AnyOfArgument());
		}

		Consume(TokenType.RIGHT_PAREN, $"Expect ')' after {characterClass.Lexeme}'s last argument");
		return characterClass.Type switch
		{
			TokenType.ANY_OF => new AnyOf(arguments),
			TokenType.NOT_ANY_OF => new NotAnyOf(arguments),
			_ => throw Error(characterClass, $"Expect '{TokenType.ANY_OF}' or '{TokenType.NOT_ANY_OF}'")
		};
	}

	// anyof_argument -> range | character_term
	private Expr AnyOfArgument() =>
		Check(TokenType.RANGE)
			? Range()
			: CharacterTerm();

	// range -> "range" "(" literal "," literal ")"
	private Range Range()
	{
		var range = Consume(TokenType.RANGE, $"Expect '{nameof(TokenType.RANGE)}'");
		Consume(TokenType.LEFT_PAREN, $"Expect '(' after {range.Lexeme}");

		var from = Consume(TokenType.LITERAL, $"Expect literal as {range.Lexeme}'s first argument");
		Consume(TokenType.COMMA, $"Expect ',' after {range.Lexeme}'s first argument");
		var to = Consume(TokenType.LITERAL, $"Expect literal as {range.Lexeme}'s second argument");

		Consume(TokenType.RIGHT_PAREN, $"Expect ')' after {range.Lexeme}'s second argument");
		return new Range(Literal(from), Literal(to));
	}

	// term -> character_term | non_character_term
	private Expr Term()
	{
		try
		{
			return CharacterTerm();
		}
		catch (ParsingException)
		{
			return NonCharacterTerm();
		}
	}

	// character_term -> ws | digit | notdigit | word | notWord | nl | cr | tab | quote | literal
	private Expr CharacterTerm()
	{
		var token = Peek();
		Expr term = token.Type switch
		{
			TokenType.WHITESPACE => Whitespace.Instance,
			TokenType.DIGIT => Digit.Instance,
			TokenType.NOT_DIGIT => NotDigit.Instance,
			TokenType.WORD => Word.Instance,
			TokenType.NOT_WORD => NotWord.Instance,
			TokenType.NEWLINE => NewLine.Instance,
			TokenType.CR => Cr.Instance,
			TokenType.TAB => Tab.Instance,
			TokenType.QUOTE => Quote.Instance,

			TokenType.LITERAL => Literal(token),
			_ => throw Error(token, "Expect Term")
		};
		Advance();
		return term;
	}

	// non_character_term -> any | start | end | boundary | notboundary | null
	private Expr NonCharacterTerm()
	{
		var token = Peek();
		Expr term = token.Type switch
		{
			TokenType.START => Start.Instance,
			TokenType.END => End.Instance,
			TokenType.BOUNDARY => Boundary.Instance,
			TokenType.NOT_BOUNDARY => NotBoundary.Instance,
			TokenType.ANY => Any.Instance,
			TokenType.NULL => Null.Instance,
			TokenType.IDENTIFIER => new Identifier(token.Lexeme),
			_ => throw Error(token, "Expect Term")
		};
		Advance();
		return term;
	}

	#endregion

	#region HELPERS

	// Anchors like [^, $, \b] are not quantifiable.
	private static Expr VerifyNoAnchor(Token quantifier, Expr argument)
	{
		if (argument is IAnchor)
		{
			throw Error(quantifier, $"Expect quantifiable token but got {argument}");
		}
		else
		{
			return argument;
		}
	}

	// Delete the quotes. "abc" becomes abc.
	private static Literal Literal(Token token) =>
		new(token.Lexeme[1..^1]);

	private bool Match(params TokenType[] types)
	{
		if (Array.Exists(types, Check))
		{
			Advance();
			return true;
		}
		else
		{
			return false;
		}
	}

	private Token Consume(TokenType type, string message)
	{
		if (Check(type))
		{
			return Advance();
		}
		else
		{
			throw Error(Peek(), message);
		}
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

	private Token PeekNext() =>
		tokens[current + 1];

	private Token Previous() =>

		tokens[current - 1];

	private bool IsAtEnd() =>
	  Peek().Type == TokenType.EOF;

	private static ParsingException Error(Token token, string message) =>
		new(token, message);

	#endregion
}