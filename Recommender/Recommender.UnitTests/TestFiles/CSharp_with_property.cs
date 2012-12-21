namespace Test {
	public class TestClass {
		private int foo;
		
		public TestClass() {
			foo = 42;
		}
		
		public string Bar {get; set;}
		
		public void DoStuff(string theStuff, int count) {
			System.Console.WriteLine(theStuff);
		}
		
		private int PrivateStuff(int count) {
			for(int i = 0; i < count; i++) {
				System.Console.Beep();
			}
			return 0;
		}
	}
}
