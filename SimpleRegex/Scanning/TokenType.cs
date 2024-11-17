namespace SimpleRegex.Scanning;

public enum TokenType
{
	// Single character tokens.
	LEFT_PAREN, RIGHT_PAREN, COMMA, CONCAT, EOF,
	// Escaped tokens.
	WHITESPACE, DIGIT, WORD, BOUNDARY, NEWLINE, CR, TAB, NULL,
	// Literals.
	LITERAL, NUMBER,
	// Keywords.
	OR, ANY, EXACTLY, AT_LEAST, BETWEEN, START, END,
	MAYBE, MAYBE_MANY, MANY, LAZY,
}


// "abc"					-> abc
// ()						-> precedence
// +						-> "ab" + "c" -> "abc"
// or						-> |

// any						-> .
// start					-> ^
// end						-> $

// maybe("abc")				-> (abc)?
// maybeMany("abc")			-> (abc)*
// many("abc")				-> (abc)+
// lazy(many("abc"))		-> (abc)+?

// exactly("abc", 3)		-> (abc){3}
// atLeast("abc", 3)		-> (abc){3,}
// between("abc", 3, 6)		-> (abc){3,6}

// whitespace, ws			-> \s
// digit					-> \d
// word						-> \w
// boundary					-> \b
// newline,ln				-> \n
// cr						-> \r
// tab						-> \t
// null						-> \0

// ---

// anyOf "abc"				-> [abc]
// range, rangeof "az", "AZ"-> [a-zA-Z]
// ???						-> [a-zA]

// ---

// not
// match(..)				-> (?:)
// capture(..) 				-> (..)

// ---

// EXAMPLE:
// start or maybe( (exactly(3, "12")) or many("34") )


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
