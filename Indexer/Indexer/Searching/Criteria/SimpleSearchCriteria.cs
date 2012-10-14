using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using Sando.Core.Extensions;
using Sando.ExtensionContracts.ProgramElementContracts;
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
			queryWeights = ExtensionPointsRepository.GetInstance().GetQueryWeightsSupplierImplementation().GetQueryWeightsValues();
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
				AppendBoostFactor(stringBuilder, SandoField.AccessLevel.ToString());
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
			    string value = programElementType.ToString();
                if(!value.Equals(ProgramElementType.Method.ToString()))
                {
                    value = value + "*";
                }
			    stringBuilder.Append(value);
				AppendBoostFactor(stringBuilder, SandoField.ProgramElementType.ToString());
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
				AppendBoostFactor(stringBuilder, SandoField.FullFilePath.ToString());
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
			if(!SearchByUsageType)
			{
				foreach(UsageType usageType in Enum.GetValues(typeof(UsageType)))
				{
					UsageTypes.Add(usageType);
				}
			}
			int searchTermsLeft = SearchTerms.Count;
			foreach(string searchTerm in SearchTerms)
			{
				//stringBuilder.Append("(");
                string searchTermEscaped = EscapeSpecialCharacters(searchTerm);
				int usageTypesLeft = UsageTypes.Count;
				foreach(UsageType usageType in UsageTypes)
				{
                    SingleUsageTypeCriteriaToString(stringBuilder, usageType, searchTermEscaped);
					if(usageTypesLeft > 1)
					{
						stringBuilder.Append(" OR ");
					}
					--usageTypesLeft;
				}
				//stringBuilder.Append(")"); 
				if(searchTermsLeft > 1)
				{
					stringBuilder.Append(" OR ");
				}
				--searchTermsLeft;
			}
			stringBuilder.Append(")");
		}

        private string EscapeSpecialCharacters(string searchTerm)
        {
            StringBuilder escapedSearchTermBuilder = new StringBuilder(searchTerm);
            escapedSearchTermBuilder.Replace("\\", "\\\\");
            escapedSearchTermBuilder.Replace("+", "\\+");
            escapedSearchTermBuilder.Replace("-", "\\-");
            escapedSearchTermBuilder.Replace("&&", "\\&\\&");
            escapedSearchTermBuilder.Replace("||", "\\|\\|");
            escapedSearchTermBuilder.Replace("!", "\\!");
            escapedSearchTermBuilder.Replace("(", "\\(");
            escapedSearchTermBuilder.Replace(")", "\\)");
            escapedSearchTermBuilder.Replace("{", "\\{");
            escapedSearchTermBuilder.Replace("}", "\\}");
            escapedSearchTermBuilder.Replace("[", "\\[");
            escapedSearchTermBuilder.Replace("]", "\\]");
            escapedSearchTermBuilder.Replace("^", "\\^");
            escapedSearchTermBuilder.Replace("\"", "\\\"");
            escapedSearchTermBuilder.Replace("~", "\\~");
            escapedSearchTermBuilder.Replace(":", "\\:");
            return escapedSearchTermBuilder.ToString();
        }

		private void SingleUsageTypeCriteriaToString(StringBuilder stringBuilder, UsageType usageType, string searchTerm)
		{
			if(searchTerm.IndexOf(" ") > 0)
			{
				searchTerm = "\"" + searchTerm + "\"";
			}
			switch(usageType)
			{
				case UsageType.Bodies:
					stringBuilder.Append(SandoField.Body.ToString() + ":");
					stringBuilder.Append(searchTerm);
					AppendBoostFactor(stringBuilder, SandoField.Body.ToString());
					break;
				case UsageType.Definitions:
					stringBuilder.Append(SandoField.Name.ToString() + ":");
					stringBuilder.Append(searchTerm);
					AppendBoostFactor(stringBuilder, SandoField.Name.ToString());
					break;
				case UsageType.EnumValues:
					stringBuilder.Append(SandoField.Values.ToString() + ":");
					stringBuilder.Append(searchTerm);
					AppendBoostFactor(stringBuilder, SandoField.Values.ToString());
					break;
				case UsageType.ExtendedClasses:
					stringBuilder.Append(SandoField.ExtendedClasses.ToString() + ":");
					stringBuilder.Append(searchTerm);
					AppendBoostFactor(stringBuilder, SandoField.ExtendedClasses.ToString());
					break;
				case UsageType.ImplementedInterfaces:
					stringBuilder.Append(SandoField.ImplementedInterfaces.ToString() + ":");
					stringBuilder.Append(searchTerm);
					AppendBoostFactor(stringBuilder, SandoField.ImplementedInterfaces.ToString());
					break;
				case UsageType.MethodArguments:
					stringBuilder.Append(SandoField.Arguments.ToString() + ":");
					stringBuilder.Append(searchTerm);
					AppendBoostFactor(stringBuilder, SandoField.Arguments.ToString());
					break;
				case UsageType.MethodReturnTypes:
					stringBuilder.Append(SandoField.ReturnType.ToString() + ":");
					stringBuilder.Append(searchTerm);
					AppendBoostFactor(stringBuilder, SandoField.ReturnType.ToString());
					break;
				case UsageType.NamespaceNames:
					stringBuilder.Append(SandoField.Namespace.ToString() + ":");
					stringBuilder.Append(searchTerm);
					AppendBoostFactor(stringBuilder, SandoField.Namespace.ToString());
					break;
				case UsageType.PropertyOrFieldTypes:
					stringBuilder.Append(SandoField.DataType.ToString() + ":");
					stringBuilder.Append(searchTerm);
					AppendBoostFactor(stringBuilder, SandoField.DataType.ToString());
					break;
				default:
					throw new IndexerException(TranslationCode.Exception_General_UnrecognizedEnumValue, null, "UsageType");
			}
		}

		private void AppendBoostFactor(StringBuilder stringBuilder, string fieldName)
		{
			if(queryWeights.ContainsKey(fieldName) && queryWeights[fieldName] != 1)
			{
				stringBuilder.Append("^");
				stringBuilder.Append(queryWeights[fieldName]);
			}
		}

		public virtual SortedSet<string> SearchTerms { get; set; }
		public virtual bool MatchCase { get; set; }
		public virtual bool ExactMode { get; set; }
		public virtual bool SearchByAccessLevel { get; set; }
		public virtual SortedSet<AccessLevel> AccessLevels { get; set; }
		public virtual bool SearchByProgramElementType { get; set; }
		public virtual SortedSet<ProgramElementType> ProgramElementTypes { get; set; }
		public virtual bool SearchByUsageType { get; set; }
		public virtual SortedSet<UsageType> UsageTypes { get; set; }
		public virtual bool SearchByLocation { get; set; }
		public virtual SortedSet<string> Locations { get; set; }

		private Dictionary<string, float> queryWeights;
	}
}
