using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gurgle
{
    public abstract class AfterPostEventBase : EventArgs
    {
        public virtual object PostResponse { get; protected set; }
        public Exception PostError { get; protected set; }

        protected AfterPostEventBase(object postResponse)
            : this(postResponse,null)
        { }

        protected AfterPostEventBase(object postResponse, Exception error)
        {
            PostResponse = postResponse;
            PostError = error;
        }
    }
}
