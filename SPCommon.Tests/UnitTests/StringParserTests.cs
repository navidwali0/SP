using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.SharePoint.Applications.GroupBoard.WebPartPages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SPCommon.Tests.UnitTests
{
    [TestClass]
    public class StringParserTests
    {
        const string SingleValue = "Test Site V1.0";
        const string CtValue = "[Item:Test Site V1.0]";
        const string MultiValue = "[Item:Test Site V1.0];#[BID:Test Site V2.0]";
        const string ItemCT = "Item";
        private const string BidCT = "BID";

        [TestMethod]
        public void TemplateParser_ExtractSingleTemplateTitle()
        {
            var parser = new TemplateParser(SingleValue, ItemCT);
            Assert.IsTrue(parser.ToString().Equals(SingleValue));
        }

        [TestMethod]
        public void TemplateParser_ExtractSingleValueBasedOnContentType()
        {
            var parser = new TemplateParser(CtValue, ItemCT);
            Assert.IsTrue(parser.ToString().Equals("Test Site V1.0"));
        }

        [TestMethod]
        public void TemplateParser_ExtractValueForMultipleTemplates()
        {
            var parser = new TemplateParser(MultiValue, ItemCT);
            Assert.IsTrue(parser.ToString().Equals("Test Site V1.0"));

            parser = new TemplateParser(MultiValue, BidCT);
            Assert.IsTrue(parser.ToString().Equals("Test Site V2.0"));
        }

        [TestMethod]
        public void TemplateParser_ExtractValueForNonExistentContentType()
        {
            var parser = new TemplateParser(MultiValue, "document");
            Assert.IsTrue(parser.ToString().Equals(MultiValue));
        }

        [TestMethod]
        public void TemplateParser_ExtractValueForNullValue()
        {
            var parser = new TemplateParser(null, "document");
            Assert.IsTrue(string.IsNullOrEmpty(parser.ToString()));
        }
    }

    class TemplateParser
    {
        private readonly string _rawValue;
        private readonly string _contentType;
        private const string Delimiter = ";#";

        public TemplateParser(string rawVaue, string contentType)
        {
            _rawValue = rawVaue;
            _contentType = contentType;
        }

        public override string ToString()
        {
            // If content type isn't set, return the raw value. For backward compatibility
            if (string.IsNullOrEmpty(_contentType)) return _rawValue;
            // If raw value isn't set to anything, just return it; calling code will deal with exception
            return string.IsNullOrEmpty(_rawValue) ? _rawValue : GetTemplateTitle();
        }
        
        #region Private methods


        private string GetTemplateTitle()
        {
            // For single value template titles, it will just be the template title, i.e. it will not follow the format
            // This will also ensure backward compatibility
            if (!_rawValue.StartsWith("["))
                return _rawValue;
            foreach (var template in GetTemplates())
            {
                // Match [type:value]
                var regex = new Regex("\\[(?<type>.*):(?<value>.*)\\]");
                var matchCollection = regex.Matches(template);
                foreach (Match match in matchCollection)
                {
                    if (match.Groups["type"].ToString().Equals(_contentType, StringComparison.OrdinalIgnoreCase))
                        return match.Groups["value"].ToString();
                }
            }
            return _rawValue; // If we got to this point, nothing's been found. Return original value
        }

        /// <summary>
        /// Separate the strings into templates. Each chunk is a name value pair of ContentType and Title
        /// [ContentType:Template Title]
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GetTemplates()
        {
            return _rawValue.Split(new[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        
        #endregion
    }
}
