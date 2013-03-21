using System;
using System.Text;
using System.Diagnostics.Contracts;
using Sando.Core.Extensions;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.Indexer.Documents;
using Sando.Indexer.Exceptions;
using Sando.Indexer.Searching.Criteria;
using Sando.Translation;

namespace Sando.Indexer.Searching
{
    public class LuceneQueryStringBuilder
    {
        private readonly SimpleSearchCriteria _criteria;
        private System.Collections.Generic.Dictionary<string, float> _queryWeights;

        public LuceneQueryStringBuilder(SimpleSearchCriteria simpleSearchCriteria)
        {            
            _criteria = simpleSearchCriteria;
        }


        public string Build()
        {
            _queryWeights = ExtensionPointsRepository.Instance.GetQueryWeightsSupplierImplementation().GetQueryWeightsValues();
            var stringBuilder = new StringBuilder();
            if (_criteria.SearchByAccessLevel && (!_criteria.SearchByProgramElementType || !_criteria.ProgramElementTypes.Contains(ProgramElementType.Comment)))
            {
                AccessLevelCriteriaToString(stringBuilder);
            }
            if (_criteria.SearchByProgramElementType)
            {
                if (stringBuilder.Length > 0)
                    stringBuilder.Append(" AND ");
                ProgramElementTypeCriteriaToString(stringBuilder);
            }
            if (_criteria.SearchByFileExtension)
            {
                if (stringBuilder.Length > 0)
                    stringBuilder.Append(" AND ");
                FileExtensionsCriteriaToString(stringBuilder);
            }
            if (_criteria.SearchByLocation)
            {
                if (stringBuilder.Length > 0)
                    stringBuilder.Append(" AND ");
                LocationCriteriaToString(stringBuilder);
            }
            UsageTypeCriteriaToString(stringBuilder);
            return stringBuilder.ToString();
        }

        private void AccessLevelCriteriaToString(StringBuilder stringBuilder)
        {
            Contract.Requires(_criteria.AccessLevels != null, "SimpleSearchCriteria:AccessLevelCriteriaToString - AccessLevels cannot be null!");
            Contract.Requires(_criteria.AccessLevels.Count > 0, "SimpleSearchCriteria:AccessLevelCriteriaToString - AccessLevels cannot be empty!");

            stringBuilder.Append("(");
            int collectionSize = _criteria.AccessLevels.Count;
            foreach (AccessLevel accessLevel in _criteria.AccessLevels)
            {
                stringBuilder.Append(SandoField.AccessLevel.ToString() + ":");
                stringBuilder.Append(accessLevel.ToString()+"*");
                AppendBoostFactor(stringBuilder, SandoField.AccessLevel.ToString());
                if (collectionSize > 1)
                {
                    stringBuilder.Append(" OR ");
                }
                --collectionSize;
            }
            stringBuilder.Append(")");
        }

        private void ProgramElementTypeCriteriaToString(StringBuilder stringBuilder)
        {
            Contract.Requires(_criteria.ProgramElementTypes != null, "SimpleSearchCriteria:ProgramElementTypeCriteriaToString - ProgramElementTypes cannot be null!");
            Contract.Requires(_criteria.ProgramElementTypes.Count > 0, "SimpleSearchCriteria:ProgramElementTypeCriteriaToString - ProgramElementTypes cannot be empty!");

            stringBuilder.Append("(");
            int collectionSize = _criteria.ProgramElementTypes.Count;
            foreach (ProgramElementType programElementType in _criteria.ProgramElementTypes)
            {
                stringBuilder.Append(SandoField.ProgramElementType.ToString() + ":");
                string value = programElementType.ToString();
                if (!value.Equals(ProgramElementType.Method.ToString()))
                {
                    value = value + "*";
                }
                stringBuilder.Append(value);
                AppendBoostFactor(stringBuilder, SandoField.ProgramElementType.ToString());
                if (collectionSize > 1)
                {
                    stringBuilder.Append(" OR ");
                }
                --collectionSize;
            }
            stringBuilder.Append(")");
        }

        private void LocationCriteriaToString(StringBuilder stringBuilder)
        {
            Contract.Requires(_criteria.Locations != null, "SimpleSearchCriteria:LocationCriteriaToString - Locations cannot be null!");
            Contract.Requires(_criteria.Locations.Count > 0, "SimpleSearchCriteria:LocationCriteriaToString - Locations cannot be empty!");

            stringBuilder.Append("(");
            int collectionSize = _criteria.Locations.Count;
            foreach (string location in _criteria.Locations)
            {
                stringBuilder.Append(SandoField.FullFilePath.ToString() + ":");
                stringBuilder.Append(String.IsNullOrWhiteSpace(location) ? "*" : '\"' + location + '\"');
                AppendBoostFactor(stringBuilder, SandoField.FullFilePath.ToString());
                if (collectionSize > 1)
                {
                    stringBuilder.Append(" OR ");
                }
                --collectionSize;
            }
            stringBuilder.Append(")");
        }

        private void FileExtensionsCriteriaToString(StringBuilder stringBuilder)
        {
            Contract.Requires(_criteria.FileExtensions != null, "SimpleSearchCriteria:LFileExtensionsCriteriaToString - FileExtensions cannot be null!");
            Contract.Requires(_criteria.FileExtensions.Count > 0, "SimpleSearchCriteria:FileExtensionsCriteriaToString - FileExtensions cannot be empty!");

            stringBuilder.Append("(");
            int collectionSize = _criteria.FileExtensions.Count;
            foreach (var fileExtension in _criteria.FileExtensions)
            {
                stringBuilder.Append(SandoField.FileExtension.ToString() + ":");
                stringBuilder.Append('\"' + fileExtension + '\"');
                AppendBoostFactor(stringBuilder, SandoField.FileExtension.ToString());
                if (collectionSize > 1)
                {
                    stringBuilder.Append(" OR ");
                }
                --collectionSize;
            }
            stringBuilder.Append(")");
        }

        private void UsageTypeCriteriaToString(StringBuilder stringBuilder)
        {
            Contract.Requires(_criteria.UsageTypes != null, "SimpleSearchCriteria:UsageTypeCriteriaToString - UsageTypes cannot be null!");
            Contract.Requires(!_criteria.SearchByUsageType || _criteria.UsageTypes.Count > 0, "SimpleSearchCriteria:UsageTypeCriteriaToString - UsageTypes cannot be empty!");

            if (_criteria.SearchTerms.Count == 0)
                return;

            if (stringBuilder.Length > 0)
                stringBuilder.Append(" AND ");

            stringBuilder.Append("(");
            if (!_criteria.SearchByUsageType)
            {
                foreach (UsageType usageType in Enum.GetValues(typeof(UsageType)))
                {
                    _criteria.UsageTypes.Add(usageType);
                }
            }
            int searchTermsLeft = _criteria.SearchTerms.Count;
            foreach (string searchTerm in _criteria.SearchTerms)
            {
                //stringBuilder.Append("(");
                bool notCondition = false;
                string searchTermEscaped = searchTerm;
                if (searchTermEscaped.StartsWith("-"))
                {
                    notCondition = true;
                    searchTermEscaped = searchTerm.Substring(1);
                }
                searchTermEscaped = EscapeSpecialCharacters(searchTermEscaped);
                int usageTypesLeft = _criteria.UsageTypes.Count;
                foreach (UsageType usageType in _criteria.UsageTypes)
                {
                    if (notCondition)
                        stringBuilder.Append(" NOT ");
                    SingleUsageTypeCriteriaToString(stringBuilder, usageType, searchTermEscaped);
                    if (usageTypesLeft > 1 && !notCondition)
                    {
                        stringBuilder.Append(" OR ");
                    }
                    --usageTypesLeft;
                }
                if (searchTermsLeft > 1)
                {
                    stringBuilder.Append(" OR ");
                }
                --searchTermsLeft;
            }
            stringBuilder.Append(")");
        }

        private string EscapeSpecialCharacters(string searchTerm)
        {
            var escapedSearchTermBuilder = new StringBuilder(searchTerm);
            //escapedSearchTermBuilder.Replace("\\", "\\\\");
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
            //escapedSearchTermBuilder.Replace("\"", "\\\"");
            escapedSearchTermBuilder.Replace("~", "\\~");
            escapedSearchTermBuilder.Replace(":", "\\:");
            return escapedSearchTermBuilder.ToString();
        }

        private void SingleUsageTypeCriteriaToString(StringBuilder stringBuilder, UsageType usageType, string searchTerm)
        {
            switch (usageType)
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
                case UsageType.RawSourceCode:
                    stringBuilder.Append(SandoField.Source.ToString() + ":");
                    stringBuilder.Append(searchTerm);
                    AppendBoostFactor(stringBuilder, SandoField.Source.ToString());
                    break;
                default:
                    throw new IndexerException(TranslationCode.Exception_General_UnrecognizedEnumValue, null, "UsageType");
            }
        }

        private void AppendBoostFactor(StringBuilder stringBuilder, string fieldName)
        {
            if (_queryWeights.ContainsKey(fieldName) && _queryWeights[fieldName] != 1)
            {
                stringBuilder.Append("^");
                stringBuilder.Append(_queryWeights[fieldName]);
            }
        }
    }
}
