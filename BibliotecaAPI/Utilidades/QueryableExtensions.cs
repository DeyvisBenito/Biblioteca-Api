using BibliotecaAPI.DTOs;

namespace BibliotecaAPI.Utilidades
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> Paginar<T>(this IQueryable<T> queryable, PaginacionDTO paginacionDTO)
        {
            return queryable.Skip((paginacionDTO.Pagina - 1) * paginacionDTO.CantidadRecordsPorPagina)
                .Take(paginacionDTO.CantidadRecordsPorPagina);
        }
    }
}
