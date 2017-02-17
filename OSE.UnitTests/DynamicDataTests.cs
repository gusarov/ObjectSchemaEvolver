using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectSchemaEvolver.UnitTests
{
	/// <summary>
	/// Summary description for DynamicDataTests
	/// </summary>
	[TestClass]
	public class DynamicDataTests
	{

		[TestMethod]
		public void Should_allow_add_properties_on_the_fly()
		{
			dynamic item = new DynamicData();
			item.Some = 123;
			Assert.AreEqual(123, item.Some);

			CollectionAssert.AreEqual(new[] { "Some" }, ((DynamicData)item).GetDynamicMemberNames().ToArray());
		}

		[TestMethod]
		public void Should_allow_delete_properties_on_the_fly()
		{
			dynamic item = new DynamicData();
			item.Some = 123;
			item.Other = 124;

			item.Some = null; // DynamicData.Delete;

			Assert.AreEqual(124, item.Other);
			CollectionAssert.AreEqual(new[] { "Other" }, ((DynamicData)item).GetDynamicMemberNames().ToArray());
		}

		/*

		[TestMethod]
		public void Should_allow_delete_properties_on_the_fly_using_keyword()
		{
			dynamic item = new DynamicData();
			item.Some = 123;
			item.Other = 124;

			item.Some = null;  //"!delete";

			Assert.AreEqual(124, item.Other);
			CollectionAssert.AreEqual(new[] { "Other" }, ((DynamicData)item).GetDynamicMemberNames().ToArray());
		}
		[TestMethod]
		public void Should_allow_delete_properties_on_the_fly_using_method()
		{
			dynamic item = new DynamicData();
			item.Some = 123;
			item.Other = 124;

			item.Delete("Some");

			Assert.AreEqual(124, item.Other);
			CollectionAssert.AreEqual(new[] { "Other" }, ((DynamicData)item).GetDynamicMemberNames().ToArray());
		}
		*/

		[TestMethod]
		public void Should_allow_to_add_method()
		{
			bool ok = false;
			dynamic item = new DynamicData();
			item.Test = new Action(delegate
			{
				ok = true;
			});
			item.Test();
			Assert.AreEqual(true, ok);
		}
	}
}
