using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace ErikEJ.SqlCeToolbox.Helpers
{
    [DescriptionAttribute("SQL Server Compact Toolbox Options")]
    public class UserOptions
    {
        [CategoryAttribute("Schema Diff"),
        DescriptionAttribute("If true, will add DROP TABLE statments for tables only in target"),
        DefaultValueAttribute(false)]
        public bool DropTargetTables { get; set; }

        [CategoryAttribute("Query Results"),
        DescriptionAttribute("If true, will show binary values a binary string in results (slower)"),
        DefaultValueAttribute(false)]
        public bool ShowBinaryValuesInResult { get; set; }

        [CategoryAttribute("Query Results"),
        DescriptionAttribute("If true, will show the results in a grid (slower)"),
        DefaultValueAttribute(false)]
        public bool ShowResultInGrid { get; set; }

        [CategoryAttribute("Object Tree"),
        DescriptionAttribute("If true, will show the __ExtendedProperties metadata table"),
        DefaultValueAttribute(false)]
        public bool DisplayDescriptionTable { get; set; }

        [CategoryAttribute("Documentation"),
        DescriptionAttribute("If true, will also include tables beginning with __ "),
        DefaultValueAttribute(false)]
        public bool IncludeSystemTablesInDocumentation { get; set; }

        [CategoryAttribute("Edit Table Data"),
        DescriptionAttribute("Sets number of rows loaded by Edit Table grid"),
        DefaultValueAttribute(false)]
        public int MaxRowsToEdit { get; set; }


    }

}
