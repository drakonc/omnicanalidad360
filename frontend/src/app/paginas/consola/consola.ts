import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Api } from '../../nucleo/api';
import { Agente, ESTADO_AGENTE, ESTADO_LLAMADA, EstadoConsola } from '../../nucleo/modelos';

/**
 * Consola del agente (ruta /consola del PRD).
 * En este hito muestra el estado del agente, la llamada que le suena,
 * el documento que capturo el IVR y el cronometro de la conversacion.
 */
@Component({
  selector: 'app-consola',
  imports: [FormsModule],
  templateUrl: './consola.html',
  styleUrl: './consola.scss'
})
export class Consola implements OnInit, OnDestroy {
  private readonly api = inject(Api);

  /** Cada cuanto la consola vuelve a preguntar si hay novedades. */
  private static readonly MILISEGUNDOS_REFRESCO = 1000;

  private temporizador?: ReturnType<typeof setInterval>;

  protected readonly agentes = signal<Agente[]>([]);
  protected readonly idAgente = signal('AG-01');
  protected readonly estado = signal<EstadoConsola | null>(null);
  protected readonly error = signal<string | null>(null);

  /** Segundos transcurridos, recalculados en cada refresco. */
  protected readonly ahora = signal(Date.now());

  protected readonly estaDisponible = computed(
    () => this.estado()?.estadoAgente === ESTADO_AGENTE.disponible
  );

  protected readonly estaEnPausa = computed(
    () => this.estado()?.estadoAgente === ESTADO_AGENTE.pausa
  );

  /** La llamada esta sonando: ya tiene agente, pero nadie ha contestado. */
  protected readonly llamadaSonando = computed(() => {
    const llamada = this.estado()?.llamada;
    return llamada?.estado === ESTADO_LLAMADA.enCola;
  });

  protected readonly llamadaEnCurso = computed(() => {
    const llamada = this.estado()?.llamada;
    return llamada?.estado === ESTADO_LLAMADA.enCurso;
  });

  /** Cronometro de la conversacion, desde que el agente contesto. */
  protected readonly segundosEnLlamada = computed(() => {
    const llamada = this.estado()?.llamada;
    if (!llamada?.horaContesta) {
      return 0;
    }
    const inicio = new Date(llamada.horaContesta).getTime();
    return Math.max(0, Math.floor((this.ahora() - inicio) / 1000));
  });

  /** Cuanto lleva el paciente esperando, antes de que le contesten. */
  protected readonly segundosEnEspera = computed(() => {
    const llamada = this.estado()?.llamada;
    if (!llamada || llamada.horaContesta) {
      return 0;
    }
    const inicio = new Date(llamada.horaEntrada).getTime();
    return Math.max(0, Math.floor((this.ahora() - inicio) / 1000));
  });

  ngOnInit(): void {
    this.api.agentes().subscribe({
      next: (lista) => this.agentes.set(lista),
      error: () => this.error.set('No se pudo conectar con la plataforma. Revisa que el backend este corriendo.')
    });

    this.refrescar();
    this.temporizador = setInterval(() => this.refrescar(), Consola.MILISEGUNDOS_REFRESCO);
  }

  ngOnDestroy(): void {
    if (this.temporizador) {
      clearInterval(this.temporizador);
    }
  }

  protected cambiarAgente(id: string): void {
    this.idAgente.set(id);
    this.refrescar();
  }

  protected ponerse(estado: string): void {
    this.api.cambiarEstado(this.idAgente(), estado).subscribe({
      next: () => this.refrescar(),
      error: () => this.error.set('No se pudo cambiar el estado.')
    });
  }

  protected contestar(): void {
    const llamada = this.estado()?.llamada;
    if (!llamada) {
      return;
    }

    this.api.contestar(this.idAgente(), llamada.idLlamada).subscribe({
      next: () => this.refrescar(),
      error: () => this.error.set('Esa llamada ya no esta disponible.')
    });
  }

  protected colgar(): void {
    const llamada = this.estado()?.llamada;
    if (!llamada) {
      return;
    }

    this.api.colgar(this.idAgente(), llamada.idLlamada).subscribe({
      next: () => this.refrescar(),
      error: () => this.error.set('No se pudo colgar la llamada.')
    });
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

  private refrescar(): void {
    this.ahora.set(Date.now());

    this.api.consola(this.idAgente()).subscribe({
      next: (estado) => {
        this.estado.set(estado);
        this.error.set(null);
      },
      error: () => this.error.set('No se pudo actualizar la consola.')
    });
  }
}
