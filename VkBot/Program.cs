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
        private static int _loadPostsCount; //Количество загружаемых постов
        private static string _resultPostDomain; //Строковое id стены для размещения результатов

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

                if (!int.TryParse(ConfigurationManager.AppSettings["LoadPostsCount"], out _loadPostsCount))
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
            }
            catch (ConfigurationErrorsException ex)
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
            if (!LoadAppSettings())
                return false;

            Console.WriteLine("Введите пароль пользователя:");
            string password = EnterPassword();
            Func<string> twoFactorFun = _useTwoFactorAuthorization ? EnterConfirmCode : (Func<string>) null;

            vk.Authorize(_userLogin, password, twoFactorFun);
            
            return true;
        }

        //Ввод кода подтверждения
        private static string EnterConfirmCode()
        {
            Console.WriteLine("Введите код подтверждения:");
            return EnterPassword();
        }

        //Ввод пароля в консоль
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

        static void Main(string[] args)
        {
            using (var vk = new VkInteraction())
            {
                if (!Authorize(vk))
                {
                    Console.ReadLine();
                    return;
                }

                while (true)
                {
                    //club173913243
                    Console.WriteLine("Введите id аккаунта: ");
                    string domain = Console.ReadLine();
                    if (string.IsNullOrEmpty(domain))
                        break;
                    var posts = vk.LoadPostsText(domain, _loadPostsCount, _loadCopyHistory);
                    var frequences = new TextMetricsCalculator(posts.ToArray()).CalcLettersFrequences();
                    var json = JsonConvert.SerializeObject(frequences);
                    Console.WriteLine(json);
                    //vk.WritePost(_resultPostDomain, json);
                }
            }
        }

    }
}
