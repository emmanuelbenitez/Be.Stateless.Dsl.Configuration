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
using System.Reflection;
using Be.Stateless.Dsl.Configuration.Specification;
using Be.Stateless.IO.Extensions;
using Be.Stateless.Resources;
using FluentAssertions;
using Xunit;
using static Be.Stateless.Unit.DelegateFactory;

namespace Be.Stateless.Dsl.Configuration.Command
{
	public class ElementInsertionCommandFixture
	{
		[Fact]
		public void ExecuteSucceeds()
		{
			var command = new ElementInsertionCommand(
				"/configuration",
				new ElementSpecification("test", null, null, "test"));
			var document = ResourceManager.Load(Assembly.GetExecutingAssembly(), "Be.Stateless.Resources.web-original.config", stream => stream.AsXmlDocument());
			command.Execute(document);
			document.SelectSingleNode("/configuration/test")
				.Should().NotBeNull();
		}

		[Fact]
		public void ExecuteSucceedsWithAttributeUpdate()
		{
			var command = new ElementInsertionCommand(
				"/configuration",
				new ElementSpecification(
					"test",
					null,
					new[] {
						new AttributeSpecification {
							Name = "test",
							NamespaceUri = "urn:test",
							Value = "value"
						},
						new AttributeSpecification {
							Name = "test",
							Value = "value"
						}
					},
					"test"));

			var document = ResourceManager.Load(Assembly.GetExecutingAssembly(), "Be.Stateless.Resources.web-original.config", stream => stream.AsXmlDocument());
			command.Execute(document);
			document.SelectSingleNode("/configuration/test")
				.Should().NotBeNull();
			document.SelectSingleNode("/configuration/test/@*[local-name() = 'test' and namespace-uri()='urn:test']")
				.Should().NotBeNull()
				.And.Subject.Value.Should().Be("value");
			document.SelectSingleNode("/configuration/test/@test")
				.Should().NotBeNull()
				.And.Subject.Value.Should().Be("value");
		}

		[Fact]
		public void ExecuteThrowsWhenElementAlreadyExists()
		{
			var command = new ElementInsertionCommand(
				"/configuration",
				new ElementSpecification("appSettings", null, null, "appSettings"));
			Action(() => command.Execute(ResourceManager.Load(Assembly.GetExecutingAssembly(), "Be.Stateless.Resources.web-original.config", stream => stream.AsXmlDocument())))
				.Should().Throw<InvalidOperationException>()
				.WithMessage("The configuration element already exists at '/configuration/appSettings'.");
		}
	}
}
