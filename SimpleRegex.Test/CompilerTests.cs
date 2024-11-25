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

	[TestMethod]
	public void Compile_SimpleExpression()
	{
		var input = """
			"a" or "b"
			""";

		var output = Compiler.Compile(input);

		AssertSuccess(output, "a|b");
	}

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
