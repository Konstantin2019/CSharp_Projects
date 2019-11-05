using System;

namespace VkBot
{
    /// <summary>
    /// Вспомогательный класс, позволяющий получать объекта типа Response, 
    /// содержащий целевой объект и объект исключения
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Response<T>
    {
        public T Value { get; set; }

        public string Error { get; set; }
    }
}
