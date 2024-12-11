namespace EPYSLTEXCore.Infrastructure.Entities.General
{
    public class InMemoryMultipartFormDataStreamProvider
    {
        public List<InMemoryFileData> Files { get; } = new();
        public Dictionary<string, string> FormData { get; } = new();
    }

}
