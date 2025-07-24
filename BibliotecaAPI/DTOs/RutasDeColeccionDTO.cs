namespace BibliotecaAPI.DTOs
{
    public class RutasDeColeccionDTO<T> : RecursoDTO where T : RecursoDTO
    {
        public IEnumerable<T> Valores { get; set; } = [];
    }
}
