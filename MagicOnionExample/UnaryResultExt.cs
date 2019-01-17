using System;
using System.Collections.Generic;
using System.Text;
using MagicOnion;

namespace MagicOnionExample
{
	public static class UnaryResultExt
	{
		public static T GetResultSync<T>(this UnaryResult<T> resultAsync)
		{
			return resultAsync.ResponseAsync.ConfigureAwait(true).GetAwaiter().GetResult();
		}
	}
}
