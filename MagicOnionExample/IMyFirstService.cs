using System;
using System.Collections.Generic;
using System.Text;
using MagicOnion;

namespace MagicOnionExample
{
	public interface IMyFirstService : IService<IMyFirstService>
	{
		// Return type must be `UnaryResult<T>` or `Task<UnaryResult<T>>`.
		// If you can use C# 7.0 or newer, recommend to use `UnaryResult<T>`.
		UnaryResult<Class1> Get(string name);
	}
}
