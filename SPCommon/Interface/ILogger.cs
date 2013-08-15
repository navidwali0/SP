using System;

namespace SPCommon.Interface
{
    public interface ILogger
    {
        void Log(string message);
        void Log(Exception e);
    }
}
