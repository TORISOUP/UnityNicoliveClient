using System;

namespace TORISOUP.NicoliveClient.Response
{
    [Serializable]
    internal struct ApiResponseDto<T>
    {
        public T data;
        public MetaDto meta;
    }

    public struct ApiResponse<T>
    {
        public T Data { get; private set; }
        public Meta Meta { get; private set; }
    }

    [Serializable]
    internal struct MetaDto
    {
        public string errorCode;
        public int status;

        public Meta ToMeta()
        {
            return new Meta(errorCode, status);
        }
    }

    public struct Meta
    {
        public string ErrorCode { get; private set; }
        public int Status { get; private set; }

        public Meta(string errorCode, int status) : this()
        {
            ErrorCode = errorCode;
            Status = status;
        }
    }

    [Serializable]
    internal struct ErrorResponseDto
    {
        public MetaDto meta;
    }
}
