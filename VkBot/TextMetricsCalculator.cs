using System;
using System.Collections.Generic;
using System.Linq;

namespace VkBot
{

    //Вычисление частотности букв набора текстов, можно добавить вычисление других метрик
    public class TextMetricsCalculator
    {
        //Конструктор получает на вход набор строк
        public TextMetricsCalculator(IEnumerable<string> strings)
        {
            _strings = strings.ToList();
        }

        private readonly List<string> _strings;

        //Также можно добавить в калькулятор строки по мере работы
        public void AddStrings(IEnumerable<string> strings)
        {
            _strings.AddRange(strings);
        }

        //Получить набор частотностей символов
        //На выходе отсортированный словарь
        public SortedDictionary<char, double> CalcLettersFrequences(bool caseSensitive = false, //Учитывать регистр букв
                                                                                                   Func<char, bool> filter = null) //Фильтр символов, по которым ведется статистика 
                                                                                                                                               // По умолчанию статистика ведется только для букв  
        {
            Func<char, bool> filtr = filter ?? char.IsLetter;
            var occurences = new SortedDictionary<char, int>();
            int count = 0;
            //Считаем количества вхождений символов
            foreach (var s in _strings)
            {
                string st = caseSensitive ? s : s.ToLower();
                foreach (char c in st)
                {
                    if (filtr(c))
                    {
                        if (!occurences.ContainsKey(c))
                            occurences.Add(c, 1);
                        else occurences[c]++;
                        count++;
                    }
                }    
            }
            //Считаем частотность символов
            var res = new SortedDictionary<char, double>();
            foreach (var pair in occurences)
                res.Add(pair.Key, pair.Value / (double)count);
            return res;
        }
    }
}