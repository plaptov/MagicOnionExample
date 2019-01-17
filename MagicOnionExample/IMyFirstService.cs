using System;
using System.Collections.Generic;
using System.Text;
using MagicOnion;

namespace MagicOnionExample
{
	public abstract class MyFirstServiceBase
	{
		public abstract Class1 Get(string name, string[] strings, DateTime dt);

		public abstract void Set(string msg);
	}
}
