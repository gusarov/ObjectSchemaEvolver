using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectSchemaEvolver.UnitTests.Adv;
using ObjectSchemaEvolver.UnitTests.Deep;

namespace ObjectSchemaEvolver.UnitTests
{
	using static Assert;

	public class SampleItem
	{
		public string FirstName { get; set; }
		public int Height { get; set; }
	}
	public class SampleItem2 : SampleItem
	{
	}
	namespace Deep
	{
		public class SampleItem3 : SampleItem
		{
		}
	}

	public class SampleRoot : AttachedPropertyStore
	{
		[DefaultValue(null)]
		public string Name { get; set; }

		[DefaultValue(0)]
		public int Age { get; set; }

		public List<SampleItem> Collection { get; } = new List<SampleItem>();
		public Dictionary<string, SampleItem> Dictionary { get; } = new Dictionary<string, SampleItem>();
		[DefaultValue(null)]
		public SampleItem Composite { get; set; }
	}

	[TestClass]
	public class BaseXmlDynamicDataTests<T>
	{
		public string StoreBack(dynamic d)
		{
			return d.StoreBackXml();
		}

		public T Load(dynamic d)
		{
			string back = StoreBack(d);
			Trace.WriteLine(back);
			Trace.WriteLine(string.Join(" ", Encoding.UTF8.GetBytes(back).Select(x => x.ToString("X2"))));
			return (T)XamlServices.Parse(back);
		}

		public dynamic Dynamic(string xaml)
		{
			return new XamlDynamicData(xaml);
		}
	}

	[TestClass]
	public class XamlDynamicDataTests : BaseXmlDynamicDataTests<SampleRoot>
	{

		[TestMethod]
		public void Should_10_get_properties()
		{
			var d = Dynamic(@"<SampleRoot Name='abc' Age='12' xmlns='test' />");
			AreEqual("abc", d.Name);
			AreEqual("12", d.Age);
			AreEqual("abc", d["Name"]);
		}
		[TestMethod]
		public void Should_15_get_properties_by_indexer()
		{
			var d = Dynamic(@"<SampleRoot Name='abc' Age='12' xmlns='test' />");
			AreEqual("abc", d["Name"]);
			AreEqual("12", d["Age"]);
		}

		[TestMethod]
		public void Should_20_update_and_store_back()
		{
			string original;
			var d = Dynamic(original = @"<SampleRoot Name='abc' Age='12' xmlns='test' />");

			AreEqual("12", d.Age);
			d.Age = 13;
			AreEqual("13", d.Age);

			Trace.WriteLine(original);
			Trace.WriteLine(string.Join(" ", Encoding.UTF8.GetBytes(original).Select(x => x.ToString("X2"))));

			SampleRoot data = Load(d);
			Assert.AreEqual(13, data.Age);
			Assert.AreEqual("abc", data.Name);
		}


		[TestMethod]
		public void Should_20_add_and_store_back()
		{
			var d = Dynamic(@"<SampleRoot Name='abc' xmlns='test' />");
			d.Age = 13;
			SampleRoot data = Load(d);
			Assert.AreEqual(13, data.Age);
		}

		[TestMethod]
		public void Should_30_get_collections_by_indexer()
		{
			string original;
			var d = Dynamic(original = @"<SampleRoot Name='abc' xmlns='test'>
	<SampleRoot.Collection>
		<SampleItem FirstName='Bob' />
		<SampleItem FirstName='Alice' />
	</SampleRoot.Collection>
</SampleRoot>");
			// make sure it is good
			XamlServices.Parse(original);

			AreEqual("Bob", d.Collection[0].FirstName);
			AreEqual("Alice", d.Collection[1].FirstName);
		}

		[TestMethod]
		public void Should_30_get_collections_by_iterator()
		{
			string original;
			var d = Dynamic(original = @"<SampleRoot Name='abc' xmlns='test'>
	<SampleRoot.Collection>
		<SampleItem FirstName='Bob' />
		<SampleItem FirstName='Alice' />
	</SampleRoot.Collection>
</SampleRoot>");
			// make sure it is good
			XamlServices.Parse(original);

			byte i = 0;
			foreach (var item in d.Collection)
			{
				AreEqual(i==0? "Bob":"Alice", item.FirstName);
				i++;
			}

		}

		[TestMethod]
		public void Should_35_insert_to_collections()
		{
			var d = Dynamic(@"<SampleRoot Name='abc' xmlns='test'>
	<SampleRoot.Collection>
		<SampleItem FirstName='Bob' />
	</SampleRoot.Collection>
</SampleRoot>");
			var newItem = d.Collection.Add("SampleItem");
			newItem.FirstName = "Alice";
			SampleRoot data = Load(d);
			Assert.AreEqual(2, data.Collection.Count);
			Assert.AreEqual("Bob", data.Collection[0].FirstName);
			Assert.AreEqual("Alice", data.Collection[1].FirstName);
		}

		[TestMethod]
		public void Should_35_insert_to_empty_collections()
		{
			var d = Dynamic(@"<SampleRoot Name='abc' xmlns='test' />");

			var newItem = d.Collection.Add("SampleItem");
			newItem.FirstName = "Alice";

			SampleRoot data = Load(d);
			Assert.AreEqual(1, data.Collection.Count);
			Assert.AreEqual("Alice", data.Collection[0].FirstName);
		}

		[TestMethod]
		public void Should_35_update_to_collections_indexer()
		{
			var d = Dynamic(@"<SampleRoot Name='abc' xmlns='test'>
	<SampleRoot.Collection>
		<SampleItem FirstName='Bob' />
	</SampleRoot.Collection>
</SampleRoot>");
			d.Collection[0].FirstName = "Alice";
			SampleRoot data = Load(d);
			Assert.AreEqual(1, data.Collection.Count);
			Assert.AreEqual("Alice", data.Collection[0].FirstName);
		}

		[TestMethod]
		public void Should_35_update_to_collections_iterator()
		{
			var d = Dynamic(@"<SampleRoot Name='abc' xmlns='test'>
	<SampleRoot.Collection>
		<SampleItem FirstName='Bob' />
	</SampleRoot.Collection>
</SampleRoot>");
			foreach (var item in d.Collection)
			{
				item.FirstName = "Alice";
			}
			SampleRoot data = Load(d);
			Assert.AreEqual(1, data.Collection.Count);
			Assert.AreEqual("Alice", data.Collection[0].FirstName);
		}

		[TestMethod]
		public void Should_35_update_to_collections_linq_ext()
		{
			var d = Dynamic(@"<SampleRoot Name='abc' xmlns='test'>
	<SampleRoot.Collection>
		<SampleItem FirstName='Bob' />
	</SampleRoot.Collection>
</SampleRoot>");
			((IEnumerable<dynamic>)d.Collection).First().FirstName = "Alice";
			SampleRoot data = Load(d);
			Assert.AreEqual(1, data.Collection.Count);
			Assert.AreEqual("Alice", data.Collection[0].FirstName);
		}

		[TestMethod]
		public void Should_35_update_to_collections_first_ext()
		{
			var d = Dynamic(@"<SampleRoot Name='abc' xmlns='test'>
	<SampleRoot.Collection>
		<SampleItem FirstName='Bob' />
	</SampleRoot.Collection>
</SampleRoot>");
			d.Collection.First().FirstName = "Alice";
			SampleRoot data = Load(d);
			Assert.AreEqual(1, data.Collection.Count);
			Assert.AreEqual("Alice", data.Collection[0].FirstName);
		}

		[TestMethod]
		public void Should_35_update_to_collections_single_ext()
		{
			var d = Dynamic(@"<SampleRoot Name='abc' xmlns='test'>
	<SampleRoot.Collection>
		<SampleItem FirstName='Bob' />
	</SampleRoot.Collection>
</SampleRoot>");
			d.Collection.Single().FirstName = "Alice";
			SampleRoot data = Load(d);
			Assert.AreEqual(1, data.Collection.Count);
			Assert.AreEqual("Alice", data.Collection[0].FirstName);
		}
		[TestMethod]
		public void Should_36_conditional_update_to_collection()
		{
			var d = Dynamic(@"<SampleRoot Name='abc' xmlns='test'>
	<SampleRoot.Collection>
		<SampleItem FirstName='Bob' />
		<SampleItem FirstName='Alice' Height='20' />
	</SampleRoot.Collection>
</SampleRoot>");

			foreach (var item in d.Collection)
			{
				if (item.Height == null)
				{
					item.Height = 21;
				}
			}

			SampleRoot data = Load(d);
			Assert.AreEqual(2, data.Collection.Count);
			Assert.AreEqual("Bob", data.Collection[0].FirstName);
			Assert.AreEqual(21, data.Collection[0].Height);
			Assert.AreEqual("Alice", data.Collection[1].FirstName);
			Assert.AreEqual(20, data.Collection[1].Height);
		}

		[TestMethod]
		public void Should_40_get_composit()
		{
			var d = Dynamic(@"<SampleRoot Name='abc' xmlns='test'>
	<SampleRoot.Composite>
		<SampleItem FirstName='Bob' />
	</SampleRoot.Composite>
</SampleRoot>");
			Assert.AreEqual("Bob", d.Composite.Single().FirstName);
			SampleRoot data = Load(d);
			Assert.AreEqual("Bob", data.Composite.FirstName);
		}

		[TestMethod]
		public void Should_45_update_composit()
		{
			var d = Dynamic(@"<SampleRoot Name='abc' xmlns='test'>
	<SampleRoot.Composite>
		<SampleItem FirstName='Bob' />
	</SampleRoot.Composite>
</SampleRoot>");
			d.Composite.Single().FirstName = "Alice";
			SampleRoot data = Load(d);
			Assert.AreEqual("Alice", data.Composite.FirstName);
		}

		[TestMethod]
		public void Should_45_update_empty_composit()
		{
			var d = Dynamic(@"<SampleRoot Name='abc' xmlns='test' />");
			var item = d.Composite.Add("SampleItem");
			item.FirstName = "Alice";
			SampleRoot data = Load(d);
			AreEqual("Alice", data.Composite.FirstName);
		}

		[TestMethod]
		public void Should_45_get_node_name()
		{
			var d = Dynamic(@"<SampleRoot Name='abc' xmlns='test'>
	<SampleRoot.Collection>
		<SampleItem FirstName='Bob' />
		<SampleItem2 FirstName='Alice' />
	</SampleRoot.Collection>
</SampleRoot>");
			foreach (var item in d.Collection)
			{
				if (item.GetName() == "SampleItem")
				{
					item.SetName("SampleItem2");
				}
				else if (item.GetName() == "SampleItem2")
				{
					item.SetName("SampleItem");
				}
			}

			SampleRoot data = Load(d);
			AreEqual(2, data.Collection.Count);
			AreEqual("Bob", data.Collection[0].FirstName);
			IsInstanceOfType(data.Collection[0], typeof(SampleItem2));
			IsInstanceOfType(data.Collection[1], typeof(SampleItem));
		}

		[TestMethod]
		public void Should_45_get_node_name_with_deep_ns()
		{
			var d = Dynamic(@"<SampleRoot Name='abc' xmlns='test'>
	<SampleRoot.Collection>
		<SampleItem FirstName='Bob' />
		<SampleItem2 FirstName='Alice' />
	</SampleRoot.Collection>
</SampleRoot>");
			foreach (var item in d.Collection)
			{
				if (item.GetName() == "SampleItem")
				{
					item.SetName("clr-namespace:ObjectSchemaEvolver.UnitTests.Deep;assembly=ObjectSchemaEvolver.UnitTests", "SampleItem3");
				}
				else if (item.GetName() == "SampleItem2")
				{
					item.SetName("SampleItem");
				}
			}

			SampleRoot data = Load(d);
			AreEqual(2, data.Collection.Count);
			AreEqual("Bob", data.Collection[0].FirstName);
			IsInstanceOfType(data.Collection[0], typeof(SampleItem3));
			IsInstanceOfType(data.Collection[1], typeof(SampleItem));
		}

		[TestMethod]
		public void Should_serialize_attached_property()
		{
			var root = new SampleRoot();
			AttachablePropertyServices.SetProperty(root, OseSchema.VersionProperty, 1.2m);
			AttachablePropertyServices.SetProperty(root, TestAttacher.SoundProperty, 15);
			var xaml = XamlServices.Save(root);
			var load = XamlServices.Parse(xaml);
			Trace.WriteLine(xaml);
			decimal val;
			AttachablePropertyServices.TryGetProperty(root, OseSchema.VersionProperty, out val);
			int snd;
			AttachablePropertyServices.TryGetProperty(root, TestAttacher.SoundProperty, out snd);
			Assert.AreEqual(1.2m, val);
			Assert.AreEqual(15, snd);
		}

		[TestMethod]
		public void Should_access_attached_property()
		{
			var root = new SampleRoot
			{
				Name = "val",
			};

			AttachablePropertyServices.SetProperty(root, OseSchema.VersionProperty, 1.2m);
			AttachablePropertyServices.SetProperty(root, TestAttacher.SoundProperty, 15);
			var xaml = XamlServices.Save(root);
			Trace.WriteLine(xaml);
			var d = Dynamic(xaml);
			AreEqual("val", d["Name"]);
			AreEqual("val", d["", "Name"]);
			AreEqual("1.2", d["ose", "OseSchema.Version"]);
			AreEqual("15", d["clr-namespace:ObjectSchemaEvolver.UnitTests.Adv;assembly=ObjectSchemaEvolver.UnitTests", "TestAttacher.Sound"]);

			d["ose", "OseSchema.Version"] = 1.3m;
			AreEqual("1.3", d["ose", "OseSchema.Version"]);
			SampleRoot data = Load(d);
			decimal val;
			AttachablePropertyServices.TryGetProperty(data, OseSchema.VersionProperty, out val);
			Assert.AreEqual(1.3m, val);
		}

		[TestMethod]
		public void Should_40_get_dic()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void Should_45_update_dic()
		{
			Assert.Inconclusive();
		}

		[TestMethod]
		public void Should_50_update_content_property()
		{
			Assert.Inconclusive();
		}
	}
}
