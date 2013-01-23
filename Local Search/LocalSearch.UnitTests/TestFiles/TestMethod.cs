using System;
using System.Windows;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace RobotConfigServe2
{ 
    public partial class ConfigManip
    {       

        public ConfigManip(string configListFileName)
        {
            configCnt = 0;

            //get the "configWorkspaceRoot"
	        int pos = configListFileName.LastIndexOf('\\');
	        configWorkspaceRoot = configListFileName.Substring(0,pos);

            if (!File.Exists(configListFileName))
            {
                MessageBox.Show("Can't find file " + configListFileName, "ERROR");
                return;
            }

            StreamReader configListFile = new StreamReader(configListFileName);
            {
                string line;
	            //read through the configListFile
                while((line = configListFile.ReadLine()) != null)
                {
                    string configuration = line;
                    configList.Add(configuration);
                    configCnt++;
                }	            

	            configListFile.Close();
            }
        }

        #region class members definition
        private string configWorkspaceRoot;
        private List<string> configList = new List<string>();
        private List<string> configSelect = new List<string>();
        private List<string> files_in_build = new List<string>();
        private List<int> allSelections = new List<int>();
        private uint configCnt;

        private List<string> fileImpacted = new List<string>();
        private List<int> lineCntImpactedPerFile = new List<int>();

        private List<string> funcImpacted = new List<string>();
        private List<int> lineCntImpactedPerFunc = new List<int>();

        private string code_change_imp_file;
        private List<int> configIndexImpacted = new List<int>();
        private List<string> configImpacted = new List<string>();
        #endregion             

    }

}








