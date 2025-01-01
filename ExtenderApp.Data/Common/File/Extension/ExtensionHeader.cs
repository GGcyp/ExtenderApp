

namespace ExtenderApp.Data
{
    public struct ExtensionHeader : IEquatable<ExtensionHeader>
    {
        public sbyte TypeCode { get; private set; }

        public uint Length { get; private set; }

        public ExtensionHeader(sbyte typeCode, uint length)
        {
            this.TypeCode = typeCode;
            this.Length = length;
        }

        public ExtensionHeader(sbyte typeCode, int length)
        {
            this.TypeCode = typeCode;
            this.Length = (uint)length;
        }

        public bool Equals(ExtensionHeader other) => this.TypeCode == other.TypeCode && this.Length == other.Length;
    }
}
