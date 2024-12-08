namespace SimpleRegex.Scanning;

public enum TokenType
{
	// Single character tokens.
	LEFT_PAREN, RIGHT_PAREN, COMMA, CONCAT, EOF,
	// Escaped tokens.
	WHITESPACE, DIGIT, NOT_DIGIT, WORD, NOT_WORD, BOUNDARY, NOT_BOUNDARY, NEWLINE, CR, TAB, NULL, QUOTE,
	// Literals.
	LITERAL, NUMBER,
	// Keywords.
	OR, ANY, START, END,
	MAYBE, MAYBE_MANY, MANY, LAZY,
	EXACTLY, AT_LEAST, BETWEEN,
	RANGE, ANY_OF, NOT_ANY_OF, CAPTURE, MATCH, NOT_MATCH,
	// Assignments.
	IDENTIFIER, EQUALS,
}