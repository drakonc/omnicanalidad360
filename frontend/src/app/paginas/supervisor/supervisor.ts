import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { Api } from '../../nucleo/api';
import { IndicadorConexion } from '../../nucleo/indicador-conexion';
import { Sincronizacion, Vigilancia } from '../../nucleo/sincronizacion';
import { Franja } from '../../diseno/franja';
import { DatoFranja } from '../../diseno/dato-franja';
import { ESTADO_AGENTE, Tablero } from '../../nucleo/modelos';
import { COLAS_DEMOSTRACION, GESTIONES_DEMOSTRACION } from '../../nucleo/datos-demostracion';

/**
 * Tablero en tiempo real del supervisor (hito 4 del PRD, capacidad C7).
 *
 * Los agentes, la cola, las atendidas, las abandonadas, la espera mas larga
 * y el TMO son DATOS REALES de la plataforma. Las colas adicionales y las
 * gestiones de dias anteriores son datos de prueba inventados, porque esos
 * hitos todavia no existen.
 *
 * El nivel de servicio no se muestra: su umbral esta por validar con gerencia
 * y el PRD no lo deja fijado.
 */
@Component({
  selector: 'app-supervisor',
  imports: [IndicadorConexion, Franja, DatoFranja],
  templateUrl: './supervisor.html',
  styleUrl: './supervisor.scss'
})
export class Supervisor implements OnInit, OnDestroy {
  private readonly api = inject(Api);
  private readonly sincronizacion = inject(Sincronizacion);

  /** El PRD pide que la vista de supervisor se actualice cada 5 segundos. */
  private static readonly MILISEGUNDOS_REFRESCO = 5000;

  private readonly vigilancia = signal<Vigilancia<Tablero> | null>(null);

  protected readonly colasDemostracion = COLAS_DEMOSTRACION;
  protected readonly gestionesDemostracion = GESTIONES_DEMOSTRACION;

  protected readonly tablero = computed(() => this.vigilancia()?.estado().datos ?? null);
  protected readonly conectado = computed(() => this.vigilancia()?.estado().conectado ?? true);
  protected readonly segundosSinDato = computed(
    () => this.vigilancia()?.estado().segundosDesdeUltimoDato ?? 0
  );

  protected readonly agentes = computed(() => this.tablero()?.agentes ?? []);

  /** Cuantos agentes hay en cada estado, para la barra apilada del diseno. */
  protected readonly reparto = computed(() => {
    const lista = this.agentes();
    const total = lista.length || 1;

    const contar = (estado: string) =>
      lista.filter((a) => a.estadoAgente === estado).length;

    const enLlamada = contar(ESTADO_AGENTE.enLlamada);
    const disponibles = contar(ESTADO_AGENTE.disponible);
    const enPausa = contar(ESTADO_AGENTE.pausa);
    const desconectados = contar(ESTADO_AGENTE.desconectado);

    return {
      total: lista.length,
      enLlamada,
      disponibles,
      enPausa,
      desconectados,
      porcentajes: {
        enLlamada: (enLlamada / total) * 100,
        disponibles: (disponibles / total) * 100,
        enPausa: (enPausa / total) * 100,
        desconectados: (desconectados / total) * 100
      }
    };
  });

  /** Porcentaje de agentes ocupados en una llamada. */
  protected readonly ocupacion = computed(() => {
    const r = this.reparto();
    return r.total === 0 ? 0 : Math.round((r.enLlamada / r.total) * 100);
  });

  protected readonly hayMovimientoHoy = computed(() => {
    const t = this.tablero();
    if (!t) {
      return false;
    }
    return t.atendidasHoy > 0 || t.abandonadasHoy > 0 || t.llamadasEnCola > 0;
  });

  ngOnInit(): void {
    this.vigilancia.set(this.sincronizacion.vigilar(
      () => this.api.tablero(),
      Supervisor.MILISEGUNDOS_REFRESCO
    ));
  }

  ngOnDestroy(): void {
    this.vigilancia()?.detener();
  }

  protected textoEstadoAgente(estado: string): string {
    switch (estado) {
      case ESTADO_AGENTE.disponible: return 'Disponible';
      case ESTADO_AGENTE.enLlamada: return 'En llamada';
      case ESTADO_AGENTE.pausa: return 'En pausa';
      default: return 'Desconectado';
    }
  }

  protected segundosEnEstado(desde: string | null): number {
    if (!desde) {
      return 0;
    }
    return Math.max(0, Math.floor((Date.now() - new Date(desde).getTime()) / 1000));
  }

  protected comoReloj(segundos: number): string {
    const minutos = Math.floor(segundos / 60);
    const resto = segundos % 60;
    return `${String(minutos).padStart(2, '0')}:${String(resto).padStart(2, '0')}`;
  }
}
