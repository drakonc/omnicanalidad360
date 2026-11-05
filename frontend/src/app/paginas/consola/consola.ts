import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { Api } from '../../nucleo/api';
import { IndicadorConexion } from '../../nucleo/indicador-conexion';
import { Sincronizacion, Vigilancia } from '../../nucleo/sincronizacion';
import { Agente, ESTADO_AGENTE, ESTADO_LLAMADA, EstadoConsola } from '../../nucleo/modelos';

/**
 * Consola del agente (ruta /consola del PRD).
 * En este hito muestra el estado del agente, la llamada que le suena,
 * el documento que capturo el IVR y el cronometro de la conversacion.
 *
 * La pantalla no se refresca sola: le pide el estado a la plataforma cada
 * segundo a traves de Sincronizacion, que conserva el ultimo dato bueno
 * si la plataforma deja de responder.
 */
@Component({
  selector: 'app-consola',
  imports: [FormsModule, IndicadorConexion],
  templateUrl: './consola.html',
  styleUrl: './consola.scss'
})
export class Consola implements OnInit, OnDestroy {
  private readonly api = inject(Api);
  private readonly sincronizacion = inject(Sincronizacion);

  /** Cada cuanto la consola le pregunta a la plataforma si hay novedades. */
  private static readonly MILISEGUNDOS_REFRESCO = 1000;

  /**
   * La vigilancia es una senal porque cambia al elegir otro agente:
   * asi las pantallas se enteran de que ahora miran otra fuente.
   */
  private readonly vigilancia = signal<Vigilancia<EstadoConsola> | null>(null);

  protected readonly agentes = signal<Agente[]>([]);
  protected readonly idAgente = signal('AG-01');
  protected readonly error = signal<string | null>(null);

  protected readonly estado = computed(() => this.vigilancia()?.estado().datos ?? null);
  protected readonly conectado = computed(() => this.vigilancia()?.estado().conectado ?? true);
  protected readonly segundosSinDato = computed(
    () => this.vigilancia()?.estado().segundosDesdeUltimoDato ?? 0
  );
  protected readonly cargando = computed(() => this.vigilancia()?.estado().cargando ?? true);

  protected readonly estaDisponible = computed(
    () => this.estado()?.estadoAgente === ESTADO_AGENTE.disponible
  );

  protected readonly estaEnPausa = computed(
    () => this.estado()?.estadoAgente === ESTADO_AGENTE.pausa
  );

  /** La llamada esta sonando: ya tiene agente, pero nadie ha contestado. */
  protected readonly llamadaSonando = computed(
    () => this.estado()?.llamada?.estado === ESTADO_LLAMADA.enCola
  );

  protected readonly llamadaEnCurso = computed(
    () => this.estado()?.llamada?.estado === ESTADO_LLAMADA.enCurso
  );

  /**
   * Cronometro de la conversacion, desde que el agente contesto.
   * Se recalcula solo porque el estado cambia en cada vuelta del refresco.
   */
  protected readonly segundosEnLlamada = computed(() => {
    const llamada = this.estado()?.llamada;
    if (!llamada?.horaContesta) {
      return 0;
    }
    return this.segundosDesde(llamada.horaContesta);
  });

  /** Cuanto lleva el paciente esperando, antes de que le contesten. */
  protected readonly segundosEnEspera = computed(() => {
    const llamada = this.estado()?.llamada;
    if (!llamada || llamada.horaContesta) {
      return 0;
    }
    return this.segundosDesde(llamada.horaEntrada);
  });

  ngOnInit(): void {
    this.api.agentes().subscribe({
      next: (lista) => this.agentes.set(lista),
      error: () => this.error.set(
        'No se pudo conectar con la plataforma. Revisa que el backend esté corriendo.'
      )
    });

    this.vigilar();
  }

  ngOnDestroy(): void {
    this.vigilancia()?.detener();
  }

  protected cambiarAgente(id: string): void {
    this.idAgente.set(id);
    this.vigilar();
  }

  protected ponerse(estado: string): void {
    this.ejecutar(
      this.api.cambiarEstado(this.idAgente(), estado),
      'No se pudo cambiar el estado.'
    );
  }

  protected contestar(): void {
    const llamada = this.estado()?.llamada;
    if (!llamada) {
      return;
    }

    this.ejecutar(
      this.api.contestar(this.idAgente(), llamada.idLlamada),
      'Esa llamada ya no está disponible.'
    );
  }

  protected colgar(): void {
    const llamada = this.estado()?.llamada;
    if (!llamada) {
      return;
    }

    this.ejecutar(
      this.api.colgar(this.idAgente(), llamada.idLlamada),
      'No se pudo colgar la llamada.'
    );
  }

  protected textoEstadoAgente(estado: string | undefined): string {
    switch (estado) {
      case ESTADO_AGENTE.disponible:
        return 'Disponible';
      case ESTADO_AGENTE.enLlamada:
        return 'En llamada';
      case ESTADO_AGENTE.pausa:
        return 'En pausa';
      default:
        return 'Desconectado';
    }
  }

  protected comoReloj(segundos: number): string {
    const minutos = Math.floor(segundos / 60);
    const resto = segundos % 60;
    return `${String(minutos).padStart(2, '0')}:${String(resto).padStart(2, '0')}`;
  }

  /** Arranca la vigilancia del agente elegido y descarta la anterior. */
  private vigilar(): void {
    this.vigilancia()?.detener();
    this.vigilancia.set(this.sincronizacion.vigilar(
      () => this.api.consola(this.idAgente()),
      Consola.MILISEGUNDOS_REFRESCO
    ));
  }

  /** Ejecuta una accion del agente y pide el estado nuevo de inmediato. */
  private ejecutar(accion: Observable<unknown>, mensajeError: string): void {
    accion.subscribe({
      next: () => {
        this.error.set(null);
        this.vigilancia()?.refrescarYa();
      },
      error: () => this.error.set(mensajeError)
    });
  }

  private segundosDesde(instante: string): number {
    const inicio = new Date(instante).getTime();
    return Math.max(0, Math.floor((Date.now() - inicio) / 1000));
  }
}
