using BibliotecaAPI.Interfaces;

namespace BibliotecaAPI.Servicios
{
    public class AlmacenarArchivosLocal : IAlmacenarArchivos
    {
        private readonly IHttpContextAccessor contextAccessor;
        private readonly IWebHostEnvironment env;

        public AlmacenarArchivosLocal(IHttpContextAccessor contextAccessor, IWebHostEnvironment env)
        {
            this.contextAccessor = contextAccessor;
            this.env = env;
        }
        public async Task<string> Almacenar(string contenedor, IFormFile foto)
        {
            var extension = Path.GetExtension(foto.FileName);
            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            string folder = Path.Combine(env.WebRootPath, contenedor);

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string ruta = Path.Combine(folder, nombreArchivo);

            using(var ms = new MemoryStream())
            {
                await foto.CopyToAsync(ms);
                var contenido = ms.ToArray();
                await File.WriteAllBytesAsync(ruta, contenido);
            }

            var request = contextAccessor.HttpContext!.Request;
            var url = $"{request.Scheme}://{request.Host}";

            var urlArchivo = Path.Combine(url, contenedor, nombreArchivo).Replace("\\", "/");

            return urlArchivo;
        }

        public Task Borrar(string? ruta, string contenedor)
        {
            if (string.IsNullOrEmpty(ruta))
            {
                return Task.CompletedTask;
            }

            var nombreArchivo = Path.GetFileName(ruta);
            var directorioArchivo = Path.Combine(env.WebRootPath, contenedor, nombreArchivo);

            if (File.Exists(directorioArchivo))
            {
                File.Delete(directorioArchivo);
            }

            return Task.CompletedTask;
        }
    }
}
