using System;
using System.Collections.Generic;
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
            said there use an each which she do how their if will
            up other about out many then them these so some her
            would make like him into time has look two more write go see number no way
            could people my than first water been call who its now did get come made may
            ".Split();
        }
    }
}
