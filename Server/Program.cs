using System;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using MagicOnionExample;

namespace Server
{
	class Program
	{
		static void Main(string[] args)
		{
			GrpcEnvironment.SetLogger(new Grpc.Core.Logging.ConsoleLogger());

			MessagePack.MessagePackSerializer.SetDefaultResolver(MessagePack.Resolvers.ContractlessStandardResolver.Instance);
			// setup MagicOnion and option.
			var service = MagicOnionEngine.BuildServerServiceDefinition(new MagicOnionOptions(true)
			{
				FormatterResolver = MessagePack.Resolvers.ContractlessStandardResolver.Instance
			});

			var server = new global::Grpc.Core.Server
			{
				Services = { service },
				Ports = { new ServerPort("localhost", 12345, ServerCredentials.Insecure) }
			};

			// launch gRPC Server.
			server.Start();

			Console.WriteLine("Started");
			// and wait.
			Console.ReadLine();
		}
	}

	public class MyFirstService : ServiceBase<IMyFirstService>, IMyFirstService
	{
		// You can use async syntax directly.
		public async UnaryResult<Class1> Get(string name)
		{
			Logger.Debug($"Received:{name}");

			return new Class1()
			{
				Name = name,
				Strings = new[] { "abc", "def" },
				Ints = new System.Collections.Generic.HashSet<int>() { 123, 456, 789 },
				Dict = new System.Collections.Generic.Dictionary<int, string[]>()
				{
					{ 1, new[] { "a", "b" } },
					{ 2, new[] { "c", "d" } },
					{ 3, new[] { "e", "f" } },
				}
			};
		}
	}
}
