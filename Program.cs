using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace krsclient.net
{
    class Program
    {
        static void Main(string[] args)
        {
            Replicator replicator = new Replicator();
            replicator.Replicate();
        }
    }
}
