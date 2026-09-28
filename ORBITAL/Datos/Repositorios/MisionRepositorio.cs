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
                    .ToList()
                    .GroupBy(m => m.id)
                    .Select(g => g.First())
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
                    .ToList()
                    .GroupBy(m => m.id)
                    .Select(g => g.First())
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

            if (string.IsNullOrWhiteSpace(mision.Nombre))
                throw new MisionInvalidaException("El nombre de la misión es obligatorio.");

            if (string.IsNullOrWhiteSpace(mision.Descripcion))
                throw new MisionInvalidaException("La descripción de la misión es obligatoria.");

            if (mision.FechaFinEstimada < mision.FechaInicio)
                throw new MisionInvalidaException("La fecha estimada de finalización no puede ser anterior a la fecha de inicio.");

            using (var db = new orbita_controlEntities())
            {
                var efMision = db.mision.Find(mision.Id);
                if (efMision == null)
                    throw new InvalidOperationException($"No se encontró la misión con Id {mision.Id}.");

                if (efMision.estado == (byte)EstadoMision.Finalizada || efMision.estado == (byte)EstadoMision.Cancelada)
                {
                    throw new InvalidOperationException($"No se pueden modificar misiones en estado {(EstadoMision)efMision.estado}.");
                }

                efMision.nombre = mision.Nombre;
                efMision.descripcion = mision.Descripcion;
                efMision.fecha_inicio = mision.FechaInicio;
                efMision.fecha_fin_estimada = mision.FechaFinEstimada;
                efMision.prioridad = (byte)mision.Prioridad;

                if (mision.Responsable != null && mision.Responsable.Id > 0)
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
                    .Include(m => m.usuario)
                    .Include(m => m.asignacion_recurso)
                    .FirstOrDefault(m => m.id == misionId);

                if (efMision == null)
                    throw new InvalidOperationException($"No se encontró la misión con Id {misionId}.");

                if (nuevoEstado == EstadoMision.EnEjecucion)
                {
                    // Validar protocolo de seguridad
                    var dominioMision = MapearMisionConRecursos(db, efMision);
                    dominioMision.ValidarProtocoloSeguridad();

                    // Regla de seguridad: Ningún recurso asignado puede estar en otra misión activa / en ejecución
                    var recursoIds = efMision.asignacion_recurso
                        .Where(a => a.activa)
                        .Select(a => a.recurso_id)
                        .ToList();

                    var conflicto = db.asignacion_recurso
                        .Include(a => a.mision)
                        .Include(a => a.recurso_exploracion)
                        .FirstOrDefault(a => a.mision_id != misionId &&
                                             a.activa &&
                                             recursoIds.Contains(a.recurso_id) &&
                                             a.mision.estado == (byte)EstadoMision.EnEjecucion);

                    if (conflicto != null)
                    {
                        throw new InvalidOperationException($"Protocolo de seguridad ORBITA: El recurso '{conflicto.recurso_exploracion.codigo}' ya se encuentra asignado a otra misión en ejecución ('{conflicto.mision.codigo}').");
                    }

                    // Asegurar que los recursos asignados figuren como asignados
                    foreach (var asig in efMision.asignacion_recurso.Where(a => a.activa))
                    {
                        var rec = db.recurso_exploracion.Find(asig.recurso_id);
                        if (rec != null)
                        {
                            rec.estado = (byte)EstadoRecurso.Asignado;
                        }
                    }
                }
                else if (nuevoEstado == EstadoMision.Finalizada)
                {
                    // Restricción: Únicamente misiones EnEjecucion pueden finalizarse
                    if (efMision.estado != (byte)EstadoMision.EnEjecucion)
                    {
                        throw new MisionInvalidaException($"Únicamente las misiones en estado 'EnEjecucion' pueden ser finalizadas. Estado actual de '{efMision.codigo}': {(EstadoMision)efMision.estado}.");
                    }

                    // Liberar recursos
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
                else if (nuevoEstado == EstadoMision.Cancelada)
                {
                    if (efMision.estado == (byte)EstadoMision.Finalizada)
                    {
                        throw new MisionInvalidaException("No se puede cancelar una misión que ya ha finalizado.");
                    }

                    // Liberar recursos
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

                efMision.estado = (byte)nuevoEstado;
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

            // Cargar recursos y asignaciones activos de la misión
            var asignacionesActivas = efM.asignacion_recurso
                .Where(a => a.activa)
                .ToList();

            if (asignacionesActivas.Count > 0)
            {
                var recursoIds = asignacionesActivas.Select(a => a.recurso_id).ToList();
                var detalles = db.vw_recurso_detalle
                    .AsNoTracking()
                    .Where(d => recursoIds.Contains(d.id))
                    .ToList();

                decimal duracionDias = (decimal)Math.Max(1, Math.Ceiling((efM.fecha_fin_estimada - efM.fecha_inicio).TotalDays));

                foreach (var asig in asignacionesActivas)
                {
                    var det = detalles.FirstOrDefault(d => d.id == asig.recurso_id);
                    if (det != null)
                    {
                        var rec = RecursoRepositorio.MapearDetalleADominio(det);
                        if (rec != null)
                        {
                            if (!dominioMision.Recursos.Any(r => r.Id == rec.Id))
                            {
                                dominioMision.Recursos.Add(rec);
                            }
                            dominioMision.Asignaciones.Add(new AsignacionRecurso(
                                asig.id,
                                dominioMision,
                                rec,
                                asig.fecha_asignacion,
                                asig.fecha_liberacion,
                                asig.activa,
                                duracionDias
                            ));
                        }
                    }
                }
            }

            return dominioMision;
        }
    }
}
