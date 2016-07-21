using System;
using System.Collections.Generic;

namespace XamTest
{
	public class TestObject
	{
		public TestObject()
		{
			Test1 = 5;
			Test2 = "berksGwapo";
			Test3 = 9.5f;
			Test4 = new Dictionary<string,object> []
			{ 
				new Dictionary<string,object>(),
				new Dictionary<string,object>()
			};

			Test4[0].Add("test5", "test 5");
			Test4[1].Add("test6", "test 6");
		}

		int test1;
		string test2;
		float test3;
		Dictionary<string, object>[] test4;

		public int Test1
		{
			get
			{
				return test1;
			}

			set
			{
				test1 = value;
			}
		}

		public string Test2
		{
			get
			{
				return test2;
			}

			set
			{
				test2 = value;
			}
		}

		public float Test3
		{
			get
			{
				return test3;
			}

			set
			{
				test3 = value;
			}
		}

		public Dictionary<string, object>[] Test4
		{
			get
			{
				return test4;
			}

			set
			{
				test4 = value;
			}
		}
		//Dictionary<string,object>[] test4;
	}
}

