using MagicOnion;
using MagicOnionExample;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
	public class InternalServiceRealization : MyFirstServiceBase
	{
		// You can use async syntax directly.
		public override Class1 Get(string name, string[] strings, DateTime dt)
		{
			return new Class1()
			{
				Name = name,
				Strings = strings,
				Ints = new HashSet<int>() { 123, 456, 789 },
				Dict = new Dictionary<int, string[]>()
				{
					{ 1, new[] { "a", "b" } },
					{ 2, new[] { "c", "d" } },
					{ 3, new[] { "e", "f" } },
				},
				Dates = new[]
				{
					dt,
					new DateTime(2000, 01, 01, 0, 0, 0, DateTimeKind.Local),
					new DateTime(2000, 01, 01, 0, 0, 0, DateTimeKind.Unspecified),
					new DateTime(2000, 01, 01, 0, 0, 0, DateTimeKind.Utc),
				}
			};
		}

		public override void Set(string msg)
		{
			Console.WriteLine(msg);
		}

	}
}
