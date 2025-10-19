using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks
{
    public class LinkClient<T> : DisposableObject where T : IPipelineContext
    {
        private readonly ILinker _linker;

        public PipelineExecute<T>? PipelineExecute { get; set; }

        public LinkClient(ILinker linker)
        {
            _linker = linker;
        }
    }
}