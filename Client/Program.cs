using Grpc.Core;
using MagicOnion.Client;
using MagicOnionExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
	class Program
	{
		static async Task Main(string[] args)
		{
			Console.Write("Press any key");
			Console.ReadKey();
			MessagePack.MessagePackSerializer.SetDefaultResolver(MessagePack.Resolvers.ContractlessStandardResolver.Instance);
			var channel = new Channel("localhost", 12345, ChannelCredentials.Insecure);

			// get MagicOnion dynamic client proxy
			var client = MagicOnionClient.Create<IMyFirstService>(channel);

			// call method.
			var result = await client.Get("cool name");
			Console.WriteLine("Client Received:" + result);
		}
	}
}
