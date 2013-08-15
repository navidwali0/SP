using System.Linq;
using System.Collections.Generic;
using Microsoft.SharePoint;
using SPCommon.CustomException;
using SPCommon.Entity;
using SPCommon.Infrastructure.Common;
using SPCommon.Interface;

namespace SPCommon.Infrastructure.Repository
{
    /// <summary>
    /// ListRepository needs to be used with a class whose type has been derived from SPCommon.Entity.BaseListItem
    /// If used with SharePoint ServiceLocator, use the appropriate 'Initialise' method otherwise it will not work
    /// ListRepositoy is designed to work with dependency injection and completely stand-alone.  
    /// For Dependency Injection, initialise/construct with your SPWeb value (usually coming from SPContext.Current.Web)
    /// For stand-alone, pass the URL of the list
    /// ListName is compulsory (otherwise it won't work, obviously)
    /// 
    /// Note: ListRepository DOES NOT open the web using elevated priveleges; to do this, call ListRepository from your own elevated context
    /// Similarly for SPWebs with 'AllowUnsafeUpdates' allowed; to support this, create your own SPWeb object, set the AllowUnsafeUpdates to true, and then pass the web into the initialiser/constructor
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListRepository<T> : IListRepository<T> where T : BaseListItem, new()
    {
        #region Constructors, method-based initialisers and private/protected variables

        protected string ListUrl;
        protected string ListName;
        protected SPWeb Web;

        /// <summary>
        /// Empty constructor required for SharePoint ServiceLocator
        /// </summary>
        public ListRepository()
        {
        }

        public ListRepository(string listUrl, string listName)
        {
            ListUrl = listUrl;
            ListName = listName;
        }

        public ListRepository(SPWeb web, string listName)
        {
            Web = web;
            ListName = listName;
        }

        public void Initialise(string listUrl, string listName)
        {
            ListUrl = listUrl;
            ListName = listName;
        }

        public void Initialise(SPWeb web, string listName)
        {
            Web = web;
            ListName = listName;
        }

        #endregion

        #region Interface methods

        public bool Create(T t)
        {
            var success = false;
            if (Web == null)
            {
                Helper.Instance.OpenWeb(ListUrl, web => { success = CreateItem(web, t); });
            }
            else
            {
                success = CreateItem(Web, t);
            }
            return success;
        }

        public T Read(int id)
        {
            var item = new T();
            if (Web == null)
            {
                Helper.Instance.OpenWeb(ListUrl, web => { item = GetSingleItem(web, id); });
            }
            else
            {
                return GetSingleItem(Web, id);
            }
            return item;
        }

        public bool Update(T t)
        {
            var success = false;
            if (Web == null)
            {
                Helper.Instance.OpenWeb(ListUrl, web => { success = UpdateItem(web, t); });
            }
            else
            {
                success = UpdateItem(Web, t);
            }
            return success;
        }

        public bool Delete(T t)
        {
            var success = false;
            if (Web == null)
            {
                Helper.Instance.OpenWeb(ListUrl, web => { success = DeleteItem(web, t); });
            }
            else
            {
                success = DeleteItem(Web, t);
            }
            return success;
        }

        public IList<T> FindAll()
        {
            return FindByQuery(null);
        }

        public IList<T> FindByQuery(object query)
        {
            var returnedList = new List<T>();
            var spquery = query as SPQuery;
            if (Web == null)
            {
                Helper.Instance.OpenWeb(ListUrl, web => { returnedList = GetAllItems(web, spquery); });
            }
            else
            {
                returnedList = GetAllItems(Web, spquery);
            }
            return returnedList;
        }

        #endregion

        #region Overridable methods for extending the Repository

        /// <summary>
        /// Maps a class derived from SPCommon.Entity.BaseListItem to a corresponding SPListItem object
        /// Extending code needs to implement the rest of 'MapEntityItemToSPListItem' so item gets mapped properly
        /// TODO: can possiby use Reflection to fill in more values than just Title
        /// </summary>
        /// <param name="item"></param>
        /// <param name="spListItem"></param>
        protected virtual void MapEntityItemToSPListItem(T item, SPListItem spListItem)
        {
            spListItem["Title"] = item.Title;
        }

        /// <summary>
        /// TODO: turn this into reflection based property mapper
        /// Maps an SPListItem into an item derived from base cass SPCommon.Entity.BaseListItem
        /// Method can be over-ridden to provide custom functionality 
        /// </summary>
        /// <param name="spItem"></param>
        /// <returns></returns>
        protected virtual T MapSPListItemToEntityItem(SPListItem spItem)
        {
            var itemMapper = new SharePointItemMapper<T>();
            var t = itemMapper.BuildEntityFromItem(spItem);
            return t;
        }

        protected virtual bool CreateItem(SPWeb web, T item)
        {
            var list = GetList(web);
            var spListItem = list.Items.Add();
            MapEntityItemToSPListItem(item, spListItem);
            spListItem.Update();
            // Set these so item can be discovered by calling code
            item.Id = spListItem.ID;
            item.Guid = spListItem.UniqueId;
            return true;
        }

        protected virtual bool UpdateItem(SPWeb web, T item)
        {
            var list = GetList(web);
            var spListItem = list.GetItemById(item.Id);
            // Only for document libraries...
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
            }

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

        protected virtual bool DeleteItem(SPWeb web, T item)
        {
            var list = GetList(web);
            var listItem = list.GetItemById(item.Id);
            list.Items.DeleteItemById(listItem.ID);
            return true;
        }

        #endregion

        #region Private methods

        private List<T> GetAllItems(SPWeb web, SPQuery query)
        {
            var list = GetList(web);
            var items = query == null
                ? list.GetItems()
                : list.GetItems(query);

            return (from SPListItem item in items select MapSPListItemToEntityItem(item)).ToList();
        }

        private T GetSingleItem(SPWeb web, int id)
        {
            var list = GetList(web);
            var spListItem = list.GetItemById(id);
            return MapSPListItemToEntityItem(spListItem);
        }

        protected SPList GetList(SPWeb web)
        {
            var list = web.Lists.TryGetList(ListName);
            if (list == null) throw new ListNotFoundException(ListName + " does not exist in web " + web.Url);
            return list;
        }

        #endregion

    }
}
