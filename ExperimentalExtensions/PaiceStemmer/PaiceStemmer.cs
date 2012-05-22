using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sando.ExperimentalExtensions.PaiceStemmer
{
	/*
	 *  Paice stemmer (translated from Java) from http://www.comp.lancs.ac.uk/computing/research/stemming
	 */
	public class PaiceStemmer
	{
		private const string ruleFile = "\\stemrules.txt";

		private List<string> ruleTable; 	
		private int[] ruleIndex; 	
		private bool preStrip;

		public PaiceStemmer(string ruleDir, string pre)
		{
			ruleTable = new List<string>();
			ruleIndex = new int[26];
			preStrip = false;
			if (pre == "/p")
			{
				preStrip=true;
			}
			ReadRules(ruleDir + ruleFile);
		}

		/****************************************************************
		*Method:		stripAffixes									*
		*Returns:		String											*
		*Recievs:		String str										*
		*Purpose:		prepares string and calls stripPrefixes			*
		*				and stripSuffixes	 							*
		****************************************************************/
		public String stripAffixes(string str)
		{
			str = str.ToLower(); //change all letters in the input to lowercase
			str = Clean(str); // remove all chars from string that are not a letter or a digit (why digit?)
			if((str.Length > 3) && (preStrip)) //if str's length is greater than 2 then remove prefixes
			{
				str = stripPrefixes(str);
			}
			if(str.Length > 3) // if str is not null remove suffix
			{
				str = stripSuffixes(str);
			}
			return str;
		} 

		/************************************************************************
		*Method:		ReadRules												*
		*Returns:		void													*
		*Receives:																*
		*Purpose:	read rules in from stemRules and enters them   				*
		*			into ruleTable, ruleIndex is set up to provide 				*
		*			faster access to relevant rules.							*
		************************************************************************/
		private void ReadRules(String stemRules)
		{
			int ruleCount = 0;
			int j = 0;

			//Acquire each rule in turn. They each take up one line
			try
			{
				StreamReader stream = new StreamReader(stemRules);
				string line = " ";
				try
				{
					while((line = stream.ReadLine()) != null)
					{
						ruleCount++;
						j = 0;
						string rule = "";
						while((j < line.Length) && (line.ElementAt(j) != ' '))
						{
							rule += line.ElementAt(j);
							j++;
						}
						ruleTable.Add(rule);
					}
				}
				catch(Exception e)
				{
					Console.WriteLine("File error during reading rules" + e);
				}

				stream.Close();
			}
			catch(Exception )
			{
				Console.WriteLine("Input file " + stemRules + " not found");
			}


			// Now assign the number of the first rule that starts with each letter
			// (if any) to an alphabetic array to facilitate selection of sections
			char ch = 'a';
			for(j = 0; j < 25; j++)
			{
				ruleIndex[j] = 0;
			}

			for(j = 0; j < (ruleCount - 1); j++)
			{
				while(((string)ruleTable.ElementAt(j)).ElementAt(0) != ch)
				{
					ch++;
					ruleIndex[charCode(ch)] = j;
				}
			}
		}

		/************************************************************************
		*Method:		stripSuffixes											*
		*Returns:		String 													*
		*Recievs:		String word												*
		*Purpose:		strips suffix off word and returns stem using 			*
		*				paice stemming algorithm								*
		************************************************************************/
		private String stripSuffixes(String word)
		{
			//integer variables 1 is positive, 0 undecided, -1 negative equiverlent of pun vars positive undecided negative
			int ruleok = 0;
			int Continue = 0;
			//integer varables
			int pll = 0;	//position of last letter
			int xl;		//counter for nuber of chars to be replaced and length of stemmed word if rule was aplied
			int pfv;	//poition of first vowel
			int prt;	//pointer into rule table
			int ir;		//index of rule
			int iw;		//index of word
			//char variables
			char ll;	// last letter
			//String variables eqiverlent of tenchar variables
			String rule = "";	//varlable holding the current rule
			String stem = "";   // string holding the word as it is being stemmed this is returned as a stemmed word.
			//boolean varable
			bool intact = true; //intact if the word has not yet been stemmed to determine a requirement of some stemming rules

			//set stem = to word
			stem = Clean(word.ToLower());

			// set the position of pll to the last letter in the string
			pll = 0;
			//move through the word to find the position of the last letter before a non letter char
			while((pll + 1 < stem.Length) && ((stem.ElementAt(pll + 1) >= 'a') && (stem.ElementAt(pll + 1) <= 'z')))
			{
				pll++;
			}

			if(pll < 1)
			{
				Continue = -1;
			}
			//find the position of the first vowel
			pfv = FirstVowel(stem, pll);

			iw = stem.Length - 1;

			//repeat until continue == negative ie. -1
			while(Continue != -1)
			{
				Continue = 0;		//SEEK RULE FOR A NEW FINAL LETTER
				ll = stem.ElementAt(pll);	//last letter

				//Check to see if there are any possible rules for stemming
				if((ll >= 'a') && (ll <= 'z'))
				{
					prt = ruleIndex[charCode(ll)];	 //pointer into rule-table
				}
				else
				{
					prt = -1;//0 is a vaild rule
				}
				if(prt == -1)
				{
					Continue = -1; //no rule available
				}

				if(Continue == 0)
				//THERE IS A POSSIBLE RULE (OR RULES) : SEE IF ONE WORKS
				{
					rule = (String)ruleTable.ElementAt(prt);	// Take first rule
					while(Continue == 0)
					{
						ruleok = 0;
						if(rule.ElementAt(0) != ll)
						{
							//rule-letter changes
							Continue = -1;
							ruleok = -1;
						}
						ir = 1;				//index of rule: 2nd character
						iw = pll - 1;			//index of word: next-last letter

						//repeat untill the rule is not undecided find a rule that is acceptable
						while(ruleok == 0)
						{
							if((rule.ElementAt(ir) >= '0') && (rule.ElementAt(ir) <= '9'))  //rule fully matched
							{
								ruleok = 1;
							}
							else if(rule.ElementAt(ir) == '*')
							{
								//match only if word intact
								if(intact)
								{
									ir = ir + 1;	       // move forwards along rule
									ruleok = 1;
								}
								else
								{
									ruleok = -1;
								}
							}
							else if(rule.ElementAt(ir) != stem.ElementAt(iw))
							{
								// mismatch of letters
								ruleok = -1;
							}
							else if(iw <= pfv)
							{
								//insufficient stem remains
								ruleok = -1;
							}
							else
							{
								//  move on to compare next pair of letters
								ir = ir + 1;	      // move forwards along rule
								iw = iw - 1;	      // move backwards along word
							}
						}

						//if the rule that has just been checked is valid
						if(ruleok == 1)
						{
							//  CHECK ACCEPTABILITY CONDITION FOR PROPOSED RULE
							xl = 0;		//count any replacement letters
							while(!((rule.ElementAt(ir + xl + 1) >= '.') && (rule.ElementAt(ir + xl + 1) <= '>')))
							{
								xl++;
							}
							xl = pll + xl + 48 - ((int)(rule.ElementAt(ir)));
							// position of last letter if rule used
							if(pfv == 0)
							{
								//if word starts with vowel...
								if(xl < 1)
								{
									// ...minimal stem is 2 letters
									ruleok = -1;
								}
								else
								{
									//ruleok=1; as ruleok must alread be positive to reach this stage
								}
							}
							//if word start swith consonant...
							else if((xl < 2) | (xl < pfv))
							{
								ruleok = -1;
								// ...minimal stem is 3 letters...
								// ...including one or more vowel
							}
							else
							{
								//ruleok=1; as ruleok must alread be positive to reach this stage
							}
						}
						// if using the rule passes the assertion tests
						if(ruleok == 1)
						{
							//  APPLY THE MATCHING RULE
							intact = false;
							// move end of word marker to position...
							// ... given by the numeral.
							pll = pll + 48 - ((int)(rule.ElementAt(ir)));
							ir++;
							stem = stem.Substring(0, (pll + 1));


							// append any letters following numeral to the word
							while((ir < rule.Length) && (('a' <= rule.ElementAt(ir)) && (rule.ElementAt(ir) <= 'z')))
							{
								stem += rule.ElementAt(ir);
								ir++;
								pll++;
							}

							//if rule ends with '.' then terminate
							if((rule.ElementAt(ir)) == '.')
							{
								Continue = -1;
							}
							else
							{
								//if rule ends with '>' then Continue
								Continue = 1;
							}
						}
						else
						{
							//if rule did not match then look for another
							prt = prt + 1;		// move to next rule in RULETABLE
							rule = (String)ruleTable.ElementAt(prt);
							if(rule.ElementAt(0) != ll)
							{
								//rule-letter changes
								Continue = -1;
							}
						}
					}
				}
			}
			return stem;
		}

		/****************************************************************
		*Method:		FirstVowel										*
		*Returns:		int 											*
		*Recieves:		String word, int last							*
		*Purpose:		checks lower-case word for position of			*
		*				the first vowel									*
		****************************************************************/
		private int FirstVowel(String word, int last)
		{
			int i = 0;
			if((i < last) && (!(vowel(word.ElementAt(i), 'a'))))
			{
				i++;
			}
			if(i != 0)
			{
				while((i < last) && (!(vowel(word.ElementAt(i), word.ElementAt(i - 1)))))
				{
					i++;
				}
			}
			if(i < last)
			{
				return i;
			}
			return last;
		}

		/****************************************************************
		*Method:		vowel											*
		*Returns:		boolean											*
		*Recieves:		char ch, char prev								*
		*Purpose:		determin whether ch is a vowel or not 			*
		*				uses prev determination when ch == y				*
		****************************************************************/
		private bool vowel(char ch, char prev)
		{
			switch(ch)
			{
				case 'a':
				case 'e':
				case 'i':
				case 'o':
				case 'u':
					return true;
				case 'y':
					{
						switch(prev)
						{
							case 'a':
							case 'e':
							case 'i':
							case 'o':
							case 'u':
								return false;
							default:
								return true;
						}
					}
				default:
					return false;
			}
		}

		/****************************************************************
		*Method:		charCode										*
		*Returns:		int												*
		*Recievs:		char ch											*
		*Purpose:		returns the relavent array index for  			*
		*				specified char 'a' to 'z'						*
		****************************************************************/
		private int charCode(char ch)
		{
			return ((int)ch) - 97;
		}

		/********************************************************
		*Method:		stripPrefixes							*
		*Returns:		String									*
		*Recieves:		String str								*
		*Purpose:		removes prefixes so that suffix			*
		*				removal can comence						*
		********************************************************/
		private String stripPrefixes(String str)
		{
			String[] prefixes = { 	"kilo"  ,
									"micro" ,
									"milli" ,
									"intra" ,
									"ultra" ,
									"mega"  ,
									"nano"  ,
									"pico"  ,
									"pseudo" };

			int last = prefixes.Length;

			for(int i = 0; i < last; i++)
			{
				if((str.StartsWith(prefixes[i])) && (str.Length > prefixes[i].Length))
				{
					str = str.Substring(prefixes[i].Length);
					return str;
				}
			}
			return str;
		}

		/********************************************************
		*Method:		Clean									*
		*Returns:		String									*
		*Recieves:		String str								*
		*Purpose:		remove all non letter or digit			*
		*			characters from srt and return				*
		********************************************************/
		private String Clean(String str)
		{
			int last = str.Length;
			String temp = "";
			for(int i = 0; i < last; i++)
			{
				if((str.ElementAt(i) >= 'a') & (str.ElementAt(i) <= 'z'))
				{
					temp += str.ElementAt(i);
				}
			}
			return temp;
		} 
	}
}
