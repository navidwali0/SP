using System;

namespace SPCommon.CustomException
{
    [Serializable]
    public class ListNotFoundException : BaseException
    {
        public ListNotFoundException()
        { }

        public ListNotFoundException(string listName)
            : base(listName)
        { }
    }
}
