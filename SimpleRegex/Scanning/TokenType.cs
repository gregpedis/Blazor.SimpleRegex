namespace SimpleRegex.Scanning;

public enum TokenType
{
	// Single character tokens.
	LEFT_PAREN, RIGHT_PAREN, COMMA, CONCAT, EOF,
	// Escaped tokens.
	WHITESPACE, DIGIT, NOT_DIGIT, WORD, NOT_WORD, BOUNDARY, NEWLINE, CR, TAB, NULL, QUOTE,
	// Literals.
	LITERAL, NUMBER,
	// Keywords.
	OR, ANY, EXACTLY, AT_LEAST, BETWEEN, START, END,
	MAYBE, MAYBE_MANY, MANY, LAZY,
	RANGE, ANY_OF, NOT_ANY_OF, CAPTURE, MATCH, NOT_MATCH,
}

// "abc"						-> abc
// ()							-> precedence
// +							-> "ab" + "c" -> "abc"
// or							-> |

// any							-> .
// start						-> ^
// end							-> $

// whitespace, ws				-> \s
// digit						-> \d
// notdigit						-> \D
// word							-> \w
// notword						-> \W
// boundary						-> \b
// newline,ln					-> \n
// cr							-> \r
// tab							-> \t
// null							-> \0
// quote						-> ""

// maybe("abc")					-> (abc)?
// maybeMany("abc")				-> (abc)*
// many("abc")					-> (abc)+

// exactly("abc", 3)			-> (abc){3}
// atLeast("abc", 3)			-> (abc){3,}
// between("abc", 3, 6)			-> (abc){3,6}

// lazy(many("abc"))			-> (abc)+?

// range("a","z")				-> a-z
// anyOf("abc")					-> [abc]
// notAnyOf("abc")				-> [^abc]
// anyof(range("a", "z"), "A")	-> [a-zA]

// capture("abc", "name")		-> (?'name'abc) or (?<name>abc)
// capture("abc") 				-> (abc)
// match("abc")					-> (?:abc)
// notMatch("abc")				-> (?!abc)

// ---

// EXAMPLE:
// start or maybe( (exactly("hello", 12)) or many("34") )


/*
+---+----------------------------------------------------------+
|   |             ERE Precedence (from high to low)            |
+---+----------------------------------------------------------+
| 2 | Escaped characters                | \<special character> |
| 3 | Bracket expression                | []                   |
| 4 | Grouping                          | ()                   |
| 5 | Single-character-ERE duplication  | * + ? {m,n}          |
| 7 | Anchoring                         | ^ $                  |
| 7 | Concatenation (custom)			| "ab" + "cd"          |
| 8 | Alternation                       | |                    |
+---+-----------------------------------+----------------------+
*/
