public class Example
{
    public void doSomething(int a, int b)
    {
    }

	public int x;

    public class Request
    {
        public int a;
        public int b;

        public void doSomething()
        {
			int x;
            Example.this.doSomething(a,b);
			Example.this.doSomething(x, Example.this.x);
			if(this.a == 0)
			    b = 0;
        }
    }
}