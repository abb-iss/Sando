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
            list.AddRange(StopWords());
            return list.Distinct().ToArray();
        }


        public static string[] CSharpKeyWords()
        {
            return @"abstract	event	new	struct
                as	explicit	null	switch
                base	extern	object	this
                bool	false	operator	throw
                break	finally	out	true system
                byte	fixed	override	try
                case	float	params	typeof
                catch   for	private	uint main
                char    foreach	protected	ulong
                checked	goto	public	unchecked
                class	if	readonly	unsafe
                const	implicit	ref	ushort
                continue	in	return	using error
                decimal	int	sbyte	virtual warning
                default	interface	sealed	volatile
                delegate	internal	short	void
                do	is	sizeof	while debug info
                double	lock	stackalloc logger
                else    long	static exception
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
                {"int", "integer"}, {"std", "standard"},
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

        public static string[] StopWords()
        {
            return @"a's able about above according accordingly across actually after afterwards again 
            against ain't all allow allows almost alone along already also although always am among amongst an and
            another any anybody anyhow anyone anything anyway anyways anywhere apart appear appreciate appropriate
            are aren't around as aside ask asking associated at available away awfully be became because become 
            becomes becoming been before beforehand behind being believe below beside besides best better between
            beyond both brief but by c'mon c's came can can't cannot cant cause causes certain certainly changes
            clearly co com come comes concerning consequently consider considering contain containing contains
            corresponding could couldn't course currently definitely described despite did didn't different do
            does doesn't doing don't done down downwards during each edu eg eight either else elsewhere enough 
            entirely especially et etc even ever every everybody everyone everything everywhere ex exactly example
            except far few fifth first five followed following follows for former formerly forth four from further 
            furthermore get gets getting given gives go goes going gone got gotten greetings had hadn't happens 
            hardly has hasn't have haven't having he he's hello help hence her here here's hereafter hereby herein 
            hereupon hers herself hi him himself his hither hopefully how howbeit however i'd i'll i'm i've ie 
            if ignored immediate in inasmuch inc indeed indicate indicated indicates inner insofar instead into 
            inward is isn't it it'd it'll it's its itself just keep keeps kept know known knows last lately later
            latter latterly least less lest let let's like liked likely little look looking looks ltd mainly many 
            may maybe me mean meanwhile merely might more moreover most mostly much must my myself name namely 
            nd near nearly necessary need needs neither never nevertheless new next nine no nobody non none 
            noone nor normally not nothing novel now nowhere obviously of off often oh ok okay old on once 
            one ones only onto or other others otherwise ought our ours ourselves out outside over overall own 
            particular particularly per perhaps placed please plus possible presumably probably provides que 
            quite qv rather rd re really reasonably regarding regardless regards relatively respectively right 
            said same saw say saying says second secondly see seeing seem seemed seeming seems seen self selves 
            sensible sent serious seriously seven several shall she should shouldn't since six so some somebody 
            somehow someone something sometime sometimes somewhat somewhere soon sorry specified specify 
            specifying still sub such sup sure t's take taken tell tends th than thank thanks thanx that that's 
            thats the their theirs them themselves then thence there there's thereafter thereby therefore therein 
            theres thereupon these they they'd they'll they're they've think third this thorough thoroughly those 
            though three through throughout thru thus to together too took toward towards tried tries truly try 
            trying twice two un under unfortunately unless unlikely until unto up upon us use used useful uses 
            using usually value various very via viz vs want wants was wasn't way we we'd we'll we're we've 
            welcome well went were weren't what what's whatever when whence whenever where where's whereafter 
            whereas whereby wherein whereupon wherever whether which while whither who who's whoever whole whom 
            whose why will willing wish with within without won't wonder would wouldn't yes yet you you'd 
            you'll you're you've your yours yourself yourselves zero".Split();
        }

    }
}
