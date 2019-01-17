using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Grpc.Core;
using MagicOnion.Client;
using MessagePack;

namespace MagicOnionExample
{
	public class ClientGenerator
	{
		public static T GenerateClient<T>(Channel channel)
			where T : class
		{
			var baseType = typeof(T);

			Type iface = InterfaceGenerator.GenerateInterface(baseType);

			var meth = typeof(MagicOnionClient).GetMethod(nameof(MagicOnionClient.Create), new[] { typeof(Channel) });
			meth = meth.MakeGenericMethod(iface);
			var dynClient = meth.Invoke(null, new[] { channel });

			AssemblyName assemblyName = new AssemblyName("MagicOnionClient.DynamicClients");
			string moduleName = String.Format("{0}.dll", assemblyName.Name);

			var assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
			var module = assembly.DefineDynamicModule(moduleName);

			var type = module.DefineType(baseType.Name + "Client",
				TypeAttributes.Public,
				baseType);

			// Поле для хранения сгенерированного клиента MagicOnion
			// private readonly InterfaceType _client;
			var fld = type.DefineField("_client", iface, FieldAttributes.Private | FieldAttributes.InitOnly);

			// public ctor(InterfaceType client)
			var ctor = type.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { iface });
			var il = ctor.GetILGenerator();
			/*
			 * : base()
			 * {
			 *	_client = client;
			 * }
			 */
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Call, type.BaseType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null));
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Stfld, fld);
			il.Emit(OpCodes.Ret);

			foreach (var m in baseType.GetMethods())
			{
				if (!m.IsAbstract)
					continue;

				Type[] paramTypes = m.GetParameters().Select(p => p.ParameterType).ToArray();
				var realMet = dynClient.GetType().GetMethod(m.Name, paramTypes);

				var met = type.DefineMethod(m.Name,
					MethodAttributes.Public | MethodAttributes.ReuseSlot | MethodAttributes.Virtual | MethodAttributes.HideBySig,
					CallingConventions.HasThis,
					m.ReturnType,
					paramTypes
				);
				il = met.GetILGenerator();

				// var unaryResult = _client.MethodName(param1, param2, ...)
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldfld, fld);
				int paramCnt = m.GetParameters().Length;
				for (int j = 0; j < paramCnt; j++)
				{
					il.Emit(OpCodes.Ldarg, j + 1);
				}
				il.Emit(OpCodes.Call, realMet);

				// var res = unaryResult.GetResultSync();
				Type retType = realMet.ReturnType;
				Type underlyingType = retType.GenericTypeArguments[0];
				var waitMeth = typeof(UnaryResultExt).GetMethod(nameof(UnaryResultExt.GetResultSync));
				waitMeth = waitMeth.MakeGenericMethod(underlyingType);
				il.Emit(OpCodes.Call, waitMeth);

				if (m.ReturnType == typeof(void))
					il.Emit(OpCodes.Pop);

				il.Emit(OpCodes.Ret);

				type.DefineMethodOverride(met, m);
			}

			var resType = type.CreateTypeInfo().AsType();
			return Activator.CreateInstance(resType, dynClient) as T;
		}
	}
}
