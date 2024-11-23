using System;
using UnityEngine;

namespace Neeto
{
    public class Bench : IDisposable
    {
        /* usage
         using (var b = new Bench())
         {
             // do stuff
         }
        // print logs
         */

        DateTime time;

        public Bench()
        {
            time = DateTime.Now;
        }
        public void Dispose()
        {
            var elapsed = (DateTime.Now - time).TotalMilliseconds;
            Debug.Log(elapsed);
        }
    }
}
