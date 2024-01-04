namespace DwC_A.Meta
{
    internal class CoreFileMetaData : AbstractFileMetaData, IFileMetaData
    {
        private readonly CoreFileType coreFileType;

        public CoreFileMetaData(CoreFileType coreFileType) :
            base(coreFileType)
        {
            this.coreFileType = coreFileType ?? new CoreFileType();
            Fields = new FieldMetaData(this.coreFileType?.Id, this.coreFileType?.Field);
        }

        public IdFieldType Id => coreFileType.Id;

        public IFieldMetaData Fields { get; }
    }
}