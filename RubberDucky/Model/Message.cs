using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RubberDucky.Model
{
    public class Message
    {
        public string Text { get; set; }
        [NotMapped]
        public Dictionary<int, string> Words { get; set; }
        public DateTime Recieved { get; set; }
        public bool IsUser { get; set; }
        [Key]
        public string Id { get; set; }

        public void UpdateText(string text)
        {
            Text = text;
            Words = GetWords(text);
        }

        private Dictionary<int, string> GetWords(string input)
        {
            MatchCollection matches = Regex.Matches(input, @"\b[\w']*\b");

            var words = from m in matches.Cast<Match>()
                        where !string.IsNullOrEmpty(m.Value)
                        select TrimSuffix(m.Value);
            
            return ToDict(words, input);
        }

        private Dictionary<int, string> ToDict(IEnumerable<string> words, string input)
        {
            var dict = new Dictionary<int, string>();
            var pointer = 0;
            foreach(var word in words)
            {
                dict.Add(input.Substring(pointer).IndexOf(word) + pointer, word);
                pointer = dict.Keys.Max() + word.Length;
            }
            return dict;
        }

        private string TrimSuffix(string word)
        {
            int apostropheLocation = word.IndexOf('\'');
            if (apostropheLocation != -1)
            {
                word = word.Substring(0, apostropheLocation);
            }

            return word;
        }
    }
}
