using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xaml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectSchemaEvolver.UnitTests
{
	[TestClass]
	public class Case_01
	{
		string _source = @"<Case1 xmlns='test'>
	<Case1.Items>
		<Case1Item Name='Peter' />
		<Case1Item Name='Alex' />
	</Case1.Items>
</Case1>
";

		Case1 Load()
		{
			return (Case1)XamlServices.Parse(_source);
		}

		[TestMethod]
		public void Should_evolve()
		{
			_source = new Case1Evolver().UpgradeDatabaseXaml(_source);
			Trace.WriteLine(_source);

			var item = Load();

			Assert.AreEqual("Peter", item.Items2[0].FirstName);
			Assert.AreEqual(DateTime.UtcNow.Date, item.Items2[0].CreatedDate);
			Assert.AreEqual("Alex", item.Items2[1].FirstName);
			Assert.AreEqual(DateTime.UtcNow.Date, item.Items2[1].CreatedDate);
		}

	}

	public class Case1Evolver : ReflectionEvolver
	{
		public static void Upgrade_0_to_5_rename_field(dynamic state)
		{
			foreach (var item in state.Items)
			{
				item.FirstName = item.Name;
				item.Name = null;
			}
		}

		public static void Upgrade_5_to_10_add_required_field(dynamic state)
		{
			foreach (var item in state.Items)
			{
				item.CreatedDate = DateTime.UtcNow.Date;
			}
		}
		public static void Upgrade_10_to_15_rename_col(dynamic state)
		{
			state.Items2 = state.Items;
			state.Items = null;
		}
	}

	public class Case1 //: AttachedPropertyStore
	{
		public decimal Version { get; set; }

		public List<Case1Item> Items2 { get; } = new List<Case1Item>();
	}

	public class Case1Item
	{
		public string FirstName { get; set; }
		public DateTime CreatedDate { get; set; }
	}
}

