using System;
using System.Collections.Generic;
using System.Linq;

namespace VkBot
{

    //Вычисление частотности букв набора текстов, можно добавить вычисление других метрик
    public class TextMetricsCalculator
    {
        //Конструктор получает на вход набор строк
        public TextMetricsCalculator(params string[] strings)
        {
            _strings = strings.ToList();
        }

        private readonly List<string> _strings;

        //Также можно добавить в калькулятор строки по мере работы
        public void AddStrings(params string[] strings)
        {
            _strings.AddRange(strings);
        }

        //Получить набор частотностей букв
        public Dictionary<char, double> CalcLettersFrequences()
        {
            var occurences = new Dictionary<char, int>();
            int count = 0;
            foreach (var s in _strings)
            {
                foreach (char c in s)
                {
                    if (!occurences.ContainsKey(c))
                        occurences.Add(c, 1);
                    else occurences[c]++;
                    count++;
                }    
            }
            var res = new Dictionary<char, double>();
            foreach (var pair in occurences)
                res.Add(pair.Key, pair.Value / (double)count);
            return res;
        }
    }
}