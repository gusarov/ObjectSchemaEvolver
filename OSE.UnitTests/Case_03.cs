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
		string _xaml = @"<Case3 xmlns='test' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
	<Case3.Trades>
		<Case3Item Name = 'Peter' Id='2' />
	</Case3.Trades>
</Case3>
";


		//[TestMethod]
		//[Ignore]
		public void Should_prepare_confusing_col_pro()
		{
			Assert.Inconclusive(XamlServices.Save(new Case2
			{
				Property1 =
				{
					new Case2Item
					{
						Name = "Peter",
					},
					new Case2Item
					{
						Name = "Alis",
					},
				},
				Property2 = new Case2Item
				{
					Name = "Boris",
				},
			}));
		}
		
		[TestMethod]
		public void Should_convert_to_dic()
		{
			var d = Dynamic(_xaml);
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

