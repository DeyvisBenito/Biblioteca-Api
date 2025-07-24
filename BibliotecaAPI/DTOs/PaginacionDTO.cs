namespace BibliotecaAPI.DTOs
{
    public record PaginacionDTO(int Pagina = 1, int CantidadRecordsPorPagina = 10)
    {
        private const int cantidadMaximaRecordPorPagina = 50;

        public int Pagina { get; init; } = Math.Max(Pagina, 1);

        public int CantidadRecordsPorPagina { get; init; } = Math.Clamp(CantidadRecordsPorPagina, 1, cantidadMaximaRecordPorPagina);
    }
}
