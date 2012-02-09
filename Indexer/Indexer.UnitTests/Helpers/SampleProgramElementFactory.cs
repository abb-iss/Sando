using System;
using Sando.Core;

namespace Sando.Indexer.UnitTests.Helpers
{
	public static class SampleProgramElementFactory
	{
		public static ClassElement GetSampleClassElement()
		{
			return new ClassElement()
			{
				AccessLevel = Core.AccessLevel.Public,
				DefinitionLineNumber = 11,
				ExtendedClasses = "SimpleClassBase",
				FullFilePath = "C:/Projects/SimpleClass.cs",
				Id = Guid.NewGuid(),
				ImplementedInterfaces = "IDisposable",
				Name = "SimpleClassName",
				Namespace = "Sando.Indexer.UnitTests",
				Snippet = "public class SimpleClass\n{private int field1;\nprotected void method(){}\n}"
			};
		}

		public static CommentElement GetSampleCommentElement()
		{
			return new CommentElement()
			{
				Body = "Comment body",
				DefinitionLineNumber = 11,
				FullFilePath = "C:/Projects/SimpleClass.cs",
				Id = Guid.NewGuid(),
				MethodId = Guid.NewGuid(),
				Name = "SimpleComment",
				Snippet = "//Comment body"
			};
		}

		public static DocCommentElement GetSampleDocCommentElement()
		{
			return new DocCommentElement()
			{
				Body = "Doc comment body",
				DefinitionLineNumber = 15,
				FullFilePath = "C:/Projects/SimpleClass.cs",
				Id = Guid.NewGuid(),
				Name = "Simple doc comment",
				Snippet = "/**Doc comment body**/"
			};
		}

		public static EnumElement GetSampleEnumElement()
		{
			return new EnumElement()
			{
				AccessLevel = Core.AccessLevel.Public,
				DefinitionLineNumber = 11,
				FullFilePath = "C:/Projects/UsageType.cs",
				Id = Guid.NewGuid(),
				Name = "UsageType",
				Namespace = "Sanod.Indexer.UnitTests",
				Snippet = "public enum UsageType\n{Definition,\nCall,\nComment\n}",
				Values = "Definition, Call, Comment"
			};
		}

		public static FieldElement GetSampleFieldElement()
		{
			return new FieldElement()
			{
				AccessLevel = Core.AccessLevel.Private,
				ClassId = Guid.NewGuid(),
				DefinitionLineNumber = 11,
				FieldType = "int",
				FullFilePath = "C:/Projects/SampleClass.cs",
				Id = Guid.NewGuid(),
				Name = "maxCollectionLength",
				Snippet = "private int maxCollectionLength;"
			};
		}

		public static MethodElement GetSampleMethodElement()
		{
			return new MethodElement()
			{
				AccessLevel = Core.AccessLevel.Private,
				Arguments = "int number, int factor",
				Body = "return number * factor;",
				ClassId = Guid.NewGuid(),
				DefinitionLineNumber = 11,
				FullFilePath = "C:/Projects/SampleClass.cs",
				Id = Guid.NewGuid(),
				Name = "multiply",
				ReturnType = "int",
				Snippet = "private int multiply(int number, int factor)\n{\nreturn number * factor;\n};"
			};
		}

		public static PropertyElement GetSamplePropertyElement()
		{
			return new PropertyElement()
			{
				AccessLevel = Core.AccessLevel.Protected,
				ClassId = Guid.NewGuid(),
				DefinitionLineNumber = 3,
				PropertyType = "double",
				FullFilePath = "C:/Projects/SampleClass.cs",
				Id = Guid.NewGuid(),
				Name = "StockValue",
				Snippet = "protected double StockValue{get; set;};"
			};
		}
	}
}
