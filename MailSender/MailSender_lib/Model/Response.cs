namespace MailSender_lib.Model
{
    /// <summary>
    /// Вспомогательный класс, позволяющий получать объекта типа Response, 
    /// содержащий целевой объект и объект исключения
    /// </summary>
    public class Response
    {
        public bool Success { get; set; }

        public string Error { get; set; }
    }
}
