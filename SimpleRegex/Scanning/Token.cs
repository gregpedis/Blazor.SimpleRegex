namespace SimpleRegex.Scanning;

public record class Token(TokenType Type, string Lexeme, int? Number, int Line)
{
	public override string ToString() =>
		$"[{Type}] '{Lexeme}' #{Line}";
}
