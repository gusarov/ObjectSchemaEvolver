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
		string _source = @"<Case1 xmlns='test' Version='1'>
	<Case1.Items>
		<Case1Item Name='Bob' />
		<Case1Item Name='Alice' />
	</Case1.Items>
</Case1>
";

		[TestMethod]
		public void Should_evolve()
		{
			_source = new Case1Evolver().UpgradeDatabaseXaml(_source);
			Trace.WriteLine(_source);

			var item = (Case1)XamlServices.Parse(_source);

			Assert.AreEqual("Bob", item.Items2[0].FirstName);
			Assert.AreEqual(DateTime.UtcNow.Date, item.Items2[0].CreatedDate);
			Assert.AreEqual("Alice", item.Items2[1].FirstName);
			Assert.AreEqual(DateTime.UtcNow.Date, item.Items2[1].CreatedDate);
		}

		[TestMethod]
		public void Should_evolve_from_zero()
		{
			var ev = new Case1Evolver();
			_source = XamlServices.Save(ev.CreateRoot());
			_source = ev.UpgradeDatabaseXaml(_source);
			Trace.WriteLine(_source);
		}
	}

	public class Case1Evolver : ReflectionEvolver<Case1>
	{
		public void Upgrade_1_to_5_rename_field(dynamic state)
		{
			foreach (var item in state.Items)
			{
				item.FirstName = item.Name;
				item.Name = null;
			}
		}

		public void Upgrade_5_to_10_add_required_field(dynamic state)
		{
			foreach (var item in state.Items)
			{
				item.CreatedDate = DateTime.UtcNow.Date;
			}
		}
		public void Upgrade_10_to_15_rename_col(dynamic state)
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

