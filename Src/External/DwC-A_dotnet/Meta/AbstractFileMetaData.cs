using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DwC_A.Meta
{
    internal abstract class AbstractFileMetaData
    {
        private readonly FileType fileType;

        public AbstractFileMetaData(FileType fileType)
        {
            if(fileType == null)
            {
                this.fileType = new FileType();
            }
            else
            {
                this.fileType = fileType;
            }
        }

        public string FileName { get { return fileType.Files?.FirstOrDefault(); } }

        public string RowType { get { return fileType.RowType; } }

        public Encoding Encoding { get { return Encoding.GetEncoding(fileType.Encoding); } }

        public string LinesTerminatedBy { get { return Regex.Unescape(fileType.LinesTerminatedBy); } }

        public string FieldsTerminatedBy { get { return Regex.Unescape(fileType.FieldsTerminatedBy); } }

        public string FieldsEnclosedBy { get { return Regex.Unescape(fileType.FieldsEnclosedBy); } }

        public string DateFormat { get { return fileType.DateFormat; } }

        public int LineTerminatorLength { get { return Encoding.GetByteCount(LinesTerminatedBy); } }

        public int HeaderRowCount { get { return fileType.IgnoreHeaderLines; } }
    }
}
