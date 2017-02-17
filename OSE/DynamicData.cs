using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectSchemaEvolver
{
	public class DynamicData : DynamicObject
	{
		// public static readonly object Delete = new object();

		// bool? _isRoot;
		/*
		public DynamicData(bool? isRoot = null)
		{
			_isRoot = isRoot;
		}
		*/

		readonly protected Dictionary<string, object> _data = new Dictionary<string, object>();

		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return _data.Keys;
		}

		/*

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			if (_isRoot == true && binder.Name.Equals("StoreBack", binder.IgnoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture))
			{
				result = StoreBack();
				return true;
			}
			if (binder.Name.Equals("Delete", binder.IgnoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture))
			{
				if (args.Length == 1)
				{
					var name = args[0] as string;
					if (name != null)
					{
						_data.Remove(name);
						result = null;
						return true;
					}
				}
			}
			return base.TryInvokeMember(binder, args, out result);
		}



		protected virtual byte[] StoreBack()
		{
			return null;
		}
		*/

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			var exists = _data.TryGetValue(binder.Name, out result);
			if (exists)
			{
				result = WrapResult(result);
			}
			return exists;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			/*
			var strValue = value as string;
			if (value == Delete || string.Equals(strValue, "!delete", StringComparison.OrdinalIgnoreCase))
			{
				_data.Remove(binder.Name);
				return true;
			}
			else
			{
				_data[binder.Name] = value ?? new NullFieldPlaceholder(binder.ReturnType); // type here just means that object have a field of this type but it is null
			}
			*/

			_data[binder.Name] = value; // type here just means that object have a field of this type but it is null
			if (value == null)
			{
				_data.Remove(binder.Name);
			}
			return true;
		}

		protected virtual object WrapResult(object result)
		{
			return result;
		}

	}
}
