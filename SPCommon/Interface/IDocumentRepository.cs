using SPCommon.Entity;

namespace SPCommon.Interface
{
    public interface IDocumentRepository<T> : IListRepository<T> where T : BaseDocument, new()
    {
        void DownloadFileData(T t);
    }
}
