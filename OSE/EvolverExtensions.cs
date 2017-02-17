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
	}
}
