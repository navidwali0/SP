using System.Collections.Generic;
using SPCommon.Entity;

namespace SPCommon.Interface
{
    public interface IRepository<T> where T : BaseListItem, new()
    {
        bool Create(T t);
        T Read(int id);
        bool Update(T t);
        bool Delete(T t);
        IList<T> FindAll();
    }
}
