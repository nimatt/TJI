using System;
using System.Globalization;
using NUnit.Framework;
using TJI.Toggl;
using TJI;

namespace TJI_Test
{
    [TestFixture]
    public class WorkEntryFixture
    {
        [Test]
        public void Create_SingleLetterProj()
        {
            TestValid("P-1", string.Empty);
        }

        [Test]
        public void Create_MinimumInfo()
        {
            TestValid("P-1", string.Empty, string.Empty);
        }

        [Test]
        public void Create_NoLetterProj()
        {
            Assert.IsNull(WorkEntry.Create(GetEntry("-1")));
        }

        [Test]
        public void Create_MultiLetterProj()
        {
            TestValid("FOOBAR-1", string.Empty);
        }

        [Test]
        public void Create_LowerCaseLetterProj()
        {
            Assert.IsNull(WorkEntry.Create(GetEntry("p-1")));
        }

        [Test]
        public void Create_NoNumber()
        {
            Assert.IsNull(WorkEntry.Create(GetEntry("P-")));
        }

        [Test]
        public void Create_NoNumberWithDesc()
        {
            Assert.IsNull(WorkEntry.Create(GetEntry("P- Hello")));
        }

        [Test]
        public void Create_MultiDigitNumber()
        {
            TestValid("P-156846516354168", string.Empty);
        }

        [Test]
        public void Create_Colon()
        {
            TestValid("P-1", string.Empty);
        }

        [Test]
        public void Create_ColonWithDesc()
        {
            TestValid("P-1","Foo");
        }

        [Test]
        public void Create_MultiWordDesc()
        {
            TestValid("PA-144", "Foo Bar");
        }

        [Test]
        public void Create_NoDash()
        {
            Assert.IsNull(WorkEntry.Create(GetEntry("P12: Foo")));
        }

        [Test]
        public void Create_NoDesc()
        {
            Assert.IsNull(WorkEntry.Create(GetEntry(null)));
        }

        [Test]
        public void Create_NoColon()
        {
            TestValid("PD-2443","Foo", string.Empty);
        }

        private void TestValid(string issueId, string comment, string seperator = ": ")
        {
            var entry = WorkEntry.Create(GetEntry($"{issueId}{seperator}{comment}"));

            Assert.IsNotNull(entry);
            Assert.AreEqual(issueId, entry.IssueId);
            Assert.AreEqual(comment, entry.Comment);
        }

        private TogglEntry GetEntry(string desc)
        {
            return new TogglEntry()
            {
                Description = desc,
                Duration = 60,
                Start = new DateTime(2015, 12, 9).ToString(CultureInfo.CurrentCulture),
                At = new DateTime(2015, 12, 9).ToString(CultureInfo.CurrentCulture)
            };
        }
    }
}