using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ABB.Swum;
using ABB.Swum.Nodes;
using Sando.Recommender;

namespace Sando.Recommender.UnitTests {
    [TestFixture]
    public class SwumDataRecordTests {
        [Test]
        public void TestRoundTrip() {
            var a1 = new WordNode("DB", PartOfSpeechTag.Preamble);
            var a2 = new WordNode("Get", PartOfSpeechTag.Verb);
            var t1 = new WordNode("Hydro", PartOfSpeechTag.NounModifier);
            var t2 = new WordNode("Fixed", PartOfSpeechTag.NounModifier);
            var t3 = new WordNode("Schedule", PartOfSpeechTag.Noun);

            var sdr = new SwumDataRecord();
            sdr.ParsedAction = new PhraseNode(new[] {a1, a2}, Location.None, false);
            sdr.Action = sdr.ParsedAction.ToPlainString();
            sdr.ParsedTheme = new PhraseNode(new[] {t1, t2, t3}, Location.None, false);
            sdr.Theme = sdr.ParsedTheme.ToPlainString();

            var actual = SwumDataRecord.Parse(sdr.ToString());
            Assert.IsTrue(SwumDataRecordsAreEqual(sdr, actual));
        }

        [Test]
        public void QueryRecommenderTest()
        {
            Assert.IsTrue((int)(10 / QueryRecommender.Distance("Add", "findAndAddRelevantFilesToIndexState")) < (int)(10 / QueryRecommender.Distance("Add", "addError")));
            Assert.IsTrue(QueryRecommender.Distance("AddFiles", "Add") < QueryRecommender.Distance("AddFiles", "AddAndDoOtherStuff"));
            Assert.IsTrue(QueryRecommender.Distance("AddFiles", "AddFile") < QueryRecommender.Distance("AddFiles", "AddFilesYeah"));
            Assert.IsTrue(QueryRecommender.Distance("Add", "AddFilesAndDoStuff") < QueryRecommender.Distance("Add", "FilesAndDoStuffAdd"));            
        }

        [Test]
        public void TestRoundTrip_FileNames() {
            var a1 = new WordNode("DB", PartOfSpeechTag.Preamble);
            var a2 = new WordNode("Get", PartOfSpeechTag.Verb);
            var t1 = new WordNode("Hydro", PartOfSpeechTag.NounModifier);
            var t2 = new WordNode("Fixed", PartOfSpeechTag.NounModifier);
            var t3 = new WordNode("Schedule", PartOfSpeechTag.Noun);
            var f1 = @"C:\foo\bar.cpp";
            var f2 = @"C:\foo\baz\xyzzy.h";
            var f3 = "test.cpp";

            var sdr = new SwumDataRecord();
            sdr.ParsedAction = new PhraseNode(new[] { a1, a2 }, Location.None, false);
            sdr.Action = sdr.ParsedAction.ToPlainString();
            sdr.ParsedTheme = new PhraseNode(new[] { t1, t2, t3 }, Location.None, false);
            sdr.Theme = sdr.ParsedTheme.ToPlainString();
            sdr.FileNames.Add(f1);
            sdr.FileNames.Add(f2);
            sdr.FileNames.Add(f3);

            var actual = SwumDataRecord.Parse(sdr.ToString());
            Assert.IsTrue(SwumDataRecordsAreEqual(sdr, actual));
        }


        public bool WordNodesAreEqual(WordNode wn1, WordNode wn2) {
            if(wn1 == wn2) {
                return true;
            }
            if(wn1 == null ^ wn2 == null) {
                return false;
            }

            return wn1.Text == wn2.Text
                   && wn1.Tag == wn2.Tag
                   && wn1.Location == wn2.Location
                   && Math.Abs(wn1.Confidence - wn2.Confidence) < .00001;

        }

        public bool PhraseNodesAreEqual(PhraseNode pn1, PhraseNode pn2) {
            if(pn1 == pn2) {
                return true;
            }
            if(pn1 == null ^ pn2 == null) {
                return false;
            }
            
            if(pn1.Size() != pn2.Size()) {
                return false;
            }
            for(int i = 0; i < pn1.Size(); i++) {
                if(!WordNodesAreEqual(pn1[i], pn2[i])) {
                    return false;
                }
            }
            return true;
        }

        public bool SwumDataRecordsAreEqual(SwumDataRecord sdr1, SwumDataRecord sdr2) {
            if(sdr1 == sdr2) {
                return true;
            }
            if(sdr1 == null ^ sdr2 == null) {
                return false;
            }

            bool areEqual = PhraseNodesAreEqual(sdr1.ParsedAction, sdr2.ParsedAction)
                            && PhraseNodesAreEqual(sdr1.ParsedTheme, sdr2.ParsedTheme)
                            && PhraseNodesAreEqual(sdr1.ParsedIndirectObject, sdr2.ParsedIndirectObject)
                            && sdr1.Action == sdr2.Action
                            && sdr1.Theme == sdr2.Theme
                            && sdr1.IndirectObject == sdr2.IndirectObject
                            && sdr1.FileNames.Count == sdr2.FileNames.Count;
            if(areEqual) {
                //both FileNames are the same size
                foreach(var file in sdr1.FileNames) {
                    areEqual = areEqual && sdr2.FileNames.Contains(file);
                }
            }
            return areEqual;
        }
    }
}
