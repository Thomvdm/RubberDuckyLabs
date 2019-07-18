using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RubberDucky.Extensions
{
    public static class SessionExtensions
    {
        public static T Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);

            return value == null ? (T)Activator.CreateInstance(typeof(T)) :
                JsonConvert.DeserializeObject<T>(value);
        }

        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static void SetString(this ISession session, string key, string value)
        {
            session.Set(key, Encoding.UTF8.GetBytes(value));
        }
    }
}
