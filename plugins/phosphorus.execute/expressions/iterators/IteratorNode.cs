/*
 * phosphorus five, copyright 2014 - Mother Earth, Jannah, Gaia
 * phosphorus five is licensed as mit, see the enclosed LICENSE file for details
 */

using System;
using System.Collections.Generic;
using phosphorus.core;

namespace phosphorus.execute.iterators
{
    public class IteratorNode : Iterator
    {
        private IEnumerable<Node> _nodes;

        public IteratorNode (IEnumerable<Node> nodes)
        {
            _nodes = nodes;
        }

        public override IEnumerable<Node> Evaluate {
            get {
                foreach (Node idx in _nodes) {
                    yield return idx;
                }
            }
        }
    }
}

