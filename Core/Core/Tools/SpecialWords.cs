using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sando.Core.Tools
{
    public static class SpecialWords
    {
        public static String[] NonInformativeWords()
        {
            var list = new List<string>();
            list.AddRange(CSharpKeyWords());
            list.AddRange(FrequentEnglishWordsTopHundred());
            list.AddRange(MostCommonVerbs());
            return list.Distinct().ToArray();
        }


        public static string[] CSharpKeyWords()
        {
            return @"abstract	event	new	struct
                as	explicit	null	switch
                base	extern	object	this
                bool	false	operator	throw
                break	finally	out	true
                byte	fixed	override	try
                case	float	params	typeof
                catch   for	private	uint
                char    foreach	protected	ulong
                checked	goto	public	unchecked
                class	if	readonly	unsafe
                const	implicit	ref	ushort
                continue	in	return	using
                decimal	int	sbyte	virtual
                default	interface	sealed	volatile
                delegate	internal	short	void
                do	is	sizeof	while
                double	lock	stackalloc	 
                else    long	static	 
                enum namespace string var".Split();
        }


        public static string[] FrequentEnglishWordsTopHundred()
        {
            return @"the of and a to in is you that it he was any where when what
            for on are as with his they I at be this have from 
            or one had by word but not what all were we when your can
            said there use an each which she do how their if will good
            up other about out many then them these so some her not non only
            would make like him into time has look two more write go see number no way
            could people my than first water been call who its now did get come made may
            ".Split();
        }

        public static string[] MostCommonVerbs()
        {
            return @"accept care could enjoy happen lead open reduce settle teach account carry count examine hate learn order refer shake tell
            achieve catch cover exist have leave ought reflect shall tend act cause create expect head lend own refuse share test add change cross 
            experience hear let pass regard shoot thank admit charge cry explain help lie pay relate should think affect check cut express hide like
            perform release shout throw afford choose damage extend hit limit pick remain show touch agree claim dance face hold link place remember shut
            train aim clean deal fail hope listen plan remove sing travel allow clear decide fall hurt live play repeat sit treat answer climb deliver 
            fasten identify look point replace sleep try appear close demand feed imagine lose prefer reply smile turn apply collect deny feel improve love
            prepare report sort understand argue come depend fight include make present represent sound use arrange commit describe fill increase manage 
            press require speak used to arrive compare design find indicate mark prevent rest stand visit ask complain destroy finish influence matter 
            produce result start vote attack complete develop fit inform may promise return state wait avoid concern die fly intend mean protect reveal 
            stay walk base confirm disappear fold introduce measure prove ring stick want be connect discover follow invite meet provide rise stop warn 
            beat consider discuss force involve mention publish roll study wash become consist divide forget join might pull run succeed watch begin 
            contact do forgive jump mind push save suffer wear believe contain draw form keep miss put say suggest will belong continue dress found kick 
            move raise see suit win break contribute drink gain kill must reach seem supply wish build control drive get knock need read sell support 
            wonder burn cook drop give know notice realize send suppose work buy copy eat go last obtain receive separate survive worry call correct enable
            grow laugh occur recognize serve take would can cost encourage handle lay offer record set talk write".Split();
        }


        public static Dictionary<string, string> HyperCommonAcronyms()
        {
            var map = new Dictionary<String, String>
            {
                {"int", "integer"},
                {"impl", "implement"},
                {"obj", "object"},
                {"pos", "position"},
                {"init", "initial"},
                {"len", "length"},
                {"attr", "attribute"},
                {"num", "number"},
                {"env", "environment"},
                {"val", "value"},
                {"str", "string"},
                {"doc", "document"},
                {"buf", "buffer"},
                {"ctx", "context"},
                {"msg", "message"},
                {"var", "variable"},
                {"param", "parameter"},
                {"decl", "declare"},
                {"arg", "argument"},
                {"rect", "rectangle"},
                {"expr", "express"},
                {"dir", "directory"}
            };
            return map;
        }
    }
}
