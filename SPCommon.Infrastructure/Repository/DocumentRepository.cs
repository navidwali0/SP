using Microsoft.SharePoint;
using SPCommon.Entity;
using SPCommon.Infrastructure.Common;
using SPCommon.Interface;

namespace SPCommon.Infrastructure.Repository
{
    /// <summary>
    /// The documentation for ListRepository covers most of this. The create / update / delete methods have been re-written for Document Libraries to support
    /// check-in/check-out, file-reads, file-uploads, etc.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DocumentRepository<T> : ListRepository<T>, IDocumentRepository<T> where T : BaseDocument, new()
    {
        #region Constructors and private/protected variables

        /// <summary>
        /// Empty constructor required for SharePoint ServiceLocator
        /// </summary>
        public DocumentRepository()
        {}

        public DocumentRepository(string libraryUrl, string libraryName)
            : base(libraryUrl, libraryName)
        {}

        public DocumentRepository(SPWeb web, string libraryName)
            : base(web, libraryName)
        {}

        #endregion

        #region Interface methods

        public void DownloadFileData(T t)
        {
            if (Web == null)
            {
                Helper.Instance.OpenWeb(ListUrl, web =>
                {
                    if (string.IsNullOrEmpty(t.FileUrl))
                        t = Read(t.Id);
                    SetFileDataForItem(web, t);
                });                
            }
            else
            {
                if (string.IsNullOrEmpty(t.FileUrl))
                    t = Read(t.Id);
                SetFileDataForItem(Web, t);                
            }
        }

        #endregion

        #region Overridden methods

        /// <summary>
        /// Extend ListRepository's MapSPListItemToEntityItem to include file data.
        /// NOTE: the binary data for the SPFile is not included. To get the binary data, call DownloadFileData(T entity)
        /// </summary>
        /// <param name="spItem"></param>
        /// <returns></returns>
        protected override T MapSPListItemToEntityItem(SPListItem spItem)
        {
            var t = base.MapSPListItemToEntityItem(spItem);
            var file = spItem.File;
            t.FileName = file.Name;
            t.IconUrl = file.IconUrl;
            t.FileUrl = file.Url;
            return t;
        }

        protected override bool CreateItem(SPWeb web, T item)
        {
            // Can't create a document library item without a file, so'z
            if (item.FileData == null) return false;
            // Upload the file first
            var folder = web.Folders[ListName];
            var fileUrl = web.Url + "/" + folder.Url + "/" + item.FileName;
            if (web.GetFile(fileUrl).Exists) return false; // file exists, return with false
            var file = folder.Files.Add(fileUrl, item.FileData);
            // Get the list item from the uploaded file
            var spListItem = file.Item;
            // Do any extra mapping required
            MapEntityItemToSPListItem(item, spListItem);
            // Update the item
            spListItem.Update();
            item.FileName = file.Name;
            item.IconUrl = file.IconUrl;
            item.FileUrl = file.Url;
            item.Id = spListItem.ID;
            item.Guid = spListItem.UniqueId;

            // If the file doesn't require check out, can exit here
            if (!file.RequiresCheckout) return true;

            // ... otherwise check it in
            var userName = web.CurrentUser == null ? "System User" : web.CurrentUser.LoginName;
            spListItem.File.CheckIn("Checked in by system after update on behalf of " + userName,
                SPCheckinType.MajorCheckIn);
            return true;
        }

        protected override bool UpdateItem(SPWeb web, T item)
        {
            var list = GetList(web);
            var spListItem = list.GetItemById(item.Id);
            
            if (spListItem.File != null)
            {
                try
                {
                    // File is checked-out already, skip update
                    if (spListItem.File.CheckOutType != SPFile.SPCheckOutType.None)
                        return false;

                    // Attempt to check out the file if necessary
                    if (spListItem.File.RequiresCheckout)
                        spListItem.File.CheckOut();
                }
                catch { return false; } // Can't check out, skip update
                // TODO: trace the above exception to the logs

                // Save the file data if required
                // TODO: Check that the file has changed before updating
                if (item.FileData != null)
                {
                    spListItem.File.SaveBinary(item.FileData);
                    // Need to get the item again as it has been updated in the server due to .SaveBinary
                    spListItem = list.GetItemById(item.Id);
                }
            }

            // Do any extra mapping necessary
            MapEntityItemToSPListItem(item, spListItem);
            spListItem.Update();

            // File doesn't require check out, don't have to do anything
            if (spListItem.File == null || !spListItem.File.RequiresCheckout) return true;

            // File is checked out, so check back in
            var userName = web.CurrentUser == null ? "System User" : web.CurrentUser.LoginName;
            spListItem.File.CheckIn("Checked in by system after update on behalf of " + userName,
                SPCheckinType.MajorCheckIn);
            return true;            
        }

        protected override bool DeleteItem(SPWeb web, T item)
        {
            var file = web.GetFile(item.FileUrl);
            file.Delete();
            return true;
        }

        #endregion

        #region Private methods

        private void SetFileDataForItem(SPWeb web, T item)
        {
            item.FileData = web.GetFile(item.FileUrl).OpenBinary();
        }

        #endregion
    }
}
