namespace BibliotecaAPI.DTOs
{
    public class FiltrarAutoresDTO
    {
        public int Pagina { get; set; } = 1;
        public int RegistrosPorPagina { get; set; } = 10;
        public PaginacionDTO PaginacionDTO { get
            {
                return new PaginacionDTO
                {
                    Pagina = Pagina,
                    CantidadRecordsPorPagina = RegistrosPorPagina
                };
            }
        }

        public string? Nombres { get; set; }
        public string? Apellidos { get; set; }
        public bool? TieneFoto { get; set; }
        public bool? TieneLibros { get; set; }
        public string? TituloLibro { get; set; }
        public bool IncluirLibros { get; set; }
        public string? CampoOrdenar { get; set; }
        public bool OrdenarAscendente { get; set; } = true;
    }
}
