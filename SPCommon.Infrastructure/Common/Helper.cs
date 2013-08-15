using System;
using Microsoft.SharePoint;

namespace SPCommon.Infrastructure.Common
{
    public class Helper
    {
        #region Singleton initialiser

        private Helper() {}
        private static Helper _helper;
        public static Helper Instance
        {
            get
            {
                if (_helper != null) return _helper;
                _helper = new Helper();
                return _helper;
            }
        }

        #endregion

        /// <summary>
        /// Creates an SPWeb object and passes it onto the Action method for calling code to execute
        /// </summary>
        /// <param name="webUrl">URL of SPWeb</param>
        /// <param name="action">Callback method, takes SPWeb as parameter</param>
        public void OpenWeb(string webUrl, Action<SPWeb> action)
        {
            using (var site = new SPSite(webUrl))
            {
                using (var web = site.OpenWeb())
                {
                    action(web);
                }
            }
        }

        /// <summary>
        /// Creates an SPSite object and passes it onto the Action method for calling code to execute.
        /// Useful for when you want to work on the RootWeb but have a List URL
        /// Example: OpenSite("http://spdev/lists/CustomList", site => { var rootWeb = site.RootWeb; // do other things })
        /// </summary>
        /// <param name="siteUrl">URL of SPSite to open</param>
        /// <param name="action">Callback method, takes SPSite as parameter</param>
        public void OpenSite(string siteUrl, Action<SPSite> action)
        {
            using (var site = new SPSite(siteUrl))
            {
                action(site);
            }
        }

        /// <summary>
        /// Creates an SPWeb
        /// </summary>
        /// <param name="webUrl">URL of SPWeb in elevated context</param>
        /// <param name="action">Callback method, takes SPWeb as parameter</param>
        public void OpenElevatedWeb(string webUrl, Action<SPWeb> action)
        {
            SPSecurity.RunWithElevatedPrivileges(() => OpenWeb(webUrl, action));
        }
        
        /// <summary>
        /// Opens an SPWeb and sets its AllowUnsafeUpdates to true. Once action is complete, sets it back to what it was previously
        /// </summary>
        /// <param name="webUrl">Url of SPWeb to open and execute unsafe action on</param>
        /// <param name="action">Callback method, takes SPWeb as parameter</param>
        public void OpenUnsafeWeb(string webUrl, Action<SPWeb> action)
        {
            OpenWeb(webUrl, web =>
            {
                var unsafeState = web.AllowUnsafeUpdates;
                web.AllowUnsafeUpdates = true;
                try
                {
                    action(web);
                }
                finally
                {
                    web.AllowUnsafeUpdates = unsafeState;
                }
            });
        }

        /// <summary>
        /// Opens an SPWeb in an elevated context and sets its AllowUnsafeUpdates to true. Once action is complete, sets it back to what it was previously
        /// </summary>
        /// <param name="webUrl"></param>
        /// <param name="action"></param>
        public void OpenUnsafeElevatedWeb(string webUrl, Action<SPWeb> action)
        {
            SPSecurity.RunWithElevatedPrivileges(() => OpenUnsafeWeb(webUrl, action));
        }
    }
}
