using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using MagicOnion;
using MagicOnionExample;

namespace MagicOnionExample
{
	public class InterfaceGenerator
	{
		public static Type GenerateInterface(Type typeForMethods)
		{
			AssemblyName assemblyName = new AssemblyName("MagicOnionExample.DynamicInterfaces");
			string moduleName = String.Format("{0}.dll", assemblyName.Name);

			var assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
			var module = assembly.DefineDynamicModule(moduleName);
			var type = module.DefineType("IMyFirstService",
				TypeAttributes.Interface |
				TypeAttributes.Abstract |
				TypeAttributes.Public);
			type.AddInterfaceImplementation(typeof(IService<>).MakeGenericType(type.UnderlyingSystemType));

			foreach (var met in typeForMethods.GetMethods())
			{
				if (!met.IsAbstract)
					continue;
				Type retType = met.ReturnType;
				if (!retType.IsGenericType || retType.GetGenericTypeDefinition() != typeof(UnaryResult<>))
				{
					if (retType == typeof(void))
						retType = typeof(MessagePack.Nil);
					retType = typeof(UnaryResult<>).MakeGenericType(retType);
				}

				type.DefineMethod(met.Name, 
					MethodAttributes.Abstract | MethodAttributes.Public | MethodAttributes.Virtual,
					retType, 
					met.GetParameters().Select(p => p.ParameterType).ToArray());
			}
			return type.CreateTypeInfo().AsType();
		}

	}
}
