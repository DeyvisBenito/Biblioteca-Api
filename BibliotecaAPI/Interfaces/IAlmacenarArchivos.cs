namespace BibliotecaAPI.Interfaces
{
    public interface IAlmacenarArchivos
    {
        Task Borrar(string? ruta, string contenedor);
        Task<string> Almacenar(string contenedor, IFormFile foto);
        async Task<string> Actualizar(string? ruta, string contenedor, IFormFile foto)
        {
            await Borrar(ruta, contenedor);
            return await Almacenar(contenedor, foto);
        }
    }
}
