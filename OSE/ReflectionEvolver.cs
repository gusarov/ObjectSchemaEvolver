using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ObjectSchemaEvolver
{
	public class ReflectionEvolver : IEvolver
	{
		static readonly Regex _rx = new Regex(@"Upgrade_(?'from'\d+(_\d+)?)_to_(?'to'\d+(_\d+)?)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
		//static Regex _rx = new Regex(@"Upgrade_(?'from'\d+)_to_(?'to'\d+)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
		public byte[] UpgradeDatabase(byte[] database, Func<byte[], dynamic> dynamer/*, Func<dynamic, byte[]> store*/, Action<byte[]> intermediateDump = null)
		{
			var store = new Func<dynamic, byte[]>(d => d.StoreBack());
			var state = dynamer(database);

			decimal version;

			if (state.Version == null)
			{
				state.Version = version = 0m;
			}
			else
			{
				version = decimal.Parse(state.Version, CultureInfo.InvariantCulture);
			}

			var methods = GetType().GetMethods();
			var levels = new List<LevelEvolver>();
			foreach (var item in methods)
			{
				var match = _rx.Match(item.Name);
				if (match.Success)
				{
					levels.Add(new LevelEvolver(
						decimal.Parse(match.Groups["from"].Value, CultureInfo.InvariantCulture)
						, decimal.Parse(match.Groups["to"].Value, CultureInfo.InvariantCulture)
						, item
					));
				}
			}
			levels.Sort();
			//verify
			for (int i = 1; i < levels.Count; i++)
			{
				if (levels[i].From != levels[i - 1].To)
				{
					throw new Exception($"Unexpected levels sequence: {levels[i - 1].From}=>{levels[i - 1].To} then {levels[i].From}=>{levels[i].To} ");
				}
			}
			bool changed = false;
			foreach (var level in levels)
			{
				if (level.From < version)
				{
					continue; // fast forward
				}
				if (level.From > version)
				{
					throw new Exception($"There is no upgrade method for current schema version {version}");
				}
				level.MethodInfo.Invoke(this, new object[] { state });
				state.Version = level.To;
				version = level.To;
				if (intermediateDump != null)
				{
					intermediateDump(store(state));
				}
				changed = true;
			}
			if (changed)
			{
				return store(state);
			}
			return database; // unchanged
		}
		class LevelEvolver : IComparable<LevelEvolver>
		{
			public decimal From, To;
			public MethodInfo MethodInfo;
			public LevelEvolver(decimal from, decimal to, MethodInfo mi)
			{
				From = from;
				To = to;
				MethodInfo = mi;
			}
			public int CompareTo(LevelEvolver other)
			{
				return From.CompareTo(other.From);
			}
		}
	}
}