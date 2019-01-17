using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq.Expressions;
using MagicOnionExample;
using MagicOnion.Server;
using MagicOnion;

namespace Server
{
	public class ServiceGenerator
	{
		public static Type GenerateService(Type typeForMethods, Type typeRealization)
		{
			var interfaceType = InterfaceGenerator.GenerateInterface(typeForMethods);

			AssemblyName assemblyName = new AssemblyName("MagicOnionServer.DynamicServices");
			string moduleName = String.Format("{0}.dll", assemblyName.Name);

			var assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
			var module = assembly.DefineDynamicModule(moduleName);

			var type = module.DefineType("DynamicMyFirstService", 
				TypeAttributes.Public,
				typeof(ServiceBase<>).MakeGenericType(interfaceType),
				new[] { interfaceType });

			var fld = type.DefineField("_service", typeRealization, FieldAttributes.Private | FieldAttributes.InitOnly);
			var ctor = type.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
			var il = ctor.GetILGenerator();

			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Call, type.BaseType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null));
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Newobj, typeRealization.GetConstructor(Type.EmptyTypes));
			il.Emit(OpCodes.Stfld, fld);
			il.Emit(OpCodes.Ret);

			foreach (var m in interfaceType.GetMethods())
			{
				if (!m.IsAbstract)
					continue;

				var realMet = typeRealization.GetMethod(m.Name);
				// TODO Добавить проверку на наличие метода и перегрузки

				var met = type.DefineMethod(m.Name,
					MethodAttributes.Public | MethodAttributes.ReuseSlot | MethodAttributes.Virtual, //| MethodAttributes.HideBySig,
					CallingConventions.HasThis,
					m.ReturnType,
					m.GetParameters().Select(p => p.ParameterType).ToArray()
				);
				il = met.GetILGenerator();
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldfld, fld);
				int paramCnt = m.GetParameters().Length;
				for (int j = 0; j < paramCnt; j++)
				{
					il.Emit(OpCodes.Ldarg, j + 1);
				}
				il.Emit(OpCodes.Call, realMet);

				Type retType = realMet.ReturnType;
				if (!retType.IsGenericType || retType.GetGenericTypeDefinition() != typeof(UnaryResult<>))
				{
					if (retType == typeof(void))
					{
						retType = typeof(MessagePack.Nil);
						il.Emit(OpCodes.Ldsfld, retType.GetField(nameof(MessagePack.Nil.Default)));
					}
					retType = typeof(UnaryResult<>).MakeGenericType(retType);
					il.Emit(OpCodes.Newobj, retType.GetConstructor(new[] { retType.GenericTypeArguments[0] }));
				}

				il.Emit(OpCodes.Ret);
			}

			return type.CreateTypeInfo().AsType();
		}
	}
}
