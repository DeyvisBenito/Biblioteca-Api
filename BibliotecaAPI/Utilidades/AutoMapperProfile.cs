using AutoMapper;
using BibliotecaAPI.DTOs;
using BibliotecaAPI.DTOs.UsuariosDTO;
using BibliotecaAPI.Entidades;

namespace BibliotecaAPI.Utilidades
{
    public class AutoMapperProfile: Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Autor, AutorDTO>()
                .ForMember(dto => dto.NombreCompleto, config => config.MapFrom(autor => NombreApellidoAutor(autor)));
            CreateMap<Autor, AutorConLibrosDTO>()
                .ForMember(dto => dto.NombreCompleto, config => config.MapFrom(autor => NombreApellidoAutor(autor)));
            CreateMap<Autor, AutorPatchDTO>().ReverseMap();

            CreateMap<AutorCreacionDTO, Autor>();
            CreateMap<AutorCreacionDTOConFoto, Autor>()
                .ForMember(autor => autor.Foto, config => config.Ignore());

            CreateMap<LibroCreacionDTO, AutorLibro>()
                     .ForMember(autorLibro => autorLibro.Libro, config => config.MapFrom(dto => new Libro { Nombre = dto.Nombre }));

            CreateMap<Libro, LibroDTO>();

            CreateMap<AutorLibro, LibroDTO>()
                .ForMember(dto => dto.Id, config => config.MapFrom(autorLibro => autorLibro.LibroId))
                .ForMember(dto => dto.Nombre, config => config.MapFrom(autorLibro => autorLibro.Libro!.Nombre));


            CreateMap<Libro, LibroConAutorDTO>();

            CreateMap<AutorLibro, AutorDTO>()
                .ForMember(dto => dto.Id, config => config.MapFrom(autorLibro => autorLibro.AutorId))
                .ForMember(dto => dto.NombreCompleto, config => config.MapFrom(autorLibro => NombreApellidoAutor(autorLibro.Autor!)));

            CreateMap<LibroCreacionDTO, Libro>().ForMember(libro => libro.Autores, config =>
                                                config.MapFrom(dto => 
                                                dto.AutoresId.Select(autorId => new AutorLibro { AutorId = autorId})));

            //Mapeo de comentarios
            CreateMap<Comentario, ComentarioDTO>()
                .ForMember(dto => dto.UsuarioEmail, config => config.MapFrom(comentario => comentario.Usuario!.Email));
            CreateMap<ComentarioCreacionDTO, Comentario>();
            CreateMap<Comentario, ComentarioPatchDTO>().ReverseMap();


            //Mapeo de usuarios
            CreateMap<Usuario, UsuarioDTO>();
        }

        private string NombreApellidoAutor(Autor autor) => $"{autor.Nombres} {autor.Apellidos}";
    }
}
