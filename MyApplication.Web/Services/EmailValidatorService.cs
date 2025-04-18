using DnsClient;
using System;
using System.Linq;

namespace MyApplication.Web.Services
{
    public class EmailValidatorService
    {
        public bool CheckEmailDomainExists(string email)
        {
            try
            {
                var domain = email.Split('@')[1];
                var lookup = new LookupClient();
                var result = lookup.Query(domain, QueryType.MX);

                var mxRecords = result.Answers.MxRecords();
                Console.WriteLine($"MX kayıtları: {string.Join(",", mxRecords.Select(mx => mx.Exchange.ToString()))}");

                return mxRecords.Any();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                return false;
            }
        }
    }
}
