using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xaml;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectSchemaEvolver.UnitTests
{
	[TestClass]
	public class Case_04 : BaseXmlDynamicDataTests<Case4>
	{
		[TestMethod]
		public void Should_use_another_field_for_version()
		{
			var xaml = new Case4Evolver().UpgradeDatabaseXaml(@"<Case4 xmlns='test' Version='555' />");
			var d = Dynamic(xaml);
			Case4 case4 = Load(d);
			Assert.AreEqual(555ul, case4.VersionStamp);
			Assert.AreEqual(10m, case4.Ver);
		}

		[TestMethod]
		public void Should_keep_ver_for_newer_schemas()
		{
			var xaml = new Case4Evolver().UpgradeDatabaseXaml(@"<Case4 xmlns='test' Ver='2000' />");
			var d = Dynamic(xaml);
			Case4 case4 = Load(d);
			Assert.AreEqual(2000m, case4.Ver);
		}

		[TestMethod]
		public void Should_complain_on_non_initialized_ver()
		{
			var xaml = XamlServices.Save(new Case4());
			Console.WriteLine(xaml);
			var ex = Record(delegate
			{
				xaml = new Case4Evolver().UpgradeDatabaseXaml(xaml);
			});
			Assert.IsNotNull(ex);
			Assert.IsInstanceOfType(ex, typeof(EvolverException));
			StringAssert.Contains(ex.Message, "There is no upgrade method for current schema version -1");
		}

		[TestMethod]
		public void Should_initialize_ver()
		{
			var ev = new Case4Evolver();
			var xaml = XamlServices.Save(ev.CreateRoot());
			var d = Dynamic(xaml);
			Case4 case4 = Load(d);
			Assert.AreEqual(10m, case4.Ver);
			Assert.AreEqual(0UL, case4.VersionStamp);
		}

	}

	public class Case4
	{
		public ulong VersionStamp { get; set; }
		public decimal Ver { get; set; } = -1;

	}


	public class Case4Evolver : ReflectionEvolver<Case4>
	{
		public Case4Evolver()
		{
			VersionFieldName = "Ver";
		}

		public void Upgrade_0_to_10(dynamic state)
		{
			state.VersionStamp = state.Version;
			state.Version = null;
		}
	}
}

