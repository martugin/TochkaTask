namespace VkBot
{
    //Принадлежность стены ВК
    public enum VkWallType
    {
        User, //Пользователь
        Group //Группа
    }

    //Обертка для стены пользователя или группы BK
    public class VkWall
    {
        public VkWall(string domain, long id, VkWallType wallType)
        {
            Domain = domain;
            Id = id;
            WallType = wallType;
        }

        //Короткий строковый адрес пользователя или группы
        public string Domain { get; }
        //Id пользователя или группы, уже с правильным знаком (-+) для использования
        public long Id { get; }
        //Принадлежность стены 
        public VkWallType WallType { get; }
    }
}