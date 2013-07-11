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

        public static string[] StopWords()
        {
            return @"a able about above abst accordance according accordingly across act actually added adj affected affecting 
            affects after afterwards again against ah all almost alone along already also although always am among amongst an and announce
            another any anybody anyhow anymore anyone anything anyway anyways anywhere apparently approximately are aren arent arise around 
            as aside ask asking at auth available away awfully b back be became because become becomes becoming been before beforehand begin
            beginning beginnings begins behind being believe below beside besides between beyond biol both brief briefly but by c ca came 
            can cannot can't cause causes certain certainly co com come comes contain containing contains could couldnt d date did didn't 
            different do does doesn't doing done don't down downwards due during e each ed edu effect eg eight eighty either else elsewhere
            end ending enough especially et et-al etc even ever every everybody everyone everything everywhere ex except f far few ff fifth
            first five fix followed following follows for former formerly forth found four from further furthermore g gave get gets getting
            give given gives giving go goes gone got gotten h had happens hardly has hasn't have haven't having he hed hence her here hereafter
            hereby herein heres hereupon hers herself hes hi hid him himself his hither home how howbeit however hundred i id ie if i'll im
            immediate immediately importance important in inc indeed index information instead into invention inward is isn't it itd it'll 
            its itself i've j just k keep keeps kept kg km know known knows l largely last lately later latter latterly least less lest let
            lets like liked likely line little 'll look looking looks ltd m made mainly make makes many may maybe me mean means meantime
            meanwhile merely mg might million miss ml more moreover most mostly mr mrs much mug must my myself n na name namely nay nd near
            nearly necessarily necessary need needs neither never nevertheless new next nine ninety no nobody non none nonetheless noone
            nor normally nos not noted nothing now nowhere o obtain obtained obviously of off often oh ok okay old omitted on once one 
            ones only onto or ord other others otherwise ought our ours ourselves out outside over overall owing own p page pages part 
            particular particularly past per perhaps placed please plus poorly possible possibly potentially pp predominantly present previously
            primarily probably promptly proud provides put q que quickly quite qv r ran rather rd re readily really recent recently ref refs
            regarding regardless regards related relatively research respectively resulted resulting results right run s said same saw say
            saying says sec section see seeing seem seemed seeming seems seen self selves sent seven several shall she shed she'll shes should
            shouldn't show showed shown showns shows significant significantly similar similarly since six slightly so some somebody somehow
            someone somethan something sometime sometimes somewhat somewhere soon sorry specifically specified specify specifying still stop
            strongly sub substantially successfully such sufficiently suggest sup sure t take taken taking tell tends th than thank thanks
            thanx that that'll thats that've the their theirs them themselves then thence there thereafter thereby thered therefore therein
            there'll thereof therere theres thereto thereupon there've these they theyd they'll theyre they've think this those thou though
            thoughh thousand throug through throughout thru thus til tip to together too took toward towards tried tries truly try trying
            ts twice two u un under unfortunately unless unlike unlikely until unto up upon ups us use used useful usefully usefulness uses
            using usually v value various 've very via viz vol vols vs w want wants was wasn't way we wed welcome we'll went were weren't
            we've what whatever what'll whats when whence whenever where whereafter whereas whereby wherein wheres whereupon wherever
            whether which while whim whither who whod whoever whole who'll whom whomever whos whose why widely willing wish with within
            without won't words world would wouldn't www x y yes yet you youd you'll your youre yours yourself yourselves you've z zero 
            ".Split();
        }

    }
}
