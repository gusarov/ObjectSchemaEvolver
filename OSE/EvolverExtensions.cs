using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectSchemaEvolver
{
	public static class EvolverExtensions
	{
		public static string UpgradeDatabaseXaml(this IEvolver evolver, string xaml)
		{
			return Encoding.UTF8.GetString(
				evolver.UpgradeDatabase(Encoding.UTF8.GetBytes(xaml)
					, b => new XamlDynamicData(Encoding.UTF8.GetString(b))
				));
		}

		public static T CreateRoot<T>(this IEvolver evolver, T root)
		{
			return InitRoot(evolver, root);
		}

		public static T CreateRoot<T>(this IEvolver evolver)
		{
			return InitRoot(evolver, Activator.CreateInstance<T>());
		}
		public static T CreateRoot<T>(this IEvolver<T> evolver, T root)
		{
			return InitRoot(evolver, root);
		}

		public static T CreateRoot<T>(this IEvolver<T> evolver)
		{
			return InitRoot(evolver, Activator.CreateInstance<T>());
		}

		private static T InitRoot<T>(IEvolver evolver, T instance)
		{
			var re = evolver as ReflectionEvolver;
			var reg = evolver as ReflectionEvolver<T>;
			if (re != null)
			{
				var versionPi = instance.GetType().GetProperty(re.VersionFieldName);
				if (versionPi == null)
				{
					throw new Exception($"Root model type '{typeof(T)}' don't have a version property '{re.VersionFieldName}'");
				}
				versionPi.SetValue(instance, re.LatestVersion, null);
			}
			reg?.InitNew(instance);
			return instance;
		}
	}
}
