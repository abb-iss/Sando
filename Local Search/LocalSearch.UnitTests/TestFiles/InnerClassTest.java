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
			if(this.a == Example.this.x)
			    b = 0;
			if(x == b)
				x++;
        }
    }

	public class Mix
	{
		public int c;
		Request obj;

		public void doSomething(int x)
		{
			obj.doSomething();		
		}

	}
}