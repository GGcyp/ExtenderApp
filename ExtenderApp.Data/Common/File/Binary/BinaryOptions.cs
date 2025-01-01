using ExtenderApp.Data.File;

namespace ExtenderApp.Data
{
    public class BinaryOptions
    {
        public BinaryCode BinaryCode { get; set; }

        public BinaryRang BinaryRang { get; set; }

        public DateTimeConstants DateTimeConstants { get; set; }

        public BinaryOptions()
        {
            BinaryCode = new BinaryCode();
            BinaryRang = new BinaryRang();
            DateTimeConstants = new DateTimeConstants();
        }
    }
}
