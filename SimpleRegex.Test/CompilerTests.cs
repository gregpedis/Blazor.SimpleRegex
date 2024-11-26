using FluentAssertions;

namespace SimpleRegex.Test;

[TestClass]
public class CompilerTests
{
	[DataTestMethod]
	[DataRow("start $", "Unexpected character '$' at line 1.")]
	[DataRow("start or \"hey", "Unterminated string at line 1.")]
	[DataRow("start\nor \n INVALID", "Unexpected identifier 'INVALID' at line 3.")]
	[DataRow("start /or end", "Expected second '/' at line 1.")]
	public void Compile_ScanningError(string input, string error)
	{
		var output = Compiler.Compile(input);
		AssertFailure<ScanningException>(output, $"Scanning Error: {error}");
	}

	[DataTestMethod]
	[DataRow("lazy(any)", "Expect quantifier after '(' at token [LEFT_PAREN] '(' at line 1.")]
	[DataRow("exactly(start, 42)", "Expect quantifiable token but got START at token [EXACTLY] 'exactly' at line 1.")]
	[DataRow("atleast(end, 42)", "Expect quantifiable token but got END at token [AT_LEAST] 'atleast' at line 1.")]
	[DataRow("between(boundary, 3, 42)", "Expect quantifiable token but got BOUNDARY at token [BETWEEN] 'between' at line 1.")]
	[DataRow("maybe(start)", "Expect quantifiable token but got START at token [MAYBE] 'maybe' at line 1.")]
	[DataRow("maybemany(end)", "Expect quantifiable token but got END at token [MAYBE_MANY] 'maybemany' at line 1.")]
	[DataRow("many(boundary)", "Expect quantifiable token but got BOUNDARY at token [MANY] 'many' at line 1.")]
	public void Compile_ParsingError(string input, string error)
	{
		var output = Compiler.Compile(input);
		AssertFailure<ParsingException>(output, $"Parsing Error: {error}");
	}

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

	[DataRow("maybe(\"a\")", "a?")]
	[DataRow("maybeMany(\"ab\")", "(ab)*")]
	[DataRow("many(\"a\")", "a+")]
	[DataRow("lazy(maybe(\"ab\"))", "(ab)??")]
	[DataRow("lazy(maybeMany(\"a\"))", "a*?")]
	[DataRow("lazy(many(\"abc\"))", "(abc)+?")]

	[DataRow("exactly(\"a\", 42)", "a{42}")]
	[DataRow("atLeast(\"ab\", 42)", "(ab){42,}")]
	[DataRow("between(\"abc\", 3, 42)", "(abc){3,42}")]

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

	[DataRow("capture(boundary)", "(\\b)")]
	[DataRow("capture(\"abc\", \"Name_42\")", "(?<Name_42>abc)")]
	[DataRow("match(any + newline)", "(?:.\\n)")]
	[DataRow("notmatch(\"abc\" or digit)", "(?!abc|\\d)")]
	public void Compile_SimpleExpression(string input, string output) =>
		AssertSuccess(Compiler.Compile(input), output);

	[TestMethod]
	public void Compile_ComplexExpression()
	{
		var input = """
			start + between("hello there", 42, 69)
			or lazy(exactly("12", 3))
			or many(any + quote or boundary)
			""";

		var output = Compiler.Compile(input);

		AssertSuccess(output,@"^(hello there){42,69}|(12){3}?|(.""""|\b)+");
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
