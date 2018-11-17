using System;
using System.Collections.Generic;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Exception;
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
                ApplicationId = AppId,
                Login = login,
                Password = password,
                Settings = Settings.All,
                TwoFactorAuthorization = getCode
            });
        }

        //Получение стены пользователя или сообщества по короткому имени
        //Если стена не найдена, возвращается null
        public VkWall GetWall(string domain)
        {
            try
            {
                long id = _api.Users.Get(new[] { domain })[0].Id;
                return new VkWall(domain, id, VkWallType.User);
            }
            catch (InvalidUserIdException) { }

            try
            {
                long id = -_api.Groups.GetById(new[] { domain }, domain, GroupsFields.CanSeelAllPosts)[0].Id;
                return new VkWall(domain, id, VkWallType.Group);
            }
            catch (InvalidGroupIdException) { }
            return null;
        }

        //Загрузка текстов постов на стене пользователя или группы
        public List<string> LoadPostsText(VkWall wall, //Стена пользователя или группы
                                                          ulong count, //Количество загружаемых постов
                                                          bool loadCopyHistory = true) //Для репостов, кроме текста поста, загружать тексты всей истории репоста
        {
            var wallGet = _api.Wall.Get(new WallGetParams { OwnerId = wall.Id, Count = count });
            var res = new List<string>();
            foreach (var post in wallGet.WallPosts)
            {
                string st = post.Text;
                if (loadCopyHistory)
                    foreach (var hist in post.CopyHistory)
                        st += hist.Text;
                res.Add(st);
            }
            return res;
        }
        
        //Запись поста на стене пользователя или группы
        public void WritePost(VkWall wall, //Стена пользователя или группы
                                         string text) //Текст поста
        {
            _api.Wall.Post(new WallPostParams { OwnerId = wall.Id, Message = text});
        }

        //При уничтожении объекта также будет уничножен объект VkApi
        public void Dispose()
        {
            _api.Dispose();
        }
    }
}