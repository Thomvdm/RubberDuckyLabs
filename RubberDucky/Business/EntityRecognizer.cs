using Microsoft.EntityFrameworkCore;
using Microsoft.Recognizers.Text;
using RubberDucky.Data;
using RubberDucky.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RubberDucky.Business
{
    public class EntityRecognizer
    {
        private readonly AppDbContext _db;
        public EntityRecognizer(AppDbContext db)
        {
            _db = db;
        }

        public List<Product> GetProducts(Dictionary<int, string> words, ModelResult modelResult)
        {
            var possibleProducts = new List<Product>();
            foreach (var word in words)
            {
                if (word.Key > modelResult.End)
                {
                    possibleProducts = _db.Set<Product>().Where(p => p.ProductName.ToLower().Contains(word.Value.ToLower())).ToList();
                }
                if (possibleProducts.Count > 0)
                {
                    break;
                }
            }
            return possibleProducts;
        }
    }
}
