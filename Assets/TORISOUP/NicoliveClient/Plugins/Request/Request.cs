using System;

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

    [Serializable]
    internal struct EnqueteRequest
    {
        public string programId;
        public string question;
        public string[] items;
    }
}

