using System.Collections.Generic;
using System.IO;
using Transformalize.Context;
using Transformalize.Contracts;

namespace Transformalize.Provider.GeoJson {
    public class GeoJsonFileWriter : IWrite {
        private readonly OutputContext _context;
        private readonly IWrite _streamWriter;
        private readonly MemoryStream _stream;

        public GeoJsonFileWriter(OutputContext context) {
            _context = context;
            _stream = new MemoryStream();
            _streamWriter = new GeoJsonStreamWriter(context, _stream);
        }

        public void Write(IEnumerable<IRow> rows) {
            using (var fileStream = File.Create(_context.Connection.File)) {
                _streamWriter.Write(rows);
                _stream.Seek(0, SeekOrigin.Begin);
                _stream.CopyTo(fileStream);
            }
        }
    }
}