using ExtenderApp.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtenderApp.Common.File
{
    internal class JsonParserStore : FileParserStore<IJsonParser>
    {
        public JsonParserStore(IEnumerable<IJsonParser> paresers) : base(paresers)
        {
        }
    }
}
