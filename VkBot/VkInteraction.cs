using System;
using System.Collections.Generic;
using System.Linq;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace VkBot
{
    //Класс для чтения и записи текстов из ВК
    public class VkInteraction : IDisposable
    {
        //Id этого приложения, полученное от ВК
        private const int AppId = 6750697;
        //Объект длявзаимодействия с API ВКонтакте, используется библиотека VK.Net, подключена в NuGet
        private readonly VkApi _api = new VkApi();

        //Авторизация приложения VK
        public void Authorize(string login, //Логин пользователя
                                                        string password, //Пароль пользователя
                                                        Func<string> getCode = null) //Функция, для получения кода второго этапа авторизации, если авторизация однофакторная, то null
        {
            _api.Authorize(new ApiAuthParams
            {
                ApplicationId = 6750697,
                Login = login,
                Password = password,
                Settings = Settings.All,
                TwoFactorAuthorization = getCode
            });
        }
 
        //Загрузка текстов постов на стене пользователя или группы
        public List<string> LoadPostsText(string domain, //Короткий адрес пользователя или сообщества
                                                          int count, //Количество загружаемых постов
                                                          bool loadCopyHistory = true) //Для репостов, кроме текста поста, загружать тексты всей истории репоста
        {
            var res = new List<string>();
            var wall = _api.Wall.Get(new WallGetParams { Domain = domain, Count = 5 });
            foreach (var post in wall.WallPosts)
            {
                string st = post.Text;
                if (loadCopyHistory)
                    foreach (var hist in post.CopyHistory)
                        st += hist.Text;
                res.Add(st);
            }
            return res;
        }

        public void WritePost(long ownerId, //Id пользователя или сообщества (передается со знаком минус)
                                         string text) //Текст поста
        {
            _api.Wall.Post(new WallPostParams {OwnerId = ownerId, Message = text});
        }

        //Запись текста на стену пользователя или сообщества
        public void WritePost(string domain, //Короткий адрес пользователя или сообщества
                                         string text) //Текст поста
        {
            WritePost(_api.Users.Get(new[] {domain})[0].Id, text);
            //var group = api.Groups.GetById(new[] {"club173913243"}, "club173913243", GroupsFields.CanSeelAllPosts);
        }

        //При уничтожении объекта также будет уничножен объект VkApi
        public void Dispose()
        {
            _api.Dispose();
        }
    }
}