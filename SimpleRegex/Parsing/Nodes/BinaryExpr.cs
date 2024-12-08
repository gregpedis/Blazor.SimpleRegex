namespace SimpleRegex.Parsing.Nodes;

public class BinaryExpr<TLeft, TRight>(TLeft left, TRight right) : Expr
{
	public TLeft Left { get; } = left;
	public TRight Right { get; } = right;

	public override string ToString() =>
		$"{GetType().SimpleName()} ({Left}, {Right})";
}

public class Execution(List<Assignment> assignments, Expr expression) : BinaryExpr<List<Assignment>, Expr>(assignments, expression);
public class Assignment(string identifier, Expr value) : BinaryExpr<string, Expr>(identifier, value);

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
		$"{GetType().SimpleName()} ({Value} ['{Min} -'{Max}'])";
}
