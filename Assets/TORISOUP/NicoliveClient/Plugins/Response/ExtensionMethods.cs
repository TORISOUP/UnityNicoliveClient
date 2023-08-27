using System;

namespace NicoliveClient
{
    /// <summary>
    /// 延長種別
    /// </summary>
    public struct ExtensionMethods
    {
        /// <summary>
        /// 延長方法（無料のみ）
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// 延長可能分数
        /// </summary>
        public int Minutes { get; private set; }

        public ExtensionMethods(string type, int minutes) : this()
        {
            Type = type;
            Minutes = minutes;
        }
    }

    [Serializable]
    internal struct ExtensionMethodsDto
    {
        public string type;
        public int minutes;

        public ExtensionMethods ToExtensionMethods()
        {
            return new ExtensionMethods(type, minutes);
        }
    }

    [Serializable]
    internal struct MethodsDto
    {
        public ExtensionMethodsDto[] methods;

    }
}
