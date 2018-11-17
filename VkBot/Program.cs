using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VkNet;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Enums.Filters;
using VkNet.Exception;

namespace VkBot
{
    class Program
    {
        //Настройки программы
        private static string _userLogin; //Логин пользователя
        private static bool _useTwoFactorAuthorization;//Использовать двухфакторную авторизацию
        private static bool _loadCopyHistory; //Загружать тексты всей истории репостов из постов, содержащих репосты
        private static ulong _loadPostsCount; //Количество загружаемых постов
        private static string _resultPostDomain; //Строковое id стены для размещения результатов
        private static int _roundDigitCount; //Количество знаков после запятой, выводимых в частотностях букв, если 0, то выводятся все знаки

        //Загрузка настроек из App.config, возращает false, если в файле ошибка
        private static bool LoadAppSettings()
        {
            try
            {
                _userLogin = ConfigurationManager.AppSettings["UserLogin"];
                if (_userLogin == null)
                {
                    Console.WriteLine("Ошибка чтения файла конфигурации App.config. Не указано значение параметра UserName");
                    return false;
                }

                _useTwoFactorAuthorization = ConfigurationManager.AppSettings["UseTwoFactorAuthorization"]?.ToLower() == "true";

                _loadCopyHistory = ConfigurationManager.AppSettings["LoadCopyHistory"]?.ToLower() == "true";

                if (!ulong.TryParse(ConfigurationManager.AppSettings["LoadPostsCount"], out _loadPostsCount))
                {
                    Console.WriteLine("Ошибка чтения файла конфигурации App.config. Не указано или некорректно значение параметра ResultPostDomain");
                    return false;
                }

                _resultPostDomain = ConfigurationManager.AppSettings["ResultPostDomain"];
                if (_resultPostDomain == null)
                {
                    Console.WriteLine("Ошибка чтения файла конфигурации App.config. Не указано значение параметра ResultPostDomain");
                    return false;
                }

                if (!int.TryParse(ConfigurationManager.AppSettings["RoundDigitCount"], out _roundDigitCount))
                    _roundDigitCount = 0;
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Ошибка чтения файла конфигурации App.config. В файле нет секции <appSettings>");
                return false;
            }
            return true;
        }

        //Загрузка параметров авторизации и запрос пароля
        //Возвращает true, если авторизация прошла успешно
        static bool Authorize(VkInteraction vk) //Объект для взаимодействия с ВК
        {
            //Авторизация прекращается при вводе правильного пароля с успехом, или при вводе пустой строки завершением работы программы
            while (true)
            {
                Console.WriteLine("Введите пароль пользователя:");
                string password = EnterPassword();
                if (string.IsNullOrEmpty(password))
                    return false;
                Func<string> twoFactorFun = _useTwoFactorAuthorization ? EnterConfirmCode : (Func<string>)null;
                try
                {
                    vk.Authorize(_userLogin, password, twoFactorFun);
                    break;
                }
                catch  (Exception ex) when (ex is VkApiAuthorizationException || ex is VkApiException)
                {
                    Console.WriteLine("Ошибка авторизации. Неправильный логин или пароль");
                }
            }
            return true;
        }

        //Ввод кода подтверждения
        private static string EnterConfirmCode()
        {
            Console.WriteLine("Введите код подтверждения:");
            return EnterPassword();
        }

        //Ввод пароля в консоль с заменой символов звездочками
        private static string EnterPassword()
        {
            string password = "";
            while (true)
            {
                ConsoleKeyInfo c = Console.ReadKey();
                Console.Clear();
                if (c.Key == ConsoleKey.Enter)
                    break;
                if (c.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                        password = password.Remove(password.Length - 1);
                }
                else
                    password += c.KeyChar;
                foreach (char ch in password)
                    Console.Write('*');
            }
            return password;
        }

        //Округление значений частотоностей для красивого вывода
        private static SortedDictionary<char, double> RoundFrequences(SortedDictionary<char, double> dic)
        {
            SortedDictionary<char, double> frequences;
            if (_roundDigitCount == 0)
                frequences = dic;
            else
            {
                frequences = new SortedDictionary<char, double>();
                foreach (var k in dic.Keys)
                    frequences[k] = Math.Round(dic[k], _roundDigitCount);
            }
            return frequences;
        }

        //Главная функция программы
        static void Main(string[] args)
        {
            if (!LoadAppSettings())
                return;
            using (var vk = new VkInteraction())
            {
                if (!Authorize(vk))
                {
                    Console.ReadLine();
                    return;
                }

                while (true)
                {
                    Console.WriteLine("Введите id аккаунта: ");
                    string domain = Console.ReadLine();
                    if (string.IsNullOrEmpty(domain))
                        break;
                    var wall = vk.GetWall(domain);
                    if (wall == null)
                        Console.WriteLine("Не найдена стена пользователя или группы " + domain);
                    else
                    {
                        //Загрузка постов
                        var posts = vk.LoadPostsText(wall, _loadPostsCount, _loadCopyHistory);

                        //Вычисление и округление частотностей букв
                        var frequences = new TextMetricsCalculator(posts).CalcLettersFrequences();
                        var roundedFrequences = RoundFrequences(frequences);
                        
                        string pref = $"{domain}, статистика для последних {_loadPostsCount} постов: ";
                        //В консоль выводим статистику красиво
                        Console.WriteLine(pref);
                        foreach (var pair in roundedFrequences)
                            Console.WriteLine($"\"{pair.Key}\": {pair.Value}");

                        //На стену выводим, как и просили, в формате JSON
                        var json = JsonConvert.SerializeObject(roundedFrequences);
                        var resWall = vk.GetWall(_resultPostDomain);
                        if (resWall == null)
                            Console.WriteLine("Не найдена стена пользователя или группы " + domain + " для размещения записей");

                        vk.WritePost(resWall, pref + json);
                    }
                }
            }
        }
    }
}
