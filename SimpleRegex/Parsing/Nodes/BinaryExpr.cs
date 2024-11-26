namespace SimpleRegex.Parsing.Nodes;

public class BinaryExpr<TLeft, TRight>(TLeft left, TRight right) : Expr
{
	public TLeft Left { get; } = left;
	public TRight Right { get; } = right;

	public override string ToString() =>
		$"""
		{GetType().SimpleName()} (
			LEFT  => {left}
			RIGHT => {left}
		)
		""";
}

public class BinaryExpr(Expr left, Expr right) : BinaryExpr<Expr, Expr>(left, right);

public class NamedCapture(Expr value, Literal name) : BinaryExpr<Expr, Literal>(value, name);
public class Range(Literal left, Literal right) : BinaryExpr<Literal, Literal>(left, right);

public class Exactly(Expr value, int count) : BinaryExpr<Expr, int>(value, count);
public class AtLeast(Expr value, int count) : BinaryExpr<Expr, int>(value, count);

public class Between(Expr value, int min, int max) : Expr
{
	public Expr Value { get; } = value;
	public int Min { get; } = min;
	public int Max { get; } = max;

	public override string ToString() =>
		$"""
		{GetType().SimpleName()} (
			VALUE => {Value}
			FROM '{Min}' TO '{Max}' TIMES
		)
		""";
}
