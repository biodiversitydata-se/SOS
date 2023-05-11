using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NReco.Csv;
using RecordParser.Builders.Reader;

namespace SOS.Lib.Helpers
{
    /// <summary>
    /// Helper class to read and write csv files
    /// </summary>
    public class CsvFileHelper : IDisposable
    {
        private StreamReader _streamReader;
        private CsvReader _csvReader;

        private StreamWriter _streamWriter;
        private CsvWriter _csvWriter;

        private bool _disposed;

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _streamWriter?.Close();
                _streamWriter?.Dispose();
                _streamReader?.Close();
                _streamReader?.Dispose();
            }

            _disposed = true;
        }

        /// <summary>
        /// Finish read operation
        /// </summary>
        public void FinishRead()
        {
            _streamReader?.Close();
            _streamReader?.Dispose();
        }

        /// <summary>
        /// Finish write operation
        /// </summary>
        public void FinishWrite()
        {
            _streamWriter?.Close();
            _streamWriter?.Dispose();
        }

        /// <summary>
        /// Flush writer
        /// </summary>
        public void Flush()
        {
            _streamWriter.Flush();
        }

        /// <summary>
        /// Flush writer
        /// </summary>
        /// <returns></returns>
        public async Task FlushAsync()
        {
            await _streamWriter.FlushAsync();
        }

        /// <summary>
        /// Get field by index
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fieldIndex"></param>
        /// <returns></returns>
        public string GetField(int fieldIndex)
        {
            return fieldIndex > _csvReader.FieldsCount ? null :_csvReader[fieldIndex];
        }

        /// <summary>
        /// Get typed records from csv file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mapping"></param>
        /// <returns></returns>
        public IEnumerable<T> GetRecords<T>(IVariableLengthReaderBuilder<T> mapping)
        {
            var builder = new StringBuilder();
            const string delimiter = "\t";
            var parser = mapping.Build(delimiter);

            var records = new List<T>();
            _csvReader.Read();

            while (_csvReader.Read())
            {
                builder.Clear();
                for (var i = 0; i < _csvReader.FieldsCount; i++)
                {
                    if (i > 0)
                    {
                        builder.Append(delimiter);
                    }

                    builder.Append(GetField(i));
                }
                var row = builder.Replace("\"", "'").ToString();
                records.Add(parser.Parse(row));
            }

            return records;
        }

        /// <summary>
        /// Initialize read operation
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="delimiter"></param>
        public void InitializeRead(Stream stream, string delimiter)
        {
           // _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _streamReader = new StreamReader(stream, Encoding.UTF8);
            _csvReader = new CsvReader(_streamReader, delimiter);
        }

        /// <summary>
        /// Initialize write operation
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="delimiter"></param>
        /// <param name="leaveStreamOpen"></param>
        public void InitializeWrite(Stream stream, string delimiter, bool leaveStreamOpen = false)
        {
            InitializeWrite(new StreamWriter(stream, Encoding.UTF8, -1, leaveStreamOpen), delimiter);
        }

        /// <summary>
        /// Initialize write operation
        /// </summary>
        /// <param name="streamWriter"></param>
        /// <param name="delimiter"></param>
        public void InitializeWrite(StreamWriter streamWriter, string delimiter)
        {
            _streamWriter = streamWriter;
            _csvWriter = new CsvWriter(_streamWriter, delimiter);
        }

        /// <summary>
        /// Create new row to write
        /// </summary>
        public void NextRecord()
        {
            _csvWriter.NextRecord();
        }

        /// <summary>
        /// Read next row
        /// </summary>
        /// <returns></returns>
        public bool Read()
        {
            return _csvReader.Read();
        }

        /// <summary>
        /// Write field
        /// </summary>
        /// <param name="field"></param>
        public void WriteField(string field)
        {
            _csvWriter.WriteField(field);
        }

        /// <summary>
        /// Write a row
        /// </summary>
        /// <param name="fields"></param>
        public void WriteRow(IEnumerable<string> fields)
        {
            foreach (var field in fields)
            {
                WriteField(field);
            }
            NextRecord();
        }
    }
}
