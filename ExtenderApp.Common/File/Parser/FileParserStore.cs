using ExtenderApp.Data;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.File
{
    public abstract class FileParserStore<TPareser> : Store<TPareser> where TPareser : class, IFileParser
    {
        public FileParserStore(IEnumerable<TPareser> paresers) : base(paresers)
        {

        }
    }
}
