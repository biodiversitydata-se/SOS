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
            if (fileType == null)
            {
                this.fileType = new FileType();
            }
            else
            {
                this.fileType = fileType;
            }
        }

        public string FileName => fileType.Files?.FirstOrDefault();

        public string RowType => fileType.RowType;

        public Encoding Encoding => Encoding.GetEncoding(fileType.Encoding);

        public string LinesTerminatedBy => Regex.Unescape(fileType.LinesTerminatedBy);

        public string FieldsTerminatedBy => Regex.Unescape(fileType.FieldsTerminatedBy);

        public string FieldsEnclosedBy => Regex.Unescape(fileType.FieldsEnclosedBy);

        public string DateFormat => fileType.DateFormat;

        public int LineTerminatorLength => Encoding.GetByteCount(LinesTerminatedBy);

        public int HeaderRowCount => fileType.IgnoreHeaderLines;
    }
}