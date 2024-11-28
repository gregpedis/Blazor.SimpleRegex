# SimpleRegex

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=gregpedis_simpleregex&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=gregpedis_simpleregex)

[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=gregpedis_simpleregex&metric=coverage)](https://sonarcloud.io/summary/new_code?id=gregpedis_simpleregex)

[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=gregpedis_simpleregex&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=gregpedis_simpleregex)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=gregpedis_simpleregex&metric=bugs)](https://sonarcloud.io/summary/new_code?id=gregpedis_simpleregex)

A simple [recursive descent parser](https://en.wikipedia.org/wiki/Recursive_descent_parser) that compiles an expression to a [regex](https://en.wikipedia.org/wiki/Regular_expression).

## Syntax

The language is multi-line, case-insensitive, whitespace-insignificant and supports the following syntax.

#### Literals

```csharp
"abc"   => abc
```

Literals are automatically escaped. This means that characters like `\` and `^` that have special meaning in regex are transformed to `\\` and `\^` respectively.

#### Concatenation

```csharp
"abc" + "def"   => abcdef
```

Concatenation is done using the `+` operator, and is used to "squash" together expressions.

#### Alternation

```csharp
"abc" or "def"  => abc|def
```

Alternation has the lowest priority in the entire grammar, just like in regular expressions.

#### Simple Quantifier

```csharp
 maybe("abc")       => (abc)?
 maybeMany("abc")   => (abc)*
 many("abc")        => (abc)+
 ```

#### Precise Quantifier

```csharp
 exactly("abc", 3)      => (abc){42}
 atLeast("abc", 3)      => (abc){42,}
 between("abc", 3, 6)   => (abc){42,100}
 ```

 #### Lazy Quantifier

```csharp
lazy(many("abc"))   => (abc)+?
```

`lazy` can accept as its argument all the simple and precise quantifiers.

#### Character Class

```csharp
range("a","z")		            => a-z
anyof(range("a", "z"), "A")	    => [a-zA]
notAnyOf("abc_", range("A","Z"))    => [^abc_A-Z]
```

#### Group Construct

```csharp
capture("abc")          => (abc)
capture("abc", "name")  => (?<name>abc)
match("abc")            => (?:abc)
notMatch("abc")         => (?!abc)
```

### Special character

```
any             => .
start           => ^
end             => $
whitespace      => \s
ws		=> \s
digit	        => \d
notdigit        => \D
word	        => \w
notword         => \W
boundary        => \b
newline	        => \n
nl		=> \n
cr	        => \r
tab	        => \t
null	        => \0
quote	        => ""
```

## Grammar

The language accepts expressions that match the following grammar in [EBNF](https://en.wikipedia.org/wiki/Extended_Backus%E2%80%93Naur_form) notation:

```ebnf
expression              = or
or			= concat ("|" concat)*
concat			= lazy ("+" lazy)*
lazy			= "lazy" "(" quantifier ")" | quantifier | factor
quantifier		= simple_quantifier | precise_quantifier
simple_quantifier	= ("maybe" | "maybemany" | "many") "(" or ")"
precise_quantifier	= exactly_or_atleast | between
exactly_or_atleast	= ("exactly" | "atleast") "(" or "," number ")"
between			= "between" "(" or "," number "," number ")"
factor			= group_construct | character_class | term
group_construct		= match | capture
match			= ("match" | "notmatch") "(" or ")"
capture			= "capture" "(" or ("," literal)? ")"
character_class		= ("anyof" | "notanyof") "(" anyof_arg ("," anyof_arg)* ")"
anyof_arg		= range | character_term
range			= "range" "(" literal "," literal ")"
term			= character_term | non_character_term
character_term		= ws | digit | notdigit | word | notWord | nl | cr | tab | quote | literal
non_character_term	= any | start | end | boundary | null
```

## Examples

???

## TODO

- Add examples on readme.

- Link github readme to blazor web app.
- Move styling to app.css
- Add some more styling, e.g. red line when there is an error.

-  Add tokens and syntax tree visualization on the blazor web app. Maybe using [treeview](https://www.w3schools.com/howto/howto_js_treeview.asp) and some JS.
- Support assignments.