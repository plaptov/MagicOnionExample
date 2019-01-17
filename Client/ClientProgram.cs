using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using MagicOnionExample;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
	class Program
	{
		static void Main(string[] args)
		{
			
			Console.Write("Press any key");
			Console.ReadKey();
			Console.WriteLine();
			MessagePackSerializer.SetDefaultResolver(MessagePack.Resolvers.TypelessContractlessStandardResolver.Instance);
			var channel = new Channel("localhost", 12345, ChannelCredentials.Insecure);

			var client = ClientGenerator.GenerateClient<MyFirstServiceBase>(channel);

			var result = client.Get(
				"not so cool name",
				new string[] { "abc", "def", "ghi" },
				DateTime.Now
			);
			foreach (var item in result.Dates)
			{
				Console.WriteLine(item);
			}

			client.Set("cool name");

			Console.Write("Press any key");
			Console.ReadKey();
		}
	}
}
