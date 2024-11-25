using FluentAssertions;

namespace SimpleRegex.Test;

[TestClass]
public class CompilerTests
{
	[TestMethod]
	public void Compile_ScanningError()
	{
		var input = """
			start or
			INVALID
			""";

		var output = Compiler.Compile(input);

		AssertFailure<ScanningException>(output, "Scanning Error: Unexpected identifier 'INVALID' at line 2.");
	}

	[TestMethod]
	public void Compile_ParsingError()
	{
		var input = "lazy(any)";

		var output = Compiler.Compile(input);

		AssertFailure<ParsingException>(output, "Parsing Error: Expect quantifier after '(' at token [LEFT_PAREN] '(' #1.");
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
