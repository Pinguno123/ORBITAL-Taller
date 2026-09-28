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
    public class MisionRepositorio
    {
        private readonly RecursoRepositorio recursoRepositorio = new RecursoRepositorio();

        public List<Mision> ObtenerTodas()
        {
            using (var db = new orbita_controlEntities())
            {
                var efMisiones = db.mision
                    .Include(m => m.usuario)
                    .Include(m => m.asignacion_recurso)
                    .AsNoTracking()
                    .ToList();

                var lista = new List<Mision>();
                foreach (var efM in efMisiones)
                {
                    lista.Add(MapearMisionConRecursos(db, efM));
                }
                return lista;
            }
        }

        public Mision ObtenerPorId(int id)
        {
            using (var db = new orbita_controlEntities())
            {
                var efM = db.mision
                    .Include(m => m.usuario)
                    .Include(m => m.asignacion_recurso)
                    .AsNoTracking()
                    .FirstOrDefault(m => m.id == id);

                return efM != null ? MapearMisionConRecursos(db, efM) : null;
            }
        }

        public Mision ObtenerPorCodigo(string codigo)
        {
            using (var db = new orbita_controlEntities())
            {
                var efM = db.mision
                    .Include(m => m.usuario)
                    .Include(m => m.asignacion_recurso)
                    .AsNoTracking()
                    .FirstOrDefault(m => m.codigo == codigo);

                return efM != null ? MapearMisionConRecursos(db, efM) : null;
            }
        }

        public List<Mision> ObtenerPorEstado(EstadoMision estado)
        {
            using (var db = new orbita_controlEntities())
            {
                byte estadoByte = (byte)estado;
                var efMisiones = db.mision
                    .Include(m => m.usuario)
                    .Include(m => m.asignacion_recurso)
                    .AsNoTracking()
                    .Where(m => m.estado == estadoByte)
                    .ToList();

                var lista = new List<Mision>();
                foreach (var efM in efMisiones)
                {
                    lista.Add(MapearMisionConRecursos(db, efM));
                }
                return lista;
            }
        }

        public void Crear(Mision mision)
        {
            if (mision == null)
                throw new ArgumentNullException(nameof(mision));

            if (string.IsNullOrWhiteSpace(mision.Codigo))
                throw new MisionInvalidaException("El código de la misión es obligatorio.");

            if (string.IsNullOrWhiteSpace(mision.Nombre))
                throw new MisionInvalidaException("El nombre de la misión es obligatorio.");

            if (string.IsNullOrWhiteSpace(mision.Descripcion))
                throw new MisionInvalidaException("La descripción de la misión es obligatoria.");

            if (mision.FechaFinEstimada < mision.FechaInicio)
                throw new MisionInvalidaException("La fecha estimada de finalización no puede ser anterior a la fecha de inicio.");

            using (var db = new orbita_controlEntities())
            {
                if (db.mision.Any(m => m.codigo == mision.Codigo))
                {
                    throw new MisionInvalidaException($"Ya existe una misión con el código '{mision.Codigo}'.");
                }

                if (mision.Responsable == null || mision.Responsable.Id <= 0)
                {
                    throw new MisionInvalidaException("La misión debe tener asignado un responsable válido de la base de datos.");
                }

                var efMision = new mision
                {
                    codigo = mision.Codigo,
                    nombre = mision.Nombre,
                    descripcion = mision.Descripcion,
                    fecha_inicio = mision.FechaInicio,
                    fecha_fin_estimada = mision.FechaFinEstimada,
                    prioridad = (byte)mision.Prioridad,
                    estado = (byte)mision.Estado,
                    responsable_id = mision.Responsable.Id
                };

                db.mision.Add(efMision);
                db.SaveChanges();

                mision.Id = efMision.id;

                // Si la misión traía recursos precargados, persistir asignaciones
                if (mision.Recursos != null && mision.Recursos.Count > 0)
                {
                    foreach (var rec in mision.Recursos)
                    {
                        var efRecurso = db.recurso_exploracion.Find(rec.Id);
                        if (efRecurso != null)
                        {
                            db.asignacion_recurso.Add(new asignacion_recurso
                            {
                                mision_id = efMision.id,
                                recurso_id = rec.Id,
                                fecha_asignacion = DateTime.Now,
                                fecha_liberacion = null,
                                activa = true
                            });

                            efRecurso.estado = (byte)EstadoRecurso.Asignado;
                        }
                    }
                    db.SaveChanges();
                }
            }
        }

        public void Actualizar(Mision mision)
        {
            if (mision == null)
                throw new ArgumentNullException(nameof(mision));

            using (var db = new orbita_controlEntities())
            {
                var efMision = db.mision.Find(mision.Id);
                if (efMision == null)
                    throw new InvalidOperationException($"No se encontró la misión con Id {mision.Id}.");

                efMision.nombre = mision.Nombre;
                efMision.descripcion = mision.Descripcion;
                efMision.fecha_inicio = mision.FechaInicio;
                efMision.fecha_fin_estimada = mision.FechaFinEstimada;
                efMision.prioridad = (byte)mision.Prioridad;
                efMision.estado = (byte)mision.Estado;

                if (mision.Responsable != null)
                {
                    efMision.responsable_id = mision.Responsable.Id;
                }

                db.SaveChanges();
            }
        }

        public void CambiarEstado(int misionId, EstadoMision nuevoEstado)
        {
            using (var db = new orbita_controlEntities())
            {
                var efMision = db.mision
                    .Include(m => m.asignacion_recurso)
                    .FirstOrDefault(m => m.id == misionId);

                if (efMision == null)
                    throw new InvalidOperationException($"No se encontró la misión con Id {misionId}.");

                efMision.estado = (byte)nuevoEstado;

                // Si la misión finaliza o se cancela, liberar recursos en la base de datos
                if (nuevoEstado == EstadoMision.Finalizada || nuevoEstado == EstadoMision.Cancelada)
                {
                    var asignacionesActivas = efMision.asignacion_recurso.Where(a => a.activa).ToList();
                    DateTime ahora = DateTime.Now;
                    foreach (var asig in asignacionesActivas)
                    {
                        asig.activa = false;
                        asig.fecha_liberacion = ahora;

                        var rec = db.recurso_exploracion.Find(asig.recurso_id);
                        if (rec != null)
                        {
                            rec.estado = (byte)EstadoRecurso.Disponible;
                        }
                    }
                }
                else if (nuevoEstado == EstadoMision.EnEjecucion)
                {
                    // Asegurar que los recursos asignados figuren como asignados
                    var asignacionesActivas = efMision.asignacion_recurso.Where(a => a.activa).ToList();
                    foreach (var asig in asignacionesActivas)
                    {
                        var rec = db.recurso_exploracion.Find(asig.recurso_id);
                        if (rec != null)
                        {
                            rec.estado = (byte)EstadoRecurso.Asignado;
                        }
                    }
                }

                db.SaveChanges();
            }
        }

        private Mision MapearMisionConRecursos(orbita_controlEntities db, mision efM)
        {
            Usuario resp = efM.usuario != null ? UsuarioRepositorio.MapearADominio(efM.usuario) : null;

            var dominioMision = new Mision(
                efM.id,
                efM.codigo,
                efM.nombre,
                efM.descripcion,
                efM.fecha_inicio,
                efM.fecha_fin_estimada,
                (PrioridadMision)efM.prioridad,
                (EstadoMision)efM.estado,
                resp
            );

            // Cargar recursos activos de la misión
            var recursoIds = efM.asignacion_recurso
                .Where(a => a.activa)
                .Select(a => a.recurso_id)
                .ToList();

            if (recursoIds.Count > 0)
            {
                var detalles = db.vw_recurso_detalle
                    .AsNoTracking()
                    .Where(d => recursoIds.Contains(d.id))
                    .ToList();

                foreach (var det in detalles)
                {
                    var rec = RecursoRepositorio.MapearDetalleADominio(det);
                    if (rec != null)
                    {
                        dominioMision.Recursos.Add(rec);
                    }
                }
            }

            return dominioMision;
        }
    }
}
