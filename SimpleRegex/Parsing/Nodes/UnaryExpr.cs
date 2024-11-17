namespace SimpleRegex.Parsing.Nodes;

public class UnaryExpr<T>(T value) : Expr
{
	public T Value { get; } = value;

	public override string ToString() =>
		$"{GetType().SimpleName()} {Value}";
}

public class UnaryExpr(Expr value) : UnaryExpr<Expr>(value);

public class Grouping(Expr value) : UnaryExpr(value);

public class Lazy(Expr value) : UnaryExpr(value);

public class Maybe(Expr value) : UnaryExpr(value);

public class MaybeMany(Expr value) : UnaryExpr(value);

public class Many(Expr value) : UnaryExpr(value);

public class Literal(string value) : UnaryExpr<string>(value)
{
	public override string ToString() =>
		$"{GetType().SimpleName()} '{Value}'";
}
