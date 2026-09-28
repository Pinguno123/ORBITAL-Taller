using ORBITAL.Datos.Context;
using ORBITAL.Dominio.Entidades;
using ORBITAL.Dominio.Enumeraciones;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace ORBITAL.Datos.Repositorios
{
    public class RecursoRepositorio
    {
        public List<RecursoExploracion> ObtenerTodos()
        {
            using (var db = new orbita_controlEntities())
            {
                var detalles = db.vw_recurso_detalle.AsNoTracking().ToList();
                return detalles.Select(MapearDetalleADominio).Where(r => r != null).ToList();
            }
        }

        public List<RecursoExploracion> ObtenerDisponibles()
        {
            using (var db = new orbita_controlEntities())
            {
                byte estadoDisponible = (byte)EstadoRecurso.Disponible;
                var detalles = db.vw_recurso_detalle
                    .AsNoTracking()
                    .Where(r => r.estado == estadoDisponible)
                    .ToList();

                return detalles.Select(MapearDetalleADominio).Where(r => r != null).ToList();
            }
        }

        public RecursoExploracion ObtenerPorId(int id)
        {
            using (var db = new orbita_controlEntities())
            {
                var detalle = db.vw_recurso_detalle.AsNoTracking().FirstOrDefault(r => r.id == id);
                return detalle != null ? MapearDetalleADominio(detalle) : null;
            }
        }

        public RecursoExploracion ObtenerPorCodigo(string codigo)
        {
            using (var db = new orbita_controlEntities())
            {
                var detalle = db.vw_recurso_detalle.AsNoTracking().FirstOrDefault(r => r.codigo == codigo);
                return detalle != null ? MapearDetalleADominio(detalle) : null;
            }
        }

        public void CambiarEstado(int recursoId, EstadoRecurso nuevoEstado)
        {
            using (var db = new orbita_controlEntities())
            {
                var efRecurso = db.recurso_exploracion.Find(recursoId);
                if (efRecurso != null)
                {
                    efRecurso.estado = (byte)nuevoEstado;
                    db.SaveChanges();
                }
            }
        }

        public void Registrar(RecursoExploracion recurso)
        {
            if (recurso == null)
                throw new ArgumentNullException(nameof(recurso));

            if (string.IsNullOrWhiteSpace(recurso.Codigo))
                throw new ArgumentException("El código del recurso es obligatorio.");

            if (string.IsNullOrWhiteSpace(recurso.Modelo))
                throw new ArgumentException("El modelo del recurso es obligatorio.");

            if (recurso is Dron d && (d.AutonomiaVuelo < 0 || d.Alcance < 0 || d.CostoPorHora < 0))
                throw new ArgumentException("Los valores numéricos del dron no pueden ser negativos.");

            if (recurso is RoverTerrestre rov && (rov.Autonomia < 0 || rov.CapacidadCarga < 0 || rov.CostoPorKilometro < 0))
                throw new ArgumentException("Los valores numéricos del rover no pueden ser negativos.");

            if (recurso is EstacionSensores es && (es.CantidadSensores < 0 || es.ConsumoEnergetico < 0 || es.CostoDiario < 0))
                throw new ArgumentException("Los valores numéricos de la estación no pueden ser negativos.");

            using (var db = new orbita_controlEntities())
            {
                if (db.recurso_exploracion.Any(r => r.codigo == recurso.Codigo))
                {
                    throw new InvalidOperationException($"Ya existe un recurso con el código '{recurso.Codigo}'.");
                }

                string tipo = DeterminarTipoRecurso(recurso);

                var efBase = new recurso_exploracion
                {
                    codigo = recurso.Codigo,
                    modelo = recurso.Modelo,
                    tipo = tipo,
                    estado = (byte)recurso.Estado
                };

                db.recurso_exploracion.Add(efBase);
                db.SaveChanges();

                recurso.Id = efBase.id;

                if (recurso is Dron dron)
                {
                    db.dron.Add(new dron
                    {
                        recurso_id = efBase.id,
                        autonomia_vuelo = dron.AutonomiaVuelo,
                        alcance = dron.Alcance,
                        costo_por_hora = dron.CostoPorHora
                    });
                }
                else if (recurso is RoverTerrestre rover)
                {
                    db.rover_terrestre.Add(new rover_terrestre
                    {
                        recurso_id = efBase.id,
                        autonomia = rover.Autonomia,
                        capacidad_carga = rover.CapacidadCarga,
                        costo_por_kilometro = rover.CostoPorKilometro
                    });
                }
                else if (recurso is EstacionSensores estacion)
                {
                    db.estacion_sensores.Add(new estacion_sensores
                    {
                        recurso_id = efBase.id,
                        cantidad_sensores = estacion.CantidadSensores,
                        consumo_energetico = estacion.ConsumoEnergetico,
                        costo_diario = estacion.CostoDiario
                    });
                }

                db.SaveChanges();
            }
        }

        public void Modificar(RecursoExploracion recurso)
        {
            if (recurso == null)
                throw new ArgumentNullException(nameof(recurso));

            using (var db = new orbita_controlEntities())
            {
                var efBase = db.recurso_exploracion.Find(recurso.Id);
                if (efBase == null)
                    throw new InvalidOperationException($"No se encontró el recurso con Id {recurso.Id}.");

                efBase.modelo = recurso.Modelo;
                efBase.estado = (byte)recurso.Estado;

                if (recurso is Dron dron)
                {
                    var efDron = db.dron.Find(recurso.Id);
                    if (efDron != null)
                    {
                        efDron.autonomia_vuelo = dron.AutonomiaVuelo;
                        efDron.alcance = dron.Alcance;
                        efDron.costo_por_hora = dron.CostoPorHora;
                    }
                }
                else if (recurso is RoverTerrestre rover)
                {
                    var efRover = db.rover_terrestre.Find(recurso.Id);
                    if (efRover != null)
                    {
                        efRover.autonomia = rover.Autonomia;
                        efRover.capacidad_carga = rover.CapacidadCarga;
                        efRover.costo_por_kilometro = rover.CostoPorKilometro;
                    }
                }
                else if (recurso is EstacionSensores estacion)
                {
                    var efEstacion = db.estacion_sensores.Find(recurso.Id);
                    if (efEstacion != null)
                    {
                        efEstacion.cantidad_sensores = estacion.CantidadSensores;
                        efEstacion.consumo_energetico = estacion.ConsumoEnergetico;
                        efEstacion.costo_diario = estacion.CostoDiario;
                    }
                }

                db.SaveChanges();
            }
        }

        private static string DeterminarTipoRecurso(RecursoExploracion recurso)
        {
            if (recurso is Dron) return "Dron";
            if (recurso is RoverTerrestre) return "RoverTerrestre";
            if (recurso is EstacionSensores) return "EstacionSensores";
            throw new NotSupportedException($"Tipo de recurso no soportado: {recurso.GetType().Name}");
        }

        public static RecursoExploracion MapearDetalleADominio(vw_recurso_detalle detalle)
        {
            if (detalle == null)
                return null;

            EstadoRecurso estado = (EstadoRecurso)detalle.estado;

            switch (detalle.tipo)
            {
                case "Dron":
                    var dron = new Dron(
                        detalle.codigo,
                        detalle.modelo,
                        detalle.autonomia_vuelo ?? 0,
                        detalle.alcance ?? 0,
                        detalle.costo_por_hora ?? 0
                    );
                    dron.Id = detalle.id;
                    dron.Estado = estado;
                    return dron;

                case "RoverTerrestre":
                    var rover = new RoverTerrestre(
                        detalle.codigo,
                        detalle.modelo,
                        detalle.autonomia_rover ?? 0,
                        detalle.capacidad_carga ?? 0,
                        detalle.costo_por_kilometro ?? 0
                    );
                    rover.Id = detalle.id;
                    rover.Estado = estado;
                    return rover;

                case "EstacionSensores":
                    var estacion = new EstacionSensores(
                        detalle.codigo,
                        detalle.modelo,
                        detalle.cantidad_sensores ?? 0,
                        detalle.consumo_energetico ?? 0,
                        detalle.costo_diario ?? 0
                    );
                    estacion.Id = detalle.id;
                    estacion.Estado = estado;
                    return estacion;

                default:
                    return null;
            }
        }
    }
}
