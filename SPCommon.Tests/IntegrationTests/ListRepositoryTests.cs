using System.Collections.Generic;
using Microsoft.SharePoint.JSGrid;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SPCommon.Entity;
using SPCommon.Infrastructure.Repository;
using SPCommon.Interface;

namespace SPCommon.Tests.IntegrationTests
{
    [TestClass]
    public class ListRepositoryTests
    {
        private const string ListName = "Test";
        private const string ListUrl = "http://spdev/lists/Test";
        private readonly IListRepository<TestEntity> _listRepository;

        public ListRepositoryTests()
        {
            _listRepository = new ListRepository<TestEntity>(ListUrl, ListName);
        }

        [TestMethod]
        public void ListRepository_GetAllItems()
        {
            var items = _listRepository.FindAll();
            var initialCount = items.Count;
            var testEntity = GetTestEntity();
            _listRepository.Create(testEntity);
            Assert.IsTrue(_listRepository.FindAll().Count == (initialCount + 1));
            Assert.IsTrue(_listRepository.Delete(testEntity));
            Assert.IsTrue(_listRepository.FindAll().Count == initialCount);
        }

        private static TestEntity GetTestEntity()
        {
            return new TestEntity {Title = "GetItemTest"};
        }

        [TestMethod]
        public void ListRepository_AddItem()
        {
            var items = _listRepository.FindAll();
            var initialCount = items.Count;
            var newItem = GetTestEntity();
            Assert.IsTrue(_listRepository.Create(newItem));
            Assert.IsTrue(_listRepository.FindAll().Count == (initialCount + 1));
            var createdItem = _listRepository.Read(newItem.Id);
            Assert.IsTrue(createdItem.Id == newItem.Id);
            Assert.IsTrue(_listRepository.Delete(createdItem));
        }

        [TestMethod]
        public void ListRepository_UpdateItem()
        {
            var items = _listRepository.FindAll();
            var initialCount = items.Count;
            var newItem = GetTestEntity();
            Assert.IsTrue(_listRepository.Create(newItem));
            Assert.IsTrue(_listRepository.FindAll().Count == (initialCount + 1));
            newItem.Title = "Test Item Updated";
            Assert.IsTrue(_listRepository.Update(newItem));
            var updatedItem = _listRepository.Read(newItem.Id);
            Assert.IsTrue(updatedItem.Id == newItem.Id);
            Assert.IsTrue(updatedItem.Title == newItem.Title);
            Assert.IsTrue(_listRepository.Delete(updatedItem));
        }

        [TestMethod]
        public void ListRepository_DeleteItem()
        {
            var items = _listRepository.FindAll();
            var initialCount = items.Count;

            var newItem = new TestEntity
            {
                Title = "Test For Delete"
            };
            var success = _listRepository.Create(newItem);
            Assert.IsTrue(success);
            Assert.IsTrue(_listRepository.FindAll().Count == (initialCount + 1));

            success = _listRepository.Delete(newItem);
            Assert.IsTrue(success);
            Assert.IsTrue(_listRepository.FindAll().Count == initialCount);
        }
    }

    public class TestEntity : BaseListItem
    {
        public string TextColumn { get; set; }
        public bool YesNoColumn { get; set; }
    }
}
