using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xaml;

namespace ObjectSchemaEvolver
{
	public static class OseSchema
	{
		public static readonly AttachableMemberIdentifier VersionProperty = new AttachableMemberIdentifier(typeof(OseSchema), "Version");

		public static void SetVersion(IAttachedPropertyStore element, decimal value)
		{
			element.SetProperty(VersionProperty, value);
		}
		public static decimal GetVersion(IAttachedPropertyStore element)
		{
			object val;
			if (element.TryGetProperty(VersionProperty, out val))
			{
				return (decimal)val;
			}
			return 0;
		}

	}

	public class AttachedPropertyStore : IAttachedPropertyStore
	{
		private readonly Dictionary<AttachableMemberIdentifier, object> _dic = new Dictionary<AttachableMemberIdentifier, object>();

		void IAttachedPropertyStore.CopyPropertiesTo(KeyValuePair<AttachableMemberIdentifier, object>[] array, int index)
		{
			KeyValuePair<AttachableMemberIdentifier, object>[] src = _dic.ToArray();
			for (int i = 0; i < src.Length; i++)
			{
				array[i + index] = src[i];
			}
		}

		bool IAttachedPropertyStore.RemoveProperty(AttachableMemberIdentifier attachableMemberIdentifier)
		{
			return _dic.Remove(attachableMemberIdentifier);
		}

		void IAttachedPropertyStore.SetProperty(AttachableMemberIdentifier attachableMemberIdentifier, object value)
		{
			_dic[attachableMemberIdentifier] = value;
		}

		bool IAttachedPropertyStore.TryGetProperty(AttachableMemberIdentifier attachableMemberIdentifier, out object value)
		{
			return _dic.TryGetValue(attachableMemberIdentifier, out value);
		}

		int IAttachedPropertyStore.PropertyCount => _dic.Count;
	}
}
