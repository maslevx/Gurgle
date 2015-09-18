using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gurgle
{
    public abstract class ServiceRequest
    {
        private string m_epUrl;
        public string EPUrl
        {
            get
            {
                if (String.IsNullOrWhiteSpace(m_epUrl))
                    m_epUrl = GetValidEP();
                return m_epUrl;
            }
            set { m_epUrl = value; }
        }

        private string m_environment;
        public string Environment
        {
            get { return m_environment; }
            set { m_environment = value.ToUpper(); }
        }

        private string GetValidEP()
        {
            string rtnVal = ResolveEP();
            if (String.IsNullOrWhiteSpace(rtnVal))
                throw new Exception("No EPUrl has been provided");
            return rtnVal;
        }
        
        protected abstract string ResolveEP();
    }
}
