namespace BibliotecaAPI.DTOs
{
    public class LlaveAPIDTO
    {
        public int Id { get; set; }
        public required string Llave { get; set; }
        public bool Activa { get; set; }
        public required string TipoLlave { get; set; }
    }
}
