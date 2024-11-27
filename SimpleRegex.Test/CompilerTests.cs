using FluentAssertions;
using System.Runtime.Serialization;

namespace SimpleRegex.Test;

[TestClass]
public class CompilerTests
{

	[DataTestMethod]
	[DataRow("start $", "Unexpected character '$' at line 1.")]
	[DataRow("start or \"hey", "Unterminated string at line 1.")]
	[DataRow("start\nor \n INVALID", "Unexpected identifier 'INVALID' at line 3.")]
	[DataRow("start /or end", "Expected second '/' at line 1.")]
	public void Compile_ScanningError(string input, string error) =>
		AssertFailure<ScanningException>(Compiler.Compile(input), $"Scanning Error: {error}");

	// TODO: Handle most or all the "Consume" invocations.
	[DataTestMethod]
	[DataRow("lazy(any)", "Expect quantifier after '(' at token [LEFT_PAREN] '(' at line 1.")]
	[DataRow("exactly(start, 42)", "Expect quantifiable token but got START at token [EXACTLY] 'exactly' at line 1.")]
	[DataRow("atleast(end, 42)", "Expect quantifiable token but got END at token [AT_LEAST] 'atleast' at line 1.")]
	[DataRow("between(boundary, 3, 42)", "Expect quantifiable token but got BOUNDARY at token [BETWEEN] 'between' at line 1.")]
	[DataRow("maybe(start)", "Expect quantifiable token but got START at token [MAYBE] 'maybe' at line 1.")]
	[DataRow("maybemany(end)", "Expect quantifiable token but got END at token [MAYBE_MANY] 'maybemany' at line 1.")]
	[DataRow("many(boundary)", "Expect quantifiable token but got BOUNDARY at token [MANY] 'many' at line 1.")]
	[DataRow("anyof(start)", "Expect Term at token [START] 'start' at line 1.")]
	[DataRow("match(whitespace, \"name\")", "'match' cannot specify a 'name' argument because it is not a 'capture' at token [MATCH] 'match' at line 1.")]
	[DataRow("notmatch(whitespace, \"name\")", "'notMatch' cannot specify a 'name' argument because it is not a 'capture' at token [NOT_MATCH] 'notmatch' at line 1.")]
	public void Compile_ParsingError(string input, string error) =>
		AssertFailure<ParsingException>(Compiler.Compile(input), $"Parsing Error: {error}");

	// TODO: Cover all the cases.
	[DataTestMethod]
	[DataRow("", "")]
	public void Compile_InterpreterError(string input, string error) =>
	AssertFailure<InterpreterException>(Compiler.Compile(input), $"Intepretation Error: {error}");

	[DataTestMethod]
	[DataRow("\"a\" or \"b\"", "a|b")]
	[DataRow("\"a\" + \"b\"", "ab")]
	[DataRow("any", ".")]
	[DataRow("start", "^")]
	[DataRow("end", "$")]
	[DataRow("ws", "\\s")]
	[DataRow("whitespace", "\\s")]
	[DataRow("digit", "\\d")]
	[DataRow("notdigit", "\\D")]
	[DataRow("word", "\\w")]
	[DataRow("notword", "\\W")]
	[DataRow("boundary", "\\b")]
	[DataRow("newline", "\\n")]
	[DataRow("nl", "\\n")]
	[DataRow("cr", "\\r")]
	[DataRow("tab", "\\t")]
	[DataRow("null", "\\0")]
	[DataRow("quote", "\"\"")]
	public void Compile_SimpleExpression(string input, string output) =>
		AssertSuccess(Compiler.Compile(input), output);

	[DataTestMethod]
	[DataRow("maybe(\"a\")", "a?")]
	[DataRow("maybeMany(\"ab\")", "(ab)*")]
	[DataRow("many(\"a\")", "a+")]
	public void Compile_SimpleQuantifier(string input, string output) =>
		AssertSuccess(Compiler.Compile(input), output);

	[DataTestMethod]
	[DataRow("exactly(\"a\", 42)", "a{42}")]
	[DataRow("atLeast(\"ab\", 42)", "(ab){42,}")]
	[DataRow("between(\"abc\", 3, 42)", "(abc){3,42}")]
	public void Compile_PreciseQuantifier(string input, string output) =>
		AssertSuccess(Compiler.Compile(input), output);

	[DataTestMethod]
	[DataRow("lazy(maybe(\"ab\"))", "(ab)??")]
	[DataRow("lazy(maybeMany(\"a\"))", "a*?")]
	[DataRow("lazy(many(\"abc\"))", "(abc)+?")]
	[DataRow("lazy(exactly(\"a\", 42))", "a{42}?")]
	[DataRow("lazy(atLeast(\"ab\", 42))", "(ab){42,}?")]
	[DataRow("lazy(between(\"abc\", 3, 42))", "(abc){3,42}?")]
	public void Compile_LazyQuantifier(string input, string output) =>
		AssertSuccess(Compiler.Compile(input), output);

	[DataTestMethod]
	[DataRow("anyOf(\"a\")", "[a]")]
	[DataRow("notAnyOf(\"-\")", "[^\\-]")]
	[DataRow("anyOf(\"^\")", "[\\^]")]
	[DataRow("notAnyOf(ws)", "[^\\s]")]
	[DataRow("anyOf(digit)", "[\\d]")]
	[DataRow("notAnyOf(notDigit)", "[^\\D]")]
	[DataRow("anyOf(word)", "[\\w]")]
	[DataRow("notAnyOf(notWord)", "[^\\W]")]
	[DataRow("anyOf(nl)", "[\\n]")]
	[DataRow("notAnyOf(newline)", "[^\\n]")]
	[DataRow("anyOf(cr)", "[\\r]")]
	[DataRow("notAnyOf(tab)", "[^\\t]")]
	[DataRow("anyOf(quote)", "[\"\"]")]
	[DataRow("notAnyOf(range(\"a\",\"z\"))", "[^a-z]")]
	[DataRow("anyOf(range(\"a\",\"z\"), \"42\")", "[a-z42]")]
	[DataRow("notAnyOf(\"hey_\", range(\"0\",\"5\"), \"_you_\", range(\"a\",\"z\"), \"_there\")", "[^hey_0-5_you_a-z_there]")]
	public void Compile_CharacterClass(string input, string output) =>
		AssertSuccess(Compiler.Compile(input), output);

	[DataTestMethod]
	[DataRow("capture(boundary)", "(\\b)")]
	[DataRow("capture(\"abc\", \"Name_42\")", "(?<Name_42>abc)")]
	[DataRow("match(any + newline)", "(?:.\\n)")]
	[DataRow("notmatch(\"abc\" or digit)", "(?!abc|\\d)")]
	public void Compile_GroupConstruct(string input, string output) =>
		AssertSuccess(Compiler.Compile(input), output);

	// TODO: Think of more complex scenarios, e.g. the start of groups or character classes not being escaped, etc.
	[TestMethod]
	public void Compile_ComplexExpression()
	{
		var input = """
			start + between("hello there", 42, 69)
			or lazy(exactly("12", 3))
			or many(any + quote or boundary)
			""";

		var output = Compiler.Compile(input);

		AssertSuccess(output, @"^(hello there){42,69}|(12){3}?|(.""""|\b)+");
	}

	[TestMethod]
	public void Compile_CommentsIgnored()
	{
		var input = """
			// This is a comment 1
			"a"
			// This is a comment 2
			or
			// This is a comment "b" or
			"c"
			// This is a comment 3
			""";

		var output = Compiler.Compile(input);

		AssertSuccess(output, "a|c");
	}

	private static void AssertSuccess(CompilationResult result, string regex)
	{
		result.Should().NotBeNull();
		result.Success.Should().BeTrue();
		result.Exception.Should().BeNull();

		result.Tokens.Should().NotBeNullOrEmpty();
		result.Tree.Should().NotBeNull();
		result.Regex.Should().Be(regex);
	}

	private static void AssertFailure<TException>(CompilationResult result, string exceptionMessage)
	{
		result.Should().NotBeNull();
		result.Success.Should().BeFalse();
		result.Tokens.Should().BeNull();
		result.Tree.Should().BeNull();
		result.Regex.Should().BeNull();
		result.Exception.Should().BeOfType<TException>();
		result.Exception.Message.Should().Be(exceptionMessage);
	}
}
