using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using Sando.Core;
using Sando.Indexer.Documents;
using Sando.Indexer.Exceptions;
using Sando.Translation;

namespace Sando.Indexer.Searching.Criteria
{
	public class SimpleSearchCriteria : SearchCriteria
	{
		public SimpleSearchCriteria()
		{
			SearchTerms = new SortedSet<string>();
			MatchCase = false;
			MatchWholeWord = false;
			ExactMode = true;
			SearchByAccessLevel = false;
			AccessLevels = new SortedSet<AccessLevel>();
			SearchByProgramElementType = false;
			ProgramElementTypes = new SortedSet<ProgramElementType>();
			SearchByUsageType = false;
			UsageTypes = new SortedSet<UsageType>();
			Locations = new SortedSet<string>();
		}

		public override string ToQueryString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if(SearchByAccessLevel)
			{
				AccessLevelCriteriaToString(stringBuilder);
			}
			if(SearchByProgramElementType)
			{
				if(stringBuilder.Length > 0)
					stringBuilder.Append(" AND ");
				ProgramElementTypeCriteriaToString(stringBuilder);
			}
			if(SearchByLocation)
			{
				if(stringBuilder.Length > 0)
					stringBuilder.Append(" AND ");
				LocationCriteriaToString(stringBuilder);
			}
			UsageTypeCriteriaToString(stringBuilder, SearchByUsageType);
			return stringBuilder.ToString();
		}

		private void AccessLevelCriteriaToString(StringBuilder stringBuilder)
		{
			Contract.Requires(AccessLevels != null, "SimpleSearchCriteria:AccessLevelCriteriaToString - AccessLevels cannot be null!");
			Contract.Requires(AccessLevels.Count > 0, "SimpleSearchCriteria:AccessLevelCriteriaToString - AccessLevels cannot be empty!");

			stringBuilder.Append("(");
			int collectionSize = AccessLevels.Count;
			foreach(AccessLevel accessLevel in AccessLevels)
			{
				stringBuilder.Append(SandoField.AccessLevel.ToString() + ":");
				stringBuilder.Append(accessLevel.ToString());
				if(collectionSize > 1)
				{
					stringBuilder.Append(" OR ");
				}
				--collectionSize;
			}
			stringBuilder.Append(")");
		}

		private void ProgramElementTypeCriteriaToString(StringBuilder stringBuilder)
		{
			Contract.Requires(ProgramElementTypes != null, "SimpleSearchCriteria:ProgramElementTypeCriteriaToString - ProgramElementTypes cannot be null!");
			Contract.Requires(ProgramElementTypes.Count > 0, "SimpleSearchCriteria:ProgramElementTypeCriteriaToString - ProgramElementTypes cannot be empty!");

			stringBuilder.Append("(");
			int collectionSize = ProgramElementTypes.Count;
			foreach(ProgramElementType programElementType in ProgramElementTypes)
			{
				stringBuilder.Append(SandoField.ProgramElementType.ToString() + ":");
				stringBuilder.Append(programElementType.ToString());
				if(collectionSize > 1)
				{
					stringBuilder.Append(" OR ");
				}
				--collectionSize;
			}
			stringBuilder.Append(")");
		}

		private void LocationCriteriaToString(StringBuilder stringBuilder)
		{
			Contract.Requires(Locations != null, "SimpleSearchCriteria:LocationCriteriaToString - Locations cannot be null!");
			Contract.Requires(Locations.Count > 0, "SimpleSearchCriteria:LocationCriteriaToString - Locations cannot be empty!");

			stringBuilder.Append("(");
			int collectionSize = Locations.Count;
			foreach(string location in Locations)
			{
				stringBuilder.Append(SandoField.FullFilePath.ToString() + ":");
				stringBuilder.Append(String.IsNullOrWhiteSpace(location) ? "*" : '\"' + location + '\"');
				if(collectionSize > 1)
				{
					stringBuilder.Append(" OR ");
				}
				--collectionSize;
			}
			stringBuilder.Append(")");
		}

		private void UsageTypeCriteriaToString(StringBuilder stringBuilder, bool searchByUsageType)
		{
			Contract.Requires(UsageTypes != null, "SimpleSearchCriteria:UsageTypeCriteriaToString - UsageTypes cannot be null!");
			Contract.Requires(!SearchByUsageType || UsageTypes.Count > 0, "SimpleSearchCriteria:UsageTypeCriteriaToString - UsageTypes cannot be empty!");

			if(SearchTerms.Count == 0)
				return;
			
			if(stringBuilder.Length > 0)
				stringBuilder.Append(" AND ");
			
			stringBuilder.Append("(");
			if(SearchByUsageType)
			{
				int collectionSize = UsageTypes.Count;
				foreach(UsageType usageType in UsageTypes)
				{
					stringBuilder.Append("(");
					SingleUsageTypeCriteriaToString(stringBuilder, usageType);
					stringBuilder.Append(")");
					if(collectionSize > 1)
					{
						stringBuilder.Append(" OR ");
					}
					--collectionSize;
				}
			}
			else //all usage types are used
			{
				int collectionSize = Enum.GetValues(typeof(UsageType)).Length;
			    int count = 0;
				foreach(UsageType usageType in Enum.GetValues(typeof(UsageType)))
				{
                    stringBuilder.Append("(");
                    SingleUsageTypeCriteriaToString(stringBuilder, usageType);
                    stringBuilder.Append(")");
                    switch (usageType)
                    {
                        case UsageType.Definitions:
                            stringBuilder.Append("^4");
                            break;
                        case UsageType.MethodArguments:
                            stringBuilder.Append("^1");
                            break;
                        default:
                            break;
                    }
                    if (collectionSize > 1)
                    {
                        count++;
                        stringBuilder.Append(" OR ");
                    }
				    --collectionSize;
				}                
			}
			stringBuilder.Append(")");
		}

		private void SingleUsageTypeCriteriaToString(StringBuilder stringBuilder, UsageType usageType)
		{
			int collectionSize = SearchTerms.Count;
			foreach(string searchTerm in SearchTerms)
			{
				switch(usageType)
				{
					case UsageType.Bodies:
						stringBuilder.Append(SandoField.Body.ToString() + ":");
						stringBuilder.Append(searchTerm);
						break;
					case UsageType.Definitions:
						stringBuilder.Append(SandoField.Name.ToString() + ":");
						stringBuilder.Append(searchTerm);
						break;
					case UsageType.EnumValues:
						stringBuilder.Append(SandoField.Values.ToString() + ":");
						stringBuilder.Append(searchTerm);
						break;
					case UsageType.ExtendedClasses:
						stringBuilder.Append(SandoField.ExtendedClasses.ToString() + ":");
						stringBuilder.Append(searchTerm);
						break;
					case UsageType.ImplementedInterfaces:
						stringBuilder.Append(SandoField.ImplementedInterfaces.ToString() + ":");
						stringBuilder.Append(searchTerm);
						break;
					case UsageType.MethodArguments:
						stringBuilder.Append(SandoField.Arguments.ToString() + ":");
						stringBuilder.Append(searchTerm);
						break;
					case UsageType.MethodReturnTypes:
						stringBuilder.Append(SandoField.ReturnType.ToString() + ":");
						stringBuilder.Append(searchTerm);
						break;
					case UsageType.NamespaceNames:
						stringBuilder.Append(SandoField.Namespace.ToString() + ":");
						stringBuilder.Append(searchTerm);
						break;
					case UsageType.PropertyOrFieldTypes:
						stringBuilder.Append(SandoField.DataType.ToString() + ":");
						stringBuilder.Append(searchTerm);
						break;
					default:
						throw new IndexerException(TranslationCode.Exception_General_UnrecognizedEnumValue, null, "UsageType");
				}
				if(collectionSize > 1)
				{
					stringBuilder.Append(" AND "); //every term must be present in the results
				}
				--collectionSize;
			}     
		}

		public virtual SortedSet<string> SearchTerms { get; set; }
		public virtual bool MatchCase { get; set; }
		public virtual bool MatchWholeWord { get; set; }
		public virtual bool ExactMode { get; set; }
		public virtual bool SearchByAccessLevel { get; set; }
		public virtual SortedSet<AccessLevel> AccessLevels { get; set; }
		public virtual bool SearchByProgramElementType { get; set; }
		public virtual SortedSet<ProgramElementType> ProgramElementTypes { get; set; }
		public virtual bool SearchByUsageType { get; set; }
		public virtual SortedSet<UsageType> UsageTypes { get; set; }
		public virtual bool SearchByLocation { get; set; }
		public virtual SortedSet<string> Locations { get; set; }
	}
}
