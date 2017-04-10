using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xaml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectSchemaEvolver.UnitTests
{
	[TestClass]
	public class Case_03 : BaseXmlDynamicDataTests<Case3>
	{
		[TestMethod]
		public void Should_convert_to_dic()
		{
			var d = Dynamic(@"<Case3 xmlns='test'>
	<Case3.Trades>
		<Case3Item Name = 'Peter' Id='2' />
	</Case3.Trades>
</Case3>
");
			foreach (var item in d.Trades)
			{
				item["http://schemas.microsoft.com/winfx/2006/xaml", "Key"] = item.Id;
			}
			Case3 case3 = Load(d);
			Assert.AreEqual("Peter", case3.Trades[2].Name);
		}

		[TestMethod]
		public void Should_convert_to_dic_with_x()
		{
			var d = Dynamic(@"<Case3 xmlns='test' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
	<Case3.Trades>
		<Case3Item Name = 'Peter' Id='2' />
	</Case3.Trades>
</Case3>
");
			foreach (var item in d.Trades)
			{
				item["http://schemas.microsoft.com/winfx/2006/xaml", "Key"] = item.Id;
			}
			Case3 case3 = Load(d);
			Assert.AreEqual("Peter", case3.Trades[2].Name);
		}

	}

	public class Case3
	{
		public IDictionary<int, Case3Item> Trades { get; } = new Dictionary<int, Case3Item>();

	}

	public class Case3Item
	{
		public string Name { get; set; }
		public int Id { get; set; }
	}
}

