using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Transfer.Helpers
{
    public interface IHttpRequestMessageHelper
    {
        Task<T> GetTransferFromRequest<T>(HttpRequestMessage req);
        Guid? GetTouchpointId(HttpRequestMessage req);
    }
}