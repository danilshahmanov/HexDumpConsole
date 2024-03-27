using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexDumpConsole
{
    public class BatchFileReader : IDisposable
    {
        private FileStream? fileStream;
        private Encoding? _encoding;
        private byte[] _buffer = new byte[BufferSize];
        public void Dispose() => fileStream?.Dispose();
        /// <summary>
        /// Размер буфера для чтения данных из файла.
        /// </summary>
        public static int BufferSize { get; } = 1024;

        /// <summary>
        /// Общее количество байтов в файле.
        /// </summary>
        public long? FileLength { get; private set; }

        /// <summary>
        /// Перечисление, представляющее поддерживаемые типы кодировок.
        /// </summary>
        public enum EncodingType
        {
            UTF8,
            UTF32,
            UTF16BE,
            UTF16LE,
            ASCII
        }

        /// <summary>
        /// Устанавливает тип кодировки для чтения данных из файла.
        /// </summary>
        /// <param name="encodingType">Тип кодировки.</param>
        public void SetEncoding(EncodingType encodingType) => _encoding = ToEncoding(encodingType);
        private static Encoding ToEncoding(EncodingType encodingType) =>
            encodingType switch
            {
                EncodingType.UTF8 => Encoding.UTF8,
                EncodingType.UTF32 => Encoding.UTF32,
                EncodingType.UTF16BE => Encoding.BigEndianUnicode,
                EncodingType.UTF16LE => Encoding.Unicode,
                EncodingType.ASCII => Encoding.ASCII,
                _ => throw new NotImplementedException("Неизвестная кодировка.")
            };
        /// <summary>
        /// Конструктор. Устанавливает файл для чтения.
        /// </summary>
        /// <param name="filePath">Путь к файлу.</param>
        public BatchFileReader(string filePath, int maxBatchSize)
        {
            fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            FileLength = fileStream.Length;
        }
       
        /// <summary>
        /// Получает строку данных из файла по заданному индексу.
        /// </summary>
        /// <param name="index">Индекс строки.</param>
        /// <returns>BufferedRow, представляющий буферизованную строку данных.</returns>
        public BufferedRow? GetBatchByIndex(int index)
        {
            if (_encoding is null)
                throw new NullReferenceException("Не установлен тип кодировки.");
            if (ReadByBuffer(index * BufferSize))
            {
                return new BufferedRow()
                {
                    EncodedBytes = [.. _buffer],
                    DecodedText = _encoding.GetString(_buffer),
                    Offset = index * BufferSize,
                };
            }
            return null;
        }
        public BufferedRow? GetNextBatch()
        {
            if (_encoding is null)
                throw new NullReferenceException("Не установлен тип кодировки.");
            if (ReadByBuffer(index * BufferSize))
            {
                return new BufferedRow()
                {
                    EncodedBytes = [.. _buffer],
                    DecodedText = _encoding.GetString(_buffer),
                    Offset = index * BufferSize,
                };
            }
            return null;
        }
        private bool ReadByBuffer(int offset)
        {
            if (offset < 0 || offset > FileLength)
            {
                return false;
            }
            //преобразовываем смещение относительно SeekOrigin.Current
            offset -= (int)fileStream.Position;
            try
            {
                fileStream.Seek(offset, SeekOrigin.Current);
                fileStream.Read(_buffer, 0, BufferSize);
                return true;
            }
            catch (Exception ex)
            {
                _fileStream.Dispose();
                _fileStream = null;
                throw new IOException("Ошибка при чтении файла.", ex);
            }
        }
    }
}
