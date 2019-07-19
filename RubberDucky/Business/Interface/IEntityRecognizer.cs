using Microsoft.Recognizers.Text;
using RubberDucky.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RubberDucky.Business.Interface
{
    public interface IEntityRecognizer
    {
        List<Product> GetProducts(Dictionary<int, string> words, ModelResult modelResult);
    }
}
