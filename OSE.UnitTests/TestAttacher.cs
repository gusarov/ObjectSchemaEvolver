using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;

namespace ObjectSchemaEvolver.UnitTests
{
	namespace Adv
	{
		public static class TestAttacher
		{
			public static readonly AttachableMemberIdentifier SoundProperty = new AttachableMemberIdentifier(typeof(TestAttacher), "Sound");

			public static void SetSound(IAttachedPropertyStore element, int value)
			{
				element.SetProperty(SoundProperty, value);
			}

			public static int GetSound(IAttachedPropertyStore element)
			{
				object val;
				if (element.TryGetProperty(SoundProperty, out val))
				{
					return (int)val;
				}
				return 0;
			}
		}
	}

}
