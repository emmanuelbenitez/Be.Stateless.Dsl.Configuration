﻿#region Copyright & License

// Copyright © 2012 - 2021 François Chabot & Emmanuel Benitez
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using Be.Stateless.Extensions;
using Be.Stateless.Xml.XPath.Extensions;

namespace Be.Stateless.Dsl.Configuration.Xml.XPath
{
	public sealed class XPathBuilder
	{
		public XPathBuilder(XPathNavigator navigator)
		{
			_navigator = (navigator ?? throw new ArgumentNullException(nameof(navigator))).CreateNavigator();
		}

		public string BuildAbsolutePath(XPathFormat format = XPathFormat.LocalName)
		{
			var hierarchy = BuildAncestorNavigationHierarchy(_navigator);
			return $"/{string.Join("/", hierarchy.Select(navigator => BuildNodePath(navigator, format)))}";
		}

		public string BuildCurrentNodePath(XPathFormat format = XPathFormat.LocalName)
		{
			return BuildNodePath(_navigator, format);
		}

		private string BuildNodePath(XPathNavigator navigator, XPathFormat format)
		{
			switch (format)
			{
				// TODO explain the difference
				case XPathFormat.Name:
					return navigator.Name;
				case XPathFormat.LocalName:
					var builder = new StringBuilder("*[");
					builder.Append($"{XpathFunctionNames.LOCAL_NAME}()='{navigator.LocalName}'");
					if (!navigator.NamespaceURI.IsNullOrWhiteSpace()) builder.AppendFormat(" and namespace-uri()='{0}'", navigator.NamespaceURI);
					var discriminants = navigator.GetDiscriminants().ToArray();
					if (discriminants.Any()) builder.AppendFormat(" and ({0})", BuildDiscriminantsSelector(navigator, discriminants));
					builder.Append("]");
					return builder.ToString();
				default:
					throw new ArgumentOutOfRangeException(nameof(format), format, null);
			}
		}

		private string BuildDiscriminantsSelector(XPathNavigator navigator, IEnumerable<string> discriminants)
		{
			return string.Join(
				" and ",
				discriminants.Select(d => $"@{d} = '{navigator.SelectSingleNode($"@{d}")?.Value}'"));
		}

		private IEnumerable<XPathNavigator> BuildAncestorNavigationHierarchy(XPathNavigator navigator)
		{
			// TODO return a IEnumerable of node and not XPathNavigators
			var hierarchy = new Stack<XPathNavigator>();
			while (true)
			{
				hierarchy.Push(navigator);
				var parent = navigator.SelectSingleNode("..");
				if (parent == null || parent.NodeType == XPathNodeType.Root) break;
				navigator = parent;
			}
			return hierarchy;
		}

		private readonly XPathNavigator _navigator;
	}
}
