namespace SimpleRegex.Parsing;

public static class TypeExtensions
{
	public static string SimpleName(this Type type) =>
		type.Name.Split('.')[^1].ToUpperInvariant();
}
