using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace Rnet.Service
{

    public static class ObservableExtensions
    {

        public static IObservable<TRet> SelectAsync<T, TRet>(this IObservable<T> select, Func<T, Task<TRet>> selector, bool preserveOrdering = false)
        {
            return preserveOrdering ?
                select.Select(x => selector(x).ToObservable()).Concat() :
                select.SelectMany(x => selector(x).ToObservable());
        }

        public static IObservable<T> WhereAsync<T>(this IObservable<T> predicate, Func<T, Task<bool>> filter)
        {
            return predicate.Select(x =>
                filter(x).ToObservable()
                    .SelectMany(cond => cond ? Observable.Return(x) : Observable.Empty<T>()))
                        .Concat();
        }

        public static IObservable<bool> AllAsync<T>(this IObservable<T> self, Func<T, Task<bool>> predicate)
        {
            return self
                .SelectMany(x => predicate(x).ToObservable())
                .All(x => x != false);
        }

        public static IObservable<bool> AnyAsync<T>(this IObservable<T> self, Func<T, Task<bool>> predicate)
        {
            return self
                .SelectMany(x => predicate(x).ToObservable())
                .Any(x => x != false);
        }

    }

}