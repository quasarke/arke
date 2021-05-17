namespace Arke.SipEngine.Web
{
    public interface IRestCommand
    {
        string UniqueId { get; set; }
        string Url { get; set; }
        string Method { get; set; }
        string Body { get; }

        void AddUrlSegment(string segName, string value);
        void AddParameter(string name, object value, RequestParameterType type);
    }
}
