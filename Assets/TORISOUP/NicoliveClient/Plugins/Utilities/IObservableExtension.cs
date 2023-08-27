using System;
using UniRx;

namespace TORISOUP.NicoliveClient.Utilities
{
    internal static class IObservableExtension
    {
        /// <summary>
        /// 対象をHot変換する
        /// </summary>
        public static IObservable<T> Kick<T>(this IObservable<T> original)
        {
            var connectableObservable = original.PublishLast();
            connectableObservable.Connect();
            return connectableObservable;
        }
    }
}
