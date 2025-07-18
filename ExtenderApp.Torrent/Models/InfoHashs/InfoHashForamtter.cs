﻿using ExtenderApp.Abstract;
using ExtenderApp.Common.IO.Binaries.Formatter;
using ExtenderApp.Data;

namespace ExtenderApp.Torrent.Models.InfoHashs
{
    internal class InfoHashForamtter : ResolverFormatter<InfoHash>
    {
        private readonly IBinaryFormatter<HashValue> _hashValue;
        public override int Length => _hashValue.Length;
        public InfoHashForamtter(IBinaryFormatterResolver resolver) : base(resolver)
        {
            _hashValue = GetFormatter<HashValue>();
        }

        public override InfoHash Deserialize(ref ExtenderBinaryReader reader)
        {
            var sha1 = _hashValue.Deserialize(ref reader);
            var sha256 = _hashValue.Deserialize(ref reader);
            return new InfoHash(sha1, sha256);
        }

        public override void Serialize(ref ExtenderBinaryWriter writer, InfoHash value)
        {
            _hashValue.Serialize(ref writer, value.sha1);
            _hashValue.Serialize(ref writer, value.sha256);
        }
    }
}
