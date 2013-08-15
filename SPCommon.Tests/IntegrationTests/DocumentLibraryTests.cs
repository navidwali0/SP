using System;
using System.Diagnostics;
using System.IO;
using Microsoft.SharePoint;
using Microsoft.SharePoint.JSGrid;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SPCommon.Entity;
using SPCommon.Infrastructure.Repository;
using SPCommon.Interface;

namespace SPCommon.Tests.IntegrationTests
{
    [TestClass]
    public class DocumentLibraryTests
    {
        private const string LibName = "TestLibrary";
        private const string LibUrl = "http://spdev/TestLibrary";
        private const string FileToUpload = @"C:\temp\testfile.txt";
        private const string FileToUpdate = @"C:\temp\updatefile.txt";
        private readonly IDocumentRepository<TestDocument> _documentRepository;  

        public DocumentLibraryTests()
        {
            _documentRepository = new DocumentRepository<TestDocument>(LibUrl, LibName);
        }

        [TestMethod]
        public void DocumentRepository_UpdateFileData()
        {
            var doc = GetTestDocument();
            Assert.IsTrue(_documentRepository.Create(doc));
            var uploadedDoc = _documentRepository.Read(doc.Id);
            _documentRepository.DownloadFileData(uploadedDoc);

            var fileData = SaveFileReadDataAndDeleteFile(uploadedDoc);
            Assert.IsTrue(fileData.ToLower().Contains("some data"));

            uploadedDoc.FileData = File.ReadAllBytes(FileToUpdate);
            Assert.IsTrue(_documentRepository.Update(uploadedDoc));

            var updatedDoc = _documentRepository.Read(uploadedDoc.Id);
            _documentRepository.DownloadFileData(updatedDoc);

            var comparisonFileData = SaveFileReadDataAndDeleteFile(updatedDoc);
            Assert.IsTrue(comparisonFileData.ToLower().Contains("updated content"));

            Assert.IsTrue(_documentRepository.Delete(updatedDoc));
        }

        [TestMethod]
        public void DocumentRepository_GetAllDocuments()
        {
            var doc = GetTestDocument();
            var countBefore = _documentRepository.FindAll().Count;
            Assert.IsTrue(_documentRepository.Create(doc));
            Assert.IsTrue(_documentRepository.FindAll().Count == (countBefore + 1));
            Assert.IsTrue(_documentRepository.Delete(doc));
            Assert.IsTrue(_documentRepository.FindAll().Count == countBefore);
        }

        [TestMethod]
        public void DocumentRepository_UpdateDocumentMetadata()
        {
            var doc = GetTestDocument();
            Assert.IsTrue(_documentRepository.Create(doc));

            var createdDoc = _documentRepository.Read(doc.Id);
            createdDoc.Title = "Changed";

            Assert.IsTrue(_documentRepository.Update(createdDoc));

            var updatedDoc = _documentRepository.Read(doc.Id);
            Assert.IsTrue(updatedDoc.Title == createdDoc.Title);

            // Delete item
            Assert.IsTrue(_documentRepository.Delete(doc));
        }

        [TestMethod]
        public void DocumentRepository_AddDocument()
        {
            var docCount = _documentRepository.FindAll().Count;
            var doc = GetTestDocument();

            Assert.IsTrue(_documentRepository.Create(doc));
            Assert.IsTrue(_documentRepository.FindAll().Count == (docCount + 1));

            Assert.IsTrue(_documentRepository.Delete(doc));
            Assert.IsTrue(_documentRepository.FindAll().Count == docCount);
        }

        [TestMethod]
        public void DocumentRepository_DeleteDocument()
        {
            var docCount = _documentRepository.FindAll().Count;
            var doc = GetTestDocument();

            Assert.IsTrue(_documentRepository.Create(doc));
            Assert.IsTrue(_documentRepository.FindAll().Count == (docCount + 1));

            Assert.IsTrue(_documentRepository.Delete(doc));
            Assert.IsTrue(_documentRepository.FindAll().Count == docCount);
        }

        [TestMethod]
        public void DocumentRepository_DeleteDocumentRegardlessOfCheckout()
        {
            var document = GetTestDocument();
            Assert.IsTrue(_documentRepository.Create(document));

            // Check out the document using good ol SharePoint code
            using (var site = new SPSite(LibUrl))
            {
                using (var web = site.OpenWeb())
                {
                    var list = web.Lists[LibName];
                    var listItem = list.GetItemById(document.Id);
                    listItem.File.CheckOut();
                }
            }

            var docs = _documentRepository.FindAll();
            Assert.IsTrue(docs.Count > 0);
            foreach (var doc in docs)
            {
                _documentRepository.Delete(doc);
            }
            Assert.IsTrue(_documentRepository.FindAll().Count == 0);
        }

        #region Helpers

        private TestDocument GetTestDocument()
        {
            return new TestDocument
            {
                Title = "TestDocument",
                FileName = "TestDocument.txt",
                FileData = File.ReadAllBytes(FileToUpload)
            };
        }

        private string SaveFileReadDataAndDeleteFile(TestDocument uploadedDoc)
        {
            // Create file
            var filePath = @"C:\temp\created-" + uploadedDoc.FileName;
            var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            fileStream.Write(uploadedDoc.FileData, 0, uploadedDoc.FileData.Length);
            fileStream.Close();

            // Read data
            var returnString = File.ReadAllLines(filePath)[0];

            // Delete file
            File.Delete(filePath);

            return returnString;
        }

        #endregion
    }

    internal class TestDocument : BaseDocument
    {
        public string Name { get; set; }
    }
}
