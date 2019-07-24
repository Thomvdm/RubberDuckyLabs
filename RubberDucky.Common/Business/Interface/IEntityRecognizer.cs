using Microsoft.Recognizers.Text;
using RubberDucky.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RubberDucky.Common.Business.Interface
{
    public interface IEntityRecognizer
    {
        List<Product> GetProducts(Dictionary<int, string> words, ModelResult modelResult);
    }
}
