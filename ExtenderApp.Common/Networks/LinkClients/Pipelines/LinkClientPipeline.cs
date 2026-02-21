using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using ExtenderApp.Abstract;

namespace ExtenderApp.Common.Networks.LinkClients
{
    internal sealed class LinkClientPipeline : ILinkClientPipeline
    {
        public ILinkClientPipeline AddAfter(string baseName, string name, ILinkClientHandler handler)
        {
            throw new NotImplementedException();
        }

        public ILinkClientPipeline AddBefore(string baseName, string name, ILinkClientHandler handler)
        {
            throw new NotImplementedException();
        }

        public ILinkClientPipeline AddFirst(string name, ILinkClientHandler handler)
        {
            throw new NotImplementedException();
        }

        public ILinkClientPipeline AddLast(string name, ILinkClientHandler handler)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<ILinkClientHandler> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public ILinkClientPipeline Remove(ILinkClientHandler handler)
        {
            throw new NotImplementedException();
        }

        public ILinkClientHandler Remove(string name)
        {
            throw new NotImplementedException();
        }

        public ILinkClientPipeline Replace(string oldName, string newName, ILinkClientHandler newHandler)
        {
            throw new NotImplementedException();
        }

        public ILinkClientPipeline Replace(ILinkClientHandler oldHandler, string newName, ILinkClientHandler newHandler)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}