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
        static string toUpper(string str) 
        {
            return str.ToUpper();
        }

        static string convertInt(int number)
        {
            return System.Convert.ToString(number);
        }

        static string replaceCharInStr(string origStr, char to_be_replaced, char replace_with)
        {
           return origStr.Replace(to_be_replaced,replace_with);
        }

        static string removeCharInStr(string origStr, char to_be_removed)
        {   
            string removed = new string(to_be_removed,1);
            return origStr.Replace(removed,"");
        }

        public ConfigManip()
        {
        }

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


        public List<string> displayConfig()
        {
            return configList;
        }
        
        public void getConfigSelect(List<string> configSelectExt)
        {
            configSelect = configSelectExt;
        }

        public void getChangeImpact(string codeChangeImpFile)
        {
            code_change_imp_file = codeChangeImpFile;
        }

        #region main functionalities

        #region scenario 1 -- given configuration, find its impact on code 
        public int genConfigImp(string workspacePos)   
        {
            // get all corresponding selections in code for all configurations of interest
	        if (getChangeSets(workspacePos, 1) == -1)
		        return -1;

            // get files in build, store them in vector "files_in_build"
            if (getFileInBuild(workspacePos) == -1)
                return -1;

            //	generate impact at line level by calling external application: ImpactCompare.exe
	        genMultiSelectImp(workspacePos);
	
            // generate impact at file level from the file (impact at line level) generated above
            if (genImpUnionFile(workspacePos) == -1)
                return -1;

            // generate impact at file level that will be feed into MasaiCode for visiualization
            if (genFileImp(workspacePos) == -1)
                return -1;

            // generate impact at func level from the file (impact at line level) generated above	        
	        if (genImpUnionFunc(workspacePos) == -1)
                return -1 ;

            // generate impact at func level that will be feed into MasaiCode for visiualization
            if(genFuncImp(workspacePos) == -1)
                return -1;

            MessageBox.Show(
                //"Impacted Files and Functions are created as " 
                //+ workspacePos 
                //+ "\\imp_union_filesImpacted.csv and imp_union_funcsImpacted.csv"
                //+ Environment.NewLine + 
                "Visualization inputs are created in "
                +workspacePos + "\\MosaiCodeInputs."
                + Environment.NewLine + "Exits normally.", "Progress");

            //clean up
            string toRemoveFile1 = workspacePos + "\\" + "imp_union_linesImpacted.csv";
            if (!File.Exists(toRemoveFile1))           
                toRemoveFile1 = "imp_union_linesImpacted.csv";
            if (File.Exists(toRemoveFile1))
                File.Delete(toRemoveFile1);

            string toRemoveFile2 = workspacePos + "\\" + "imp_intersection_linesImpacted.csv";
            if (!File.Exists(toRemoveFile2))
                toRemoveFile2 = "imp_intersection_linesImpacted.csv";
            if (File.Exists(toRemoveFile2))
                File.Delete(toRemoveFile2);

            string toRemoveFile3 = workspacePos + "\\" + "imp_impactCompare_statistics";
            if (!File.Exists(toRemoveFile3))
                toRemoveFile3 = "imp_impactCompare_statistics";
            if (File.Exists(toRemoveFile3))
                File.Delete(toRemoveFile3);

            string toMoveFile1 = workspacePos + "\\" + "imp_union_filesImpacted.csv";
            if (File.Exists(toMoveFile1))
                File.Delete(toMoveFile1);

            string toMoveFile2 = workspacePos + "\\" + "imp_union_funcsImpacted.csv";
            if (File.Exists(toMoveFile2))
                File.Delete(toMoveFile2);

	        return 0;
        }
        #endregion

        #region scenario 2 -- given impact of code change, find configurations impacted

        public int findChangeImpConfig(string workspacePos)
        {
            // get all corresponding selections in code for all configurations of interest
            if (getChangeSets(workspacePos, 2) == -1)
                return -1;

            var prg = new Progress();
            prg.Show();
            prg.setProgressInit(allSelections.Count);

            foreach (int i in allSelections)
            {
                bool isImpacted = genSingleConfigImp(workspacePos, i);
                if(isImpacted)
                    configIndexImpacted.Add(i);

                prg.showProgress();
            }

            prg.Close();

            // output configurations impacted by code change
            if(configIndexImpacted.Count > 0)
                getConfigImpacted(workspacePos);

            if (configImpacted.Count == 0)
                MessageBox.Show("There is no impacted configurations.");
            else
            {
                string strConifgImpacted = "";
                Dictionary<string, bool> dit = new Dictionary<string, bool>();
                for (int i = 0; i < configImpacted.Count; i++)
                {                    
                    if (! dit.ContainsKey(configImpacted[i])) //avoid duplication
                    {
                        dit.Add(configImpacted[i], true);
                        strConifgImpacted += configImpacted[i] + "\n";
                    }
                }

                MessageBox.Show("Impacted Configurations are: \n" + strConifgImpacted, "Report");
            }

            

            return 0;
        }

        #endregion

        #endregion


        public int genConfigImp()
        {
            return genConfigImp(configWorkspaceRoot);
        }

        public int findChangeImpConfig()
        {
            return findChangeImpConfig(configWorkspaceRoot);
        }

        public int getConfigImpacted(string workspacePos)
        {
            //reverse mapping --- impacted configurations
            // [in] configIndexImpacted
            // [out] configImpacted
            string fullmapFileName = "";

            //to avoid hard code
            string[] files = Directory.GetFiles(workspacePos);
            foreach (string file in files)
            {
                string file2 = file.Substring(workspacePos.Length + 1);
                if (file2.StartsWith("config_select_mapping") && file2.EndsWith(".txt"))
                {
                    fullmapFileName = file;
                    break;
                }
            }

            if (!File.Exists(fullmapFileName))
            {
                MessageBox.Show("Missing config-changeset-mapping file.", "ERROR");
                return -1;
            }    

            for (int i = 0; i < configIndexImpacted.Count; i++)
            {
                int impacted = configIndexImpacted[i];

                StreamReader mapFile = new StreamReader(fullmapFileName);
                string dummyLine;
                dummyLine = mapFile.ReadLine();
                string line;
                while ((line = mapFile.ReadLine()) != null)
                {
                    string[] words = line.Split('\t');
                    string _domain = words[0];
                    string _type = words[1];
                    int _selection = System.Convert.ToInt32(words[2]);

                    //if (_domain == "")
                    //    continue;

                     if (impacted == _selection)
                     {
                         configImpacted.Add(_domain + ":" + _type); //may contatin duplication
                         break;
                     }
                }
                mapFile.Close();
            }

            return 0;

        }         


        private bool genSingleConfigImp(string workspacePos, int configIndex)
        {
            string instruction = "\"" + workspacePos + "\\" + "ImpactCompare.exe" + "\"";
            string[] parameters = new string[2];
            parameters[0] = "\"" + workspacePos + "\\" + convertInt(configIndex) 
                + "\\imp_linesImpacted.csv" + "\"";
            parameters[1] = "\"" + code_change_imp_file + "\"";                       

            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.RedirectStandardError = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = instruction;
            process.StartInfo.Arguments = String.Join(" ", parameters);
            try
            {
                process.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                //return false;
            }

            while (!process.HasExited)
            {
                //    MessageBox.Show("a");
            }
            
            //string removeFile1 = workspacePos + "\\" + "imp_intersection_linesImpacted.csv";
            //string removeFile2 = workspacePos + "\\" + "imp_impactCompare_statistics";
            //string removeFile3 = workspacePos + "\\" + "imp_union_linesImpacted.csv";
            string removeFile1 = "imp_intersection_linesImpacted.csv";
            string removeFile2 = "imp_impactCompare_statistics";
            string removeFile3 = "imp_union_linesImpacted.csv";
            //File.Delete(removeFile1);
            //File.Delete(removeFile2);
            //File.Delete(removeFile3);

            //readin "imp_impactCompare_statistics" file and see if the union is empty
            if (!File.Exists(removeFile2))
            {
                MessageBox.Show("Missing imp_impactCompare_statistics.", "ERROR");
                return false;
            }

            StreamReader statFile = new StreamReader(removeFile2);
            string line;
            while ((line = statFile.ReadLine()) != null)
            {
                if (line.Contains("Number of common files: "))
                {
                    int idx = line.IndexOf(':');
                    string line2 = line.Substring(idx + 1);
                    line2.Trim('\n');
                    int impCnt = System.Convert.ToInt32(line2);

                    statFile.Close();
                    File.Delete(removeFile1);
                    File.Delete(removeFile2);
                    File.Delete(removeFile3);

                    if (impCnt > 0)
                        return true;
                    else
                        return false;                   
                }
            }

            statFile.Close();
            MessageBox.Show("Wrong imp_impactCompare_statistics file.", "ERROR");

            return false;
        }
        
        private void genMultiSelectImp(string workspacePos)
        {
            string instruction = "\"" + workspacePos + "\\" + "ImpactCompare.exe" + "\"";

            //progress bar
            var prg = new Progress();            
            prg.Show();
            prg.setProgressInvisible();

            int numOfPara = allSelections.Count;
            if (numOfPara < 2)
            {
                numOfPara = 2;
                allSelections.Add(allSelections[0]);
            }

            string[] parameters = new string[numOfPara];

            for (int i = 0; i < numOfPara; i++) // allSelections.size() must >0
            {
                parameters[i] = "\"" + workspacePos + "\\" + convertInt(allSelections[i]) + "\\imp_linesImpacted.csv" + "\"";                
            }
            
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = false;
            process.StartInfo.RedirectStandardError = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = instruction;
            process.StartInfo.Arguments = String.Join(" ", parameters);
            try
            {
                process.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);                
            }

            while (!process.HasExited)
            {
            //    MessageBox.Show("a");
            }

            System.Threading.Thread.Sleep(3000); //to avoid progress bar flashing (too short progress)
            prg.Close();
            
        }

	    private int getChangeSets(string workspacePos, int toolOpt)  //[IMPORTANT] this is system dependent !!!
        {
            // look for configSelect in config-selection
            // e.g. workspacePos + "\\config_select_mapping_robotics_rel5_11.0160.txt";
            string fullmapFileName = ""; 

            //to avoid hard code
            string[] files = Directory.GetFiles(workspacePos);
            foreach (string file in files)
            {
                string file2 = file.Substring(workspacePos.Length+1);
                if (file2.StartsWith("config_select_mapping") && file2.EndsWith(".txt"))
                {
                    fullmapFileName = file;
                    break;
                }
            }

            if (!File.Exists(fullmapFileName))
            {
                MessageBox.Show("Missing config-changeset-mapping file.", "ERROR");
                return -1;
            }
	        StreamReader mapFile = new StreamReader(fullmapFileName);           
	
	        bool[] isIdentifiedConfig = new bool[configSelect.Count]; //if there is corresponding mapping identified in code
	
	        string dummyLine;
	        dummyLine = mapFile.ReadLine();

            string line;
            while((line = mapFile.ReadLine()) != null)
            {
                string[] words = line.Split('\t');
	            string _domain = words[0];
		        string _type = words[1];
		        int _selection = System.Convert.ToInt32(words[2]);

		        if(_domain == "")
			        continue;

		        for(int i=0; i<configSelect.Count; i++)
		        {
			        string select = configSelect[i];
			        int pos = select.IndexOf(":");
			        string domain_sel = select.Substring(0,pos);
			        string type_sel = select.Substring(pos+1, select.Length -(pos+1));
			        if((_domain == domain_sel) && (_type == type_sel))
			        {
				        isIdentifiedConfig[i] = true;
				        allSelections.Add(_selection);
				        break;  
			        }
		        }   

	        }

	        mapFile.Close();

	        for(int i=0; i<configSelect.Count; i++)
	        {
		        if(isIdentifiedConfig[i] == false)
                {
			        if (toolOpt == 1)
			            MessageBox.Show("Configurable Option " + configSelect[i] + " is not identified in code.", "WARN" );
			        //return -1;
		        }
	        }

	        if(allSelections.Count == 0)
	        {
		        MessageBox.Show("No mapping to code.", "EXCEPTION");
		        return -1;
	        }

	        return 0;
         }

	    private int getFileInBuild(string workspacePos)
        {
            string filesInBuildFilePath = workspacePos + "\\" + "imp_filesInBuild.csv";
            if (!File.Exists(filesInBuildFilePath))
            {
                MessageBox.Show("Missing imp_filesInBuild.csv file.", "ERROR");
                return -1;
            }
	        StreamReader filesInBuildFile = new StreamReader(filesInBuildFilePath);
	        
            filesInBuildFile.ReadLine();
	        
            string fileLine = "";

	        while( (fileLine = filesInBuildFile.ReadLine()) != null)
	        {
		        if(fileLine == "")
			        continue;

		        int pos = fileLine.IndexOf(",");
		        string fileInBuild = fileLine.Substring(0,pos);
		        files_in_build.Add(fileInBuild);
	        }

	        filesInBuildFile.Close();
            return 0;
        }
	    
        private int genFileImp(string workspacePos)
        {
            //[in] all files, total number of lines: imp_userInterestedFiles.csv (if not given, interest in all)
	        //[in] files in build: vector "files_in_build"
	        //[in] files impacted and its line numbers generated above: imp_union_filesImpacted.csv --> vectors "fileImpacted" and "lineCntImpactedPerFile"
	        //[out] file, in_build, # of lines, # of impacte lines, impact ratio
                       
            string allFilesPath = workspacePos + "\\" + "imp_userInterestedFiles.csv";
            if (!File.Exists(allFilesPath))
            {
                MessageBox.Show("Missing imp_userInterestedFiles.csv.", "ERROR");
                return -1;                
            }
	        StreamReader allFiles = new StreamReader(allFilesPath);

            MessageBox.Show("Generate File level impact ... Wait ...", "Progress");

            if (!Directory.Exists(workspacePos + "\\MosaiCodeInputs"))
                Directory.CreateDirectory(workspacePos + "\\MosaiCodeInputs");

            string fileImpFilePath = workspacePos + "\\MosaiCodeInputs\\fileImpact_MasaicodeInput.csv";
            StreamWriter fileImpFile = new StreamWriter(fileImpFilePath);

            fileImpFile.WriteLine("ID,File,InBuild,NumOfLines,NumOfImpactedLines,ImpactRatio");
		
	        uint count = 1;
	
            string line = "";
            while( (line = allFiles.ReadLine()) != null)
	        {	 
                if (line == "")
			        continue;

                string file = "";
		        string num_line = "";
		        string is_in_build = "";
		        int num_line_imp = 0;
		        float ratio_imp = 0;
                
		        int pos = line.IndexOf(",");
		        file = line.Substring(0,pos);
		        num_line = line.Substring(pos+1);

                if(files_in_build.FindIndex(item => item == file)==-1)
                {
                    is_in_build = "0";
			        num_line_imp = 0; //-1;
			        ratio_imp = 0; //-1;
                }
		        else //is in build, look up for impact 
		        {
			        is_in_build = "1";
			        int it_strVec = fileImpacted.FindIndex(item => item == file);                        
			        if(it_strVec == -1) //is not impacted
			        {
				        num_line_imp = 0;
				        ratio_imp = 0;
			        }
			        else
			        {			         
				        num_line_imp = lineCntImpactedPerFile[it_strVec];
				        ratio_imp = (float)(num_line_imp) / float.Parse(num_line) * (float)(100);
			        }

		        }   

		        file = replaceCharInStr(file,'\\','/');  
                string count2 = System.Convert.ToString(count++);
                fileImpFile.WriteLine(count2+","+file+","+is_in_build+","+num_line+","+num_line_imp+","+ratio_imp);
		        
	        } //end of browsing all files	

	        allFiles.Close();
	        fileImpFile.Close();

            return 0;
        } 
	    
        private int genFuncImp(string workspacePos)
        {
            //[in] all files, all funcs, total number of lines: imp_userInterestedFuncs.csv (not necessary)
	        //[in] files in build: vector "files_in_build"
	        //[in] files+funcs impacted and its line numbers generated above: imp_union_funcsImpacted.csv --> vectors "funcImpacted" and "lineCntImpactedPerFunc"
	        //[out] file, in_build, func, # of lines, # of impacte lines, impact ratio
		
	        string allFuncsPath = workspacePos + "\\" + "imp_userInterestedFuncs.csv";
            if (!File.Exists(allFuncsPath))
            {
                //MessageBox.Show("Missing imp_userInterestedFuncs.csv.", "WARNING");
                return -2;
            }
	        StreamReader allFuncs = new StreamReader(allFuncsPath);
            //if(! allFuncs.is_open())
            //    return;

            MessageBox.Show("Generate function level impact ... Wait ...", "Progress");
            //progress bar
            //var prg = new Progress();
            //prg.Show();

            string funcImpFilePath = workspacePos + "\\MosaiCodeInputs\\funcImpact_MasaicodeInput.csv";
            StreamWriter funcImpFile = new StreamWriter(funcImpFilePath);
            //if(! funcImpFile.is_open())
            //    return;
            funcImpFile.WriteLine("ID,File+Func,InBuild,NumOfLines,NumOfImpactedLines,ImpactRatio"); 

	        int count = 1;
            string line = "";
	        while( (line = allFuncs.ReadLine()) != null)
	        {
                if (line == "")
        			continue;
                
                string fileAfunc = "";
		        string file = "";
		        string func = "";
		        string num_line = "";
		        string is_in_build = "";
		        int num_line_imp = 0;
		        float ratio_imp = 0;
		
		        int pos = line.LastIndexOf(",");
		        fileAfunc = line.Substring(0,pos);
		        fileAfunc = removeCharInStr(fileAfunc,'"'); //function may be double quoted
		        num_line = line.Substring(pos+1);
		        pos = fileAfunc.IndexOf(",");
		        file = fileAfunc.Substring(0,pos);
		        func = fileAfunc.Substring(pos+1);

		        int it_strVec;
		        if( (it_strVec = files_in_build.FindIndex(item => item == file)) == -1) //is not in build
		        {
			        is_in_build = "0";
			        num_line_imp = 0; //-1;
			        ratio_imp = 0; //-1;
		        }
		        else //is in build, look up for impact 
		        {
			        is_in_build = "1";
			        it_strVec = funcImpacted.FindIndex(item => item == fileAfunc);
			        if(it_strVec == -1) //is not impacted
			        {
				        num_line_imp = 0;
				        ratio_imp = 0;
			        }
			        else
			        {    
				        num_line_imp = lineCntImpactedPerFunc[it_strVec];
				        ratio_imp = (float)(num_line_imp) / float.Parse(num_line) * (float)(100);
			        }

		        }

		        file = replaceCharInStr(file,'\\','/');
                string count2 = System.Convert.ToString(count++);
                funcImpFile.WriteLine(count2 + "," + file + "/" + func + "," + is_in_build + "," + num_line + "," + num_line_imp + "," + ratio_imp);  

	        } //end of browsing all files+funcs
            
	        allFuncs.Close();
	        funcImpFile.Close();
            return 0;
        }
	    
        private int genImpUnionFile(string workspacePos)
        {
            //[in] imp_union_linesImpacted.csv
	        //[out] imp_union_filesImpacted.csv

            //MessageBox.Show("Generate file level impact ... Wait ...", "Progress");

	        List<string> filesImpacted = new List<string>();
	        int itFilesImpacted;
	        List<int> numOfLineImpacted = new List<int>();

	        string unionLineFilePath = workspacePos + "\\" + "imp_union_linesImpacted.csv";         
            if (!File.Exists(unionLineFilePath))
            {
                unionLineFilePath = "imp_union_linesImpacted.csv";
                if (!File.Exists(unionLineFilePath))
                {
                    MessageBox.Show("Missing imp_union_linesImpacted.csv.", "ERROR");
                    return -1;
                }
            }

	        StreamReader unionLineFile = new StreamReader(unionLineFilePath);	        
            unionLineFile.ReadLine(); // dummyLine
            string line = "";
	        while( (line = unionLineFile.ReadLine()) != null)
	        {
				if (line == "")
		        	continue;

		        int pos = line.IndexOf(",");
		        string fileName = line.Substring(0,pos);
		
		        itFilesImpacted = filesImpacted.FindIndex(item => item == fileName);                    
		        if (itFilesImpacted != -1) {
				    numOfLineImpacted[itFilesImpacted]++;
                } else {
			        filesImpacted.Add(fileName);
			        numOfLineImpacted.Add(1);
                }
            }
	        unionLineFile.Close();

	        string unionFileFilePath = workspacePos + "\\" + "imp_union_filesImpacted.csv";
	        StreamWriter unionFileFile = new StreamWriter(unionFileFilePath);
	        
	        fileImpacted = filesImpacted;
	        lineCntImpactedPerFile = numOfLineImpacted;

	        unionFileFile.WriteLine("File_Name,Number_Line_Impacted");
	
            for(int i=0; i<filesImpacted.Count; i++)
	        {
		        unionFileFile.WriteLine(filesImpacted[i] + "," + System.Convert.ToString(numOfLineImpacted[i]));
	        }
	 
	        unionFileFile.Close();            

            return 0;
        }

	    
        private int genImpUnionFunc(string workspacePos)
        {
            //[in] imp_union_linesImpacted.csv
	        //[out] imp_union_funcsImpacted.csv

	        List<string> funcsImpacted = new List<string>();
	        int itFuncsImpacted;
	        List<int> numOfLineImpacted = new List<int>();

	        string unionLineFilePath = workspacePos + "\\" + "imp_union_linesImpacted.csv";
            if (!File.Exists(unionLineFilePath))
            {
                unionLineFilePath = "imp_union_linesImpacted.csv";
                if (!File.Exists(unionLineFilePath))
                {
                    MessageBox.Show("Missing imp_union_linesImpacted.csv.", "ERROR");
                    return -1;
                }
            }


	        StreamReader unionLineFile = new StreamReader(unionLineFilePath);
	        unionLineFile.ReadLine();
	        string line = "";
	        while( (line = unionLineFile.ReadLine()) != null)
	        {
				if (line == "")
		        	continue;
                
                int pos0 = line.IndexOf(",");
		        int pos = line.IndexOf(",",pos0+1);
		        string fileAfuncName = line.Substring(0,pos);
		        fileAfuncName = removeCharInStr(fileAfuncName,'"'); //function name may be double quoted
		
		        itFuncsImpacted = funcsImpacted.FindIndex(item => item == fileAfuncName);
                    
		        if (itFuncsImpacted != -1) {                
				    numOfLineImpacted[itFuncsImpacted]++;
                 } else {
			        funcsImpacted.Add(fileAfuncName);
			        numOfLineImpacted.Add(1);
		        }        
            }
	        unionLineFile.Close();


	        string unionFuncFilePath = workspacePos + "\\" + "imp_union_funcsImpacted.csv";
	        StreamWriter unionFuncFile = new StreamWriter(unionFuncFilePath);
	
	        funcImpacted = funcsImpacted;
	        lineCntImpactedPerFunc = numOfLineImpacted;

	        unionFuncFile.WriteLine("File_Name,Func_Name,Number_Line_Impacted");
	        for(int i=0; i<funcsImpacted.Count; i++)
	        {
		        int pos = funcsImpacted[i].IndexOf(",");
		        string fileName = funcsImpacted[i].Substring(0,pos);
		        string funcName = funcsImpacted[i].Substring(pos+1);
		        unionFuncFile.WriteLine(fileName + "," + funcName + "," + System.Convert.ToString(numOfLineImpacted[i]));
	        }
	 
	        unionFuncFile.Close();
            return 0;
        }

    }

}








