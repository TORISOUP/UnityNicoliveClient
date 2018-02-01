using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NicoliveClient
{
    [Serializable]
    internal struct OperatorCommentRequest
    {
        public string text;
        public string userName;
        public bool isPermanent;
        public string color;
    }

    [Serializable]
    internal struct BspCommentRequest
    {
        public string text;
        public string userName;
        public string color;
    }
}

