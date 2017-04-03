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
		string _xaml = @"<Case4 xmlns='test' Version='555' />
";


		[TestMethod]
		public void Should_use_another_field_for_version()
		{
			_xaml = new Case4Evolver().UpgradeDatabaseXaml(_xaml);
			var d = Dynamic(_xaml);
			Case4 case4 = Load(d);
			Assert.AreEqual(555ul, case4.VersionStamp);
			Assert.AreEqual(10m, case4.Ver);
		}

		[TestMethod]
		public void Should_keep_ver_for_newer_schemas()
		{
			_xaml = new Case4Evolver().UpgradeDatabaseXaml(@"<Case4 xmlns='test' Ver='2000' />");
			var d = Dynamic(_xaml);
			Case4 case4 = Load(d);
			Assert.AreEqual(2000m, case4.Ver);
		}

	}

	public class Case4
	{
		public ulong VersionStamp { get; set; }
		public ulong Ver { get; set; }

	}


	public class Case4Evolver : ReflectionEvolver
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

