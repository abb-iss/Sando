using System;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.UnitTestHelpers
{
	public static class SampleProgramElementFactory
	{
		public static ClassElement GetSampleClassElement(
			AccessLevel accessLevel = AccessLevel.Public,
			int definitionLineNumber = 11,
			string extendedClasses = "SimpleClassBase",
			string fullFilePath = "C:/Projects/SampleClass.cs",
			string implementedInterfaces = "IDisposable",
			string name = "SimpleClassName",
			string namespaceName = "Sando.Indexer.UnitTests",
			string snippet = "public class SimpleClass\n{private int field1;\nprotected void method(){}\n}",
			string modifiers = ""
		)
		{
			return new ClassElement(name, definitionLineNumber, -1000, fullFilePath, snippet, accessLevel, namespaceName, extendedClasses, implementedInterfaces, modifiers, "junk body");
		}

		public static CommentElement GetSampleCommentElement(
			string body = "Comment body",
			int definitionLineNumber = 11,
			string fullFilePath = "C:/Projects/SimpleClass.cs",
			string name = "SimpleComment",
			string snippet = "//Comment body"
		)
		{
			return new CommentElement(name, definitionLineNumber, -1000, fullFilePath, snippet, body);
		}

		public static EnumElement GetSampleEnumElement(
			AccessLevel accessLevel = AccessLevel.Public,
			int definitionLineNumber = 11,
			string fullFilePath = "C:/Projects/UsageType.cs",
			string name = "UsageType",
			string namespaceName = "Sando.Indexer.UnitTests",
			string snippet = "public enum UsageType\n{Definition,\nCall,\nComment\n}",
			string body = "Definition, Call, Comment"
		)
		{
            return new EnumElement(name, definitionLineNumber, -1000, fullFilePath, snippet, accessLevel, namespaceName, body);
		}

		public static FieldElement GetSampleFieldElement(
			AccessLevel accessLevel = AccessLevel.Private,
			string classId = "0f8fad5b-d9cb-469f-a165-70867728950e",
			string className = "SampleCLass",
			int definitionLineNumber = 11,
			string fieldType = "int",
			string fullFilePath = "C:/Projects/SampleClass.cs",
			string name = "maxCollectionLength",
			string snippet = "private int maxCollectionLength;",
			string modifiers = "",
			string initialValue = ""
		)
		{
            return new FieldElement(name, definitionLineNumber, -1000, fullFilePath, snippet, accessLevel, fieldType, new Guid(classId), className, modifiers, initialValue);
		}

		public static MethodElement GetSampleMethodElement(
			AccessLevel accessLevel = AccessLevel.Public,
			string arguments = "int number, int factor",
			string body = "return number * factor;",
			string classId = "0f8fad5b-d9cb-469f-a165-70867728950e",
			string className = "SampleCLass",
			int definitionLineNumber = 12,
			string fullFilePath = "C:/Projects/SampleClass.cs",
			string name = "multiply",
			string returnType = "int",
			string snippet = "private int multiply(int number, int factor)\n{\nreturn number * factor;\n};",
			string modifiers = ""
		)
		{
            return new MethodElement(name, definitionLineNumber, -1000, fullFilePath, snippet, accessLevel, arguments, returnType, body, new Guid(classId), className, modifiers, false);
		}

		public static PropertyElement GetSamplePropertyElement(
			AccessLevel accessLevel = AccessLevel.Private,
			string body = "",
			string classId = "0f8fad5b-d9cb-469f-a165-70867728950e",
			string className = "SampleCLass",
			int definitionLineNumber = 3,
			string propertyType = "double",
			string fullFilePath = "C:/Projects/SampleClass.cs",
			string name = "StockValue",
			string snippet = "protected double StockValue{get; set;};",
			string modifiers = ""
		)
		{
            return new PropertyElement(name, definitionLineNumber, -1000, fullFilePath, snippet, accessLevel, propertyType, body, new Guid(classId), className, modifiers);
		}

        public static ProgramElement GetSampleStructElement(
            AccessLevel accessLevel = AccessLevel.Public,
            int definitionLineNumber = 11,
            string extendedClasses = "SimpleClassBase",
            string body = "",
            string fullFilePath = "C:/Projects/SampleClass.cs",
            string implementedInterfaces = "IDisposable",
            string name = "SimpleClassName",
            string namespaceName = "Sando.Indexer.UnitTests",
            string snippet = "public class SimpleClass\n{private int field1;\nprotected void method(){}\n}",
            string modifiers = ""
        )
        {
            return new StructElement(name, definitionLineNumber, -1000, fullFilePath, snippet, accessLevel, namespaceName, body, extendedClasses, modifiers);
        }
	}
}
