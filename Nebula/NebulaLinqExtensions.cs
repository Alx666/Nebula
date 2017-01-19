using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nebula.Shared
{
    public static class NebulaLinqExtensions
    {
        public static IEnumerable<TResult> SafeSelect<T, TResult>(this IEnumerable<T> hEnum, Func<T, TResult> hSelector) where TResult : class
        {
            foreach (T item in hEnum)
            {
                TResult res = null;

                try
                {
                    res = hSelector(item);
                }
                catch (Exception)
                {
                    //Skip elements if exception uccurs
                }

                if (res != null)
                    yield return res;
            }

        }
    }
}
