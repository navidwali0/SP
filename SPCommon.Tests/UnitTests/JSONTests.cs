using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SPCommon.Entity;
using SPCommon.Serializers;

namespace SPCommon.Tests.UnitTests
{
    [TestClass]
    public class JSONTests
    {
        [TestMethod]
        public void JSON_SerializeObjectToJSON()
        {
            var baseItem = new BaseListItem
            {
                Author = "Navid"
            };
            var json = baseItem.ToJSON();
            Assert.IsTrue(json.ToString().Contains("Navid"));
        }

        [TestMethod]
        public void JSON_BuildJSONTree()
        {
            var jsonList = new JSONList
            {
                new BaseListItem {Author = "Navid"},
                new BaseListItem {Author = "Preety"},
                new BaseListItem {Author = "One"},
                new BaseDocument {Author = "Two"}
            };

            var list = new List<BaseListItem>
            {
                new BaseListItem {Author = "Navid"},
                new BaseListItem {Author = "Preety"},
            };

            var jsonList2 = JSONList.ToJSONList(list);
            var jsonString2 = jsonList2.ToJSONString();
            Assert.IsTrue(jsonString2.Contains("Navid"));
            var jsonString = jsonList.ToJSONString();
            Assert.IsTrue(jsonString.Contains("Navid"));
        }
    }
}
