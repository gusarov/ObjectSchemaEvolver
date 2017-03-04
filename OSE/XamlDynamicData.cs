using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ObjectSchemaEvolver
{
	public class XamlDynamicData : DynamicObject, IEnumerable<object>, IEnumerable<XamlDynamicData>
	{
		protected readonly Dictionary<string, object> _localData = new Dictionary<string, object>();

		XElement _element;
		//XmlNode _xmlNode;
		//private XmlNamespaceManager _xmlNs;

		public XamlDynamicData(string xaml)
		{
			_localData["StoreBack"] = new Func<byte[]>(StoreBack);
			_localData["StoreBackXml"] = new Func<string>(StoreBackXml);
			_localData["IsRoot"] = true;
			_localData["Raw"] = xaml;

			_element = XElement.Parse(xaml);

			/*
			var xd = new XmlDocument();
			xd.LoadXml(xaml);
			_xmlNode = xd.FirstChild;
			*/
			//_xmlNs = new XmlNamespaceManager(xd.NameTable);
			//var xmlns = xd.DocumentElement?.Attributes["xmlns"]?.InnerText ?? "";
			//_xmlNs.AddNamespace("_", xmlns);

		}

		XamlDynamicData(XElement element)
		{
			_element = element;
		}


		byte[] StoreBack()
		{
			var settings = new XmlWriterSettings();
			settings.OmitXmlDeclaration = true;
			settings.Encoding = new UTF8Encoding(false, false);
			settings.Indent = true;
			settings.IndentChars = "\t";
			settings.NewLineChars = Environment.NewLine;
			settings.NewLineHandling = NewLineHandling.None;
			settings.NewLineOnAttributes = false;

			using (var ms = new MemoryStream())
			{
				using (var xml = XmlWriter.Create(ms, settings))
				{
					_element.Save(xml);
				}
				return ms.ToArray();
			}
			// return Encoding.UTF8.GetBytes(StoreBackXml());
		}

		string StoreBackXml()
		{
			return Encoding.UTF8.GetString(StoreBack());
			// _element.Save()
		}

		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			Trace.WriteLine($"{_element.Name.LocalName} TryInvokeMember - {binder.Name}");
			return base.TryInvokeMember(binder, args, out result);
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			return TryGet(binder.Name, out result);
		}

		bool TryGet(string binderName, out object result, string binderNamespace = null)
		{
			object tmpResult;
			if (_localData.TryGetValue(binderName, out tmpResult))
			{
				result = tmpResult;
				return true;
			}

			var attr = string.IsNullOrEmpty(binderNamespace) ? _element.Attribute(binderName) : _element.Attribute((XNamespace)binderNamespace + binderName);
			if (attr != null)
			{
				switch (attr.NodeType)
				{
					case XmlNodeType.Attribute:
						Trace.WriteLine($"{_element.Name.LocalName} GetMember - {binderName} as existing attribute");
						result = attr.Value;
						return true;
					default:
						throw new System.Exception("Unexpected xaml");
				}
			}
			foreach (var childNode in _element.Nodes())
			{
				switch (childNode.NodeType)
				{
					case XmlNodeType.Element:
						var elem = childNode as XElement;
						if (elem != null)
						{
							if (elem.Name.LocalName == binderName || elem.Name.LocalName == _element.Name.LocalName + "." + binderName)
							{
								Trace.WriteLine($"{_element.Name.LocalName} GetMember - {binderName} as existing child node");
								result = Wrap(elem);
								return true;
							}
						}
						break;
					default:
						throw new System.Exception("Unexpected xaml");
				}
			}

			// still nothing - probably this is uninitialized collection or complex property, let's return a placeholder
			Trace.WriteLine($"{_element.Name.LocalName} GetMember - {binderName} create placeholder");
			result = Delayed(_element, binderName); //Add(_element.Name.LocalName + "." + binder.Name);
			return true;
		}

		static XamlDynamicData Delayed(XElement parent, string elementName)
		{
			return new XamlDynamicData(default(XElement))
			{
				_delayedParentElement = parent,
				_delayedElementName = elementName,
				_isDelayed = true,
			};
		}

		XElement _delayedParentElement;
		string _delayedElementName;
		private bool _isDelayed;

		void EnsureCollection()
		{
			if (_isDelayed)
			{
				_element = new XElement(_delayedParentElement.GetDefaultNamespace() + _delayedParentElement.Name.LocalName + "." + _delayedElementName);
				_delayedParentElement.Add(_element);
				_delayedParentElement = null;
				_delayedElementName = null;
				_isDelayed = false;
			}
		}

		/*
		void EnsureAttribute()
		{
			if (_isDelayed)
			{
				_element = new XAttribute(_element.GetDefaultNamespace() + _delayedElementName, "");
				_delayedParentElement.Add(el);
				_delayedParentElement = null;
				_delayedElementName = null;
				_isDelayed = false;
			}
		}
		*/

		public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
		{
			if (indexes.Length > 2)
			{
				throw new Exception("Only 1 and 2 index parameters supported");
			}

			var indexObj = indexes[0];

			Trace.WriteLine($"{_element.Name.LocalName} TryGetIndex {indexObj}");

			if ((indexObj is int))
			{
				var index = (int)indexObj;
				var arr = _element.Nodes().OfType<XElement>().ToArray();
				result = Wrap(arr[index]);
				return true;
			}

			var indexStr = indexObj as string;
			if (indexStr != null)
			{
				if (indexes.Length == 2)
				{
					var second = indexes[1] as string;
					return TryGet(second, out result, indexStr);
				}
				else
				{
					return TryGet(indexStr, out result);
				}
			}

			return base.TryGetIndex(binder, indexes, out result);
		}

		public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
		{
			if (indexes.Length > 2)
			{
				throw new Exception("Only 1 and 2 index parameters supported");
			}

			var indexObj = indexes[0];

			Trace.WriteLine($"{_element.Name.LocalName} TrySetIndex {indexObj}");

			if ((indexObj is int))
			{
				throw new NotImplementedException("Set by index is not implemented");
				/*
				var index = (int)indexObj;
				var arr = _element.Nodes().OfType<XElement>().ToArray();
				result = Wrap(arr[index]);
				return true;
				*/
			}

			var indexStr = indexObj as string;
			if (indexStr != null)
			{
				if (indexes.Length == 2)
				{
					var second = indexes[1] as string;
					return TrySet(second, value, indexStr);
				}
				else
				{
					return TrySet(indexStr, value);
				}
			}
			return base.TrySetIndex(binder, indexes, value);
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			return TrySet(binder.Name, value);
		}

		bool TrySet(string binderName, object value, string binderNamespace = null)
		{
			var existingAttr = string.IsNullOrEmpty(binderNamespace) ? _element.Attribute(binderName) : _element.Attribute((XNamespace)binderNamespace + binderName);
			if (existingAttr != null)
			{
				if (value == null)
				{
					existingAttr.Remove();
				}
				else
				{
					existingAttr.Value = value.ToString();
				}
			}
			else
			{
				var existing = _element.Nodes().OfType<XElement>().FirstOrDefault(x => x.Name.LocalName == _element.Name.LocalName + "." + binderName);
				if (existing != null && value == null)
				{
					existing.Remove();
					return true;
				}

				var dd = value as XamlDynamicData;
				if (dd != null)
				{
					if (dd._element != null)
					{
						var ne = new XElement(_element.GetDefaultNamespace() + _element.Name.LocalName + "." + binderName);
						ne.Add(dd._element.Nodes());
						_element.Add(ne);
					}
				}
				else
				{
					if (string.IsNullOrEmpty(binderNamespace))
					{
						_element.SetAttributeValue(binderName, value);
					}
					else
					{
						_element.SetAttributeValue((XNamespace)binderNamespace + binderName, value);
					}
				}
			}
			return true;
		}

		protected XamlDynamicData Wrap(XElement element)
		{
			return new XamlDynamicData(element);
		}

		IEnumerator<XamlDynamicData> IEnumerable<XamlDynamicData>.GetEnumerator()
		{
			foreach (var item in this)
			{
				yield return (XamlDynamicData)item;
			}
		}

		IEnumerator<object> IEnumerable<object>.GetEnumerator()
		{
			foreach (var item in this)
			{
				yield return item;
			}
		}

		public IEnumerator GetEnumerator()
		{
			foreach (var node in _element.Nodes().OfType<XElement>())
			{
				yield return Wrap(node);
			}
		}

		public dynamic Add(string node)
		{
			EnsureCollection();
			Trace.WriteLine($"{_element.Name.LocalName} Add {node}");
			var el = new XElement(_element.GetDefaultNamespace() + node);
			_element.Add(el);
			return Wrap(el);
		}
		public dynamic First()
		{
			Trace.WriteLine($"{_element.Name.LocalName} First");
			return Wrap(_element.Nodes().OfType<XElement>().First());
		}

		public dynamic Single()
		{
			Trace.WriteLine($"{_element.Name.LocalName} First");
			return Wrap(_element.Nodes().OfType<XElement>().Single());
		}
		public string GetName()
		{
			Trace.WriteLine($"{_element.Name.LocalName} GetName");
			return _element.Name.LocalName;
		}
		public string GetName(out string ns)
		{
			Trace.WriteLine($"{_element.Name.LocalName} GetName");
			ns = _element.Name.NamespaceName;
			return _element.Name.LocalName;
		}
		public void SetName(string name)
		{
			Trace.WriteLine($"{_element.Name.LocalName} SetName {name}");
			_element.Name = _element.Name.Namespace + name;
		}
		public void SetName(string ns, string name)
		{
			Trace.WriteLine($"{_element.Name.LocalName} SetName {ns}, {name}");
			_element.Name = (XNamespace)ns + name;
		}

		public void Clear()
		{
			Trace.WriteLine($"{_element.Name.LocalName} Clear");
			_element.Nodes().Remove();
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj) && _isDelayed)
			{
				return true; // delayed potential item is allways actually null
			}
			if (_isDelayed)
			{
				return false; // delayed potential item never matches to anything
			}
			return base.Equals(obj);
		}

		public static bool operator ==(XamlDynamicData a, object b)
		{
			if (ReferenceEquals(null, a))
			{
				return ReferenceEquals(null, b);
			}
			return a.Equals(b);
		}

		public static bool operator !=(XamlDynamicData a, object b)
		{
			return !(a == b);
		}


	}
}