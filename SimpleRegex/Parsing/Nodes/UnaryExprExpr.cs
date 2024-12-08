namespace SimpleRegex.Parsing.Nodes;

public class UnaryExpr<T>(T value) : Expr
{
	public T Value { get; } = value;

	public override string ToString() =>
		$"{GetType().SimpleName()} ({Value})";
}

public class UnaryExprExpr(Expr value) : UnaryExpr<Expr>(value);

public class Capture(Expr value) : UnaryExprExpr(value);
public class Match(Expr value) : UnaryExprExpr(value);
public class NotMatch(Expr value) : UnaryExprExpr(value);

public class Lazy(Expr value) : UnaryExprExpr(value);

public class Maybe(Expr value) : UnaryExprExpr(value);
public class MaybeMany(Expr value) : UnaryExprExpr(value);
public class Many(Expr value) : UnaryExprExpr(value);

public class UnaryExprString(string value) : UnaryExpr<string>(value);

public class Identifier(string value) : UnaryExprString(value);
public class Literal(string value) : UnaryExprString(value);
