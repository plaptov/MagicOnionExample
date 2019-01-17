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

			var type = ServiceGenerator.GenerateService(typeof(MyFirstServiceBase), typeof(InternalServiceRealization));

			MessagePack.MessagePackSerializer.SetDefaultResolver(MessagePack.Resolvers.TypelessContractlessStandardResolver.Instance);
			// setup MagicOnion and option.
			var service = MagicOnionEngine.BuildServerServiceDefinition(new[] { type },
			new MagicOnionOptions(true)
			{
				FormatterResolver = MessagePack.Resolvers.TypelessContractlessStandardResolver.Instance
			});

			var server = new Grpc.Core.Server
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

	/*public class MyFirstService : ServiceBase<IMyFirstService>, IMyFirstService
	{
		// You can use async syntax directly.
		public UnaryResult<Class1> Get(string name)
		{
			Logger.Debug($"Received:{name}");

			return new InternalServiceRealization().Get(name);
		}
	}*/
}
