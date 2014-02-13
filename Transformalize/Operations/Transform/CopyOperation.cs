using System.Collections.Generic;
using System.Threading;
using Transformalize.Libs.Rhino.Etl;

namespace Transformalize.Operations.Transform {
    public class CopyOperation : ShouldRunOperation {

        public CopyOperation(string inKey, string outKey) : base(inKey, outKey) { }

        public override IEnumerable<Row> Execute(IEnumerable<Row> rows) {
            foreach (var row in rows) {
                if (ShouldRun(row)) {
                    row[OutKey] = row[InKey];
                } else {
                    Interlocked.Increment(ref SkipCount);
                }
                yield return row;
            }
        }
    }
}