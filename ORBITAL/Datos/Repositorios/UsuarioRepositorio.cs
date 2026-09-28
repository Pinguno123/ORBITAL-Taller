using ORBITAL.Datos.Context;
using ORBITAL.Dominio.Entidades;
using ORBITAL.Dominio.Enumeraciones;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace ORBITAL.Datos.Repositorios
{
    public class UsuarioRepositorio
    {
        public Usuario ValidarCredenciales(string nombreUsuario, string contrasena)
        {
            using (var db = new orbita_controlEntities())
            {
                var efUser = db.usuario
                    .FirstOrDefault(u => u.nombre_usuario == nombreUsuario && u.contrasena == contrasena);

                if (efUser == null)
                    return null;

                return MapearADominio(efUser);
            }
        }

        public Usuario ObtenerPorNombreUsuario(string nombreUsuario)
        {
            using (var db = new orbita_controlEntities())
            {
                var efUser = db.usuario.FirstOrDefault(u => u.nombre_usuario == nombreUsuario);
                return efUser != null ? MapearADominio(efUser) : null;
            }
        }

        public Usuario ObtenerPorId(int id)
        {
            using (var db = new orbita_controlEntities())
            {
                var efUser = db.usuario.Find(id);
                return efUser != null ? MapearADominio(efUser) : null;
            }
        }

        public List<Usuario> ObtenerTodos()
        {
            using (var db = new orbita_controlEntities())
            {
                return db.usuario
                    .AsNoTracking()
                    .ToList()
                    .Select(MapearADominio)
                    .ToList();
            }
        }

        public void Registrar(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            using (var db = new orbita_controlEntities())
            {
                var efUser = new usuario
                {
                    nombre_usuario = usuario.NombreUsuario,
                    contrasena = usuario.Contraseña,
                    rol = (byte)usuario.Rol,
                    estado = (byte)usuario.Estado
                };

                db.usuario.Add(efUser);
                db.SaveChanges();
                usuario.Id = efUser.id;
            }
        }

        public static Usuario MapearADominio(usuario efUser)
        {
            if (efUser == null)
                return null;

            return new Usuario(
                efUser.id,
                efUser.nombre_usuario,
                efUser.contrasena,
                (RolUsuario)efUser.rol,
                (EstadoUsuario)efUser.estado
            );
        }
    }
}
