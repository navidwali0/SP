using SPCommon.Serializers;

namespace SPCommon.Interface
{
    public interface IJSONSerializable
    {
        JSON ToJSON();
    }
}
