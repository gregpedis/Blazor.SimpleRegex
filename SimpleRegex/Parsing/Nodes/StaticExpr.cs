namespace SimpleRegex.Parsing.Nodes;

public class StaticExpr<T> : Expr where T : StaticExpr<T>, new()
{
	public static T Instance => new();

	public override string ToString() =>
		GetType().SimpleName();
}

public class Any : StaticExpr<Any>;
public class Start : StaticExpr<Start>;
public class End : StaticExpr<End>;

public class Whitespace : StaticExpr<Whitespace>;
public class Digit : StaticExpr<Digit>;
public class Word : StaticExpr<Word>;
public class Boundary : StaticExpr<Boundary>;
public class NewLine : StaticExpr<NewLine>;
public class Cr : StaticExpr<Cr>;
public class Tab : StaticExpr<Tab>;
public class Null : StaticExpr<Null>;

