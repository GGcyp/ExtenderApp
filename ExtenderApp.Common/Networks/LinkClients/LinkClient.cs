using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    public class LinkClient : DisposableObject
    {
        private readonly ILinker _linker;


        public LinkClient(ILinker linker)
        {
            _linker = linker;
        }
    }
}