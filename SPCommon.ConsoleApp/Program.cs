using System;
using Microsoft.SharePoint;
using SPCommon.Entity;
using SPCommon.Infrastructure.Repository;

namespace SPCommon.ConsoleApp
{
    class Program
    {
        private const string ListName = "Test";
        private const string ListUrl = "http://spdev/lists/Test";

        static void Main(string[] args)
        {
            var listRepository = new ListRepository<TestEntity>(ListUrl, ListName);
            var items = listRepository.FindAll();
            Console.WriteLine(items.Count);
        }
    }

    public class TestEntity : BaseListItem
    {
        public string TextColumn { get; set; }
        public bool YesNoColumn { get; set; }
    }
}
