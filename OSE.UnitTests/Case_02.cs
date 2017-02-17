using System;
using System.Collections.Generic;
using System.Xaml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectSchemaEvolver.UnitTests
{
	[TestClass]
	public class Case_02 : BaseXmlDynamicDataTests<Case2>
	{
		string _xaml = @"<Case2 xmlns='test'>
	<Case2.Property1>
		<Case2Item Name = 'Peter' />
		<!--Case2Item Name = 'Alis' /-->
	</Case2.Property1>
	<Case2.Property2>
		<Case2Item Name = 'Boris' />
	</Case2.Property2>
</Case2>
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
		public void Should_add_age_like_if_both_are_collections()
		{
			// Since there is no visible difference for XML this approachs must work! Othervise the execution would depend on data

			var d = Dynamic(_xaml);
			foreach (var item in d.Property1)
			{
				item.Age = 20;
			}
			foreach (var item in d.Property2)
			{
				item.Age = 21;
			}

			Case2 case2 = Load(d);
			Assert.AreEqual(20, case2.Property1[0].Age);
			Assert.AreEqual(21, case2.Property2.Age);
		}

	}

	public class Case2
	{
		public IList<Case2Item> Property1 { get; } = new List<Case2Item>();
		public Case2Item Property2 { get; set; }


	}

	public class Case2Item
	{
		public string Name { get; set; }
		public int Age { get; set; }
	}
}

