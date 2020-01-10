namespace FireBase_lib.Entities
{
    /// <summary>
    /// Интерфейс сериализуемых объектов
    /// </summary>
    public interface ISerializableObject
    {
        string Name { get; set; }
        string Value { get; set; }
    }
}
