using System.Collections.Generic;

namespace SPCommon.Entity
{
    public class BaseDocument : BaseListItem
    {
        public string Path { get; set; }
        public string FileName { get; set; }
        public string IconUrl { get; set; }
        public byte[] FileData { get; set; }
        public string FileUrl { get; set; }

        #region Serializer

        protected override Dictionary<string, string> ProvideJSONMapping()
        {
            var propMap = base.ProvideJSONMapping();
            propMap.Add("Path", Path);
            propMap.Add("FileName", FileName);
            propMap.Add("IconUrl", IconUrl);
            propMap.Add("FileUrl", FileUrl);
            return propMap;
        }

        #endregion
    }
}
