using System.Collections.Generic;
using SPCommon.Entity;

namespace SPCommon.Interface
{
    public interface IListRepository<T> : IRepository<T> where T : BaseListItem, new()
    {
        IList<T> FindByQuery(object query);
    }
}
