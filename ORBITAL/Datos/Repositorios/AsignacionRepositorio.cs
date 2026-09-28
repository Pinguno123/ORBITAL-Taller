using ORBITAL.Datos.Context;
using ORBITAL.Dominio.Entidades;
using ORBITAL.Dominio.Enumeraciones;
using ORBITAL.Dominio.Excepciones;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace ORBITAL.Datos.Repositorios
{
    public class AsignacionRepositorio
    {
        public void RegistrarAsignacion(int misionId, int recursoId, decimal cantidadOperacion = 1)
        {
            using (var db = new orbita_controlEntities())
            {
                var efRecurso = db.recurso_exploracion.Find(recursoId);
                if (efRecurso == null)
                    throw new InvalidOperationException($"No se encontró el recurso con Id {recursoId}.");

                if (efRecurso.estado != (byte)EstadoRecurso.Disponible)
                    throw new RecursoNoDisponibleException($"El recurso {efRecurso.codigo} no está disponible (Estado: {(EstadoRecurso)efRecurso.estado}).");

                var efMision = db.mision.Find(misionId);
                if (efMision == null)
                    throw new InvalidOperationException($"No se encontró la misión con Id {misionId}.");

                var asignacion = new asignacion_recurso
                {
                    mision_id = misionId,
                    recurso_id = recursoId,
                    fecha_asignacion = DateTime.Now,
                    fecha_liberacion = null,
                    activa = true,
                    cantidad_operacion = cantidadOperacion > 0 ? cantidadOperacion : 1
                };

                db.asignacion_recurso.Add(asignacion);
                efRecurso.estado = (byte)EstadoRecurso.Asignado;

                db.SaveChanges();
            }
        }

        public void LiberarRecursoDeMision(int misionId, int recursoId)
        {
            using (var db = new orbita_controlEntities())
            {
                var asignacion = db.asignacion_recurso
                    .FirstOrDefault(a => a.mision_id == misionId && a.recurso_id == recursoId && a.activa);

                if (asignacion != null)
                {
                    asignacion.activa = false;
                    asignacion.fecha_liberacion = DateTime.Now;

                    var recurso = db.recurso_exploracion.Find(recursoId);
                    if (recurso != null)
                    {
                        recurso.estado = (byte)EstadoRecurso.Disponible;
                    }

                    db.SaveChanges();
                }
            }
        }

        public void LiberarTodasDeMision(int misionId)
        {
            using (var db = new orbita_controlEntities())
            {
                var asignaciones = db.asignacion_recurso
                    .Where(a => a.mision_id == misionId && a.activa)
                    .ToList();

                DateTime ahora = DateTime.Now;
                foreach (var asig in asignaciones)
                {
                    asig.activa = false;
                    asig.fecha_liberacion = ahora;

                    var recurso = db.recurso_exploracion.Find(asig.recurso_id);
                    if (recurso != null)
                    {
                        recurso.estado = (byte)EstadoRecurso.Disponible;
                    }
                }

                db.SaveChanges();
            }
        }

        public List<vw_asignaciones_activas> ObtenerAsignacionesActivas()
        {
            using (var db = new orbita_controlEntities())
            {
                return db.vw_asignaciones_activas.AsNoTracking().ToList();
            }
        }

        public List<asignacion_recurso> ObtenerPorMision(int misionId)
        {
            using (var db = new orbita_controlEntities())
            {
                return db.asignacion_recurso
                    .AsNoTracking()
                    .Where(a => a.mision_id == misionId)
                    .ToList();
            }
        }
    }
}
