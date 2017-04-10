using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CSharp.RuntimeBinder;

namespace ObjectSchemaEvolver
{
	public class ReflectionEvolver<T> : ReflectionEvolver, IEvolver<T>
	{
		public virtual void InitNew(T newRoot)
		{
			
		}
	}

	public class ReflectionEvolver : IEvolver
	{
		public string VersionFieldName { get; set; } = "Version";

		static readonly Regex _rx = new Regex(@"Upgrade_(?'from'\d+(_\d+)?)_to_(?'to'\d+(_\d+)?)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
		//static Regex _rx = new Regex(@"Upgrade_(?'from'\d+)_to_(?'to'\d+)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);
		public byte[] UpgradeDatabase(byte[] database, Func<byte[], dynamic> dynamer/*, Func<dynamic, byte[]> store*/, Action<byte[]> intermediateDump = null)
		{
			var store = new Func<dynamic, byte[]>(d => d.StoreBack());
			var state = dynamer(database);
			var verGetter = new Func<dynamic, string>(s => s[VersionFieldName].ToString(CultureInfo.InvariantCulture)); //BuildDynamicGetter(state.GetType(), VersionFieldName);
			var verSetter = new Action<dynamic, string>((s,v)=>s[VersionFieldName] = v); //BuildDynamicSetter(state.GetType(), VersionFieldName);

			decimal version;

			var verString = verGetter(state);
			if (verString == null)
			{
				version = 0m;
				verSetter(state, "0");
			}
			else
			{
				version = decimal.Parse(verString, CultureInfo.InvariantCulture);
			}

			var levels = LoadLevels();
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
				verSetter(state, level.To.ToString(CultureInfo.InvariantCulture));
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

		private List<LevelEvolver> _levels;

		private List<LevelEvolver> LoadLevels()
		{
			if (_levels != null)
			{
				return _levels;
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
			return _levels = levels;
		}

		public decimal LatestVersion
		{
			get { return LoadLevels().Last().To; }
		}

		private static Func<object, object> BuildDynamicGetter(Type targetType, string propertyName)
		{
			var rootParam = Expression.Parameter(typeof(object));
			var propBinder = Microsoft.CSharp.RuntimeBinder.Binder.GetMember(CSharpBinderFlags.None, propertyName, targetType, new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
			DynamicExpression propGetExpression = Expression.Dynamic(propBinder, typeof(object),
				Expression.Convert(rootParam, targetType));
			Expression<Func<object, object>> getPropExpression = Expression.Lambda<Func<object, object>>(propGetExpression, rootParam);
			return getPropExpression.Compile();
		}
		private static Action<object, object> BuildDynamicSetter(Type targetType, string propertyName)
		{
			var rootParam = Expression.Parameter(typeof(object));
			var valueParam = Expression.Parameter(typeof(object));
			var propBinder = Microsoft.CSharp.RuntimeBinder.Binder.SetMember(CSharpBinderFlags.None, propertyName, targetType, new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
			DynamicExpression propSetExpression = Expression.Dynamic(propBinder, typeof(object), Expression.Convert(rootParam, targetType), Expression.Convert(valueParam, typeof(object)));
			Expression<Action<object, object>> setPropExpression = Expression.Lambda<Action<object, object>>(propSetExpression, rootParam, valueParam);
			return setPropExpression.Compile();
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