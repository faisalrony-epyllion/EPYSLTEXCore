namespace EPYSLTEXCore.Infrastructure.Entities.General
{
    public class InMemoryFileData
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public MemoryStream Content { get; set; }
    }
}
