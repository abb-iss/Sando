using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sando.IntegrationTests.LocalSearch
{
    public class NTree<T>
    {
        T data;
        LinkedList<NTree<T>> children;

        public NTree(T data)
        {
            this.data = data;
            children = new LinkedList<NTree<T>>();
        }

        public T getData()
        {
            return this.data;
        }

        public void addChild(T data)
        {
            children.AddLast(new NTree<T>(data));
        }

        public NTree<T> getChild(int i)
        {
            foreach (NTree<T> n in children)
                if (i-- == 0) return n;
            return null;
        }

        public int getChildNumber()
        {
            return children.Count;
        }

        public void RemoveChildren()
        {
            children.Clear();
        }

        public void RemoveChild(NTree<T> child)
        {
            children.Remove(child);
        }

    }
        
}
