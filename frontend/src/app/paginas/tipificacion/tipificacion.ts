import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Api } from '../../nucleo/api';
import { Franja } from '../../diseno/franja';
import { DatoFranja } from '../../diseno/dato-franja';
import { CausalDemostracion, GESTION_DEMOSTRACION } from '../../nucleo/datos-demostracion';

/**
 * Tipificacion post-llamada (hito 3 del PRD, capacidad C5).
 *
 * MAQUETA: el resumen de la llamada son datos de prueba inventados. Lo unico
 * real es el catalogo de causales, que viene del catalogo sembrado en la
 * plataforma. Cuando se construya el hito 3, el resumen vendra de la gestion
 * en borrador que deja la llamada que se acaba de terminar.
 */
@Component({
  selector: 'app-tipificacion',
  imports: [FormsModule, Franja, DatoFranja],
  templateUrl: './tipificacion.html',
  styleUrl: './tipificacion.scss'
})
export class Tipificacion implements OnInit, OnDestroy {
  private readonly api = inject(Api);

  /** Limite para cerrar la gestion, como lo plantea el diseno. */
  private static readonly SEGUNDOS_LIMITE = 60;

  private temporizador?: ReturnType<typeof setInterval>;

  protected readonly gestion = GESTION_DEMOSTRACION;

  protected readonly catalogo = signal<CausalDemostracion[]>([]);
  protected readonly nivel1Elegido = signal<string | null>(null);
  protected readonly nivel2Elegido = signal<string | null>(null);
  protected readonly observaciones = signal('');
  protected readonly guardada = signal(false);
  protected readonly intentoSinCausal = signal(false);
  protected readonly segundosTranscurridos = signal(0);

  /** Niveles 1 distintos, en el orden del catalogo. */
  protected readonly nivelesUno = computed(() => {
    const vistos = new Set<string>();
    return this.catalogo()
      .map((c) => c.nivel1)
      .filter((n) => (vistos.has(n) ? false : vistos.add(n)));
  });

  /** Niveles 2 disponibles para el Nivel 1 elegido. */
  protected readonly nivelesDos = computed(() => {
    const n1 = this.nivel1Elegido();
    if (!n1) {
      return [];
    }
    return this.catalogo().filter((c) => c.nivel1 === n1 && c.nivel2 !== null);
  });

  /** Si la causal elegida exige cita, la gestion no cierra sin ella (RN-5). */
  protected readonly exigeCita = computed(() => {
    const n1 = this.nivel1Elegido();
    const n2 = this.nivel2Elegido();
    return this.catalogo().some(
      (c) => c.nivel1 === n1 && c.nivel2 === n2 && c.exigeCita
    );
  });

  protected readonly segundosRestantes = computed(() =>
    Math.max(0, Tipificacion.SEGUNDOS_LIMITE - this.segundosTranscurridos())
  );

  protected readonly porcentajeRestante = computed(
    () => (this.segundosRestantes() / Tipificacion.SEGUNDOS_LIMITE) * 100
  );

  /** Sin Nivel 1 no se puede cerrar la gestion (RN-5). */
  protected readonly puedeGuardar = computed(() => this.nivel1Elegido() !== null);

  ngOnInit(): void {
    this.api.tipificaciones().subscribe({
      next: (lista) => this.catalogo.set(lista),
      error: () => this.catalogo.set([])
    });

    this.temporizador = setInterval(
      () => this.segundosTranscurridos.update((s) => s + 1),
      1000
    );
  }

  ngOnDestroy(): void {
    if (this.temporizador) {
      clearInterval(this.temporizador);
    }
  }

  protected elegirNivel1(nivel1: string): void {
    this.nivel1Elegido.set(nivel1);
    this.nivel2Elegido.set(null);
    this.intentoSinCausal.set(false);
    this.guardada.set(false);
  }

  protected elegirNivel2(nivel2: string): void {
    this.nivel2Elegido.set(nivel2);
    this.guardada.set(false);
  }

  protected guardar(): void {
    if (!this.puedeGuardar()) {
      this.intentoSinCausal.set(true);
      return;
    }
    this.guardada.set(true);
  }

  protected iconoDe(nivel1: string): string {
    switch (nivel1) {
      case 'ASIGNACION': return 'event_available';
      case 'INFORMACION': return 'info';
      case 'ESCALAMIENTO - NO CITA': return 'report_problem';
      case 'CANCELACION': return 'event_busy';
      case 'CONFIRMACION': return 'task_alt';
      case 'PQR': return 'feedback';
      case 'NO CONTACTO': return 'phone_missed';
      default: return 'more_horiz';
    }
  }

  protected comoReloj(segundos: number): string {
    const minutos = Math.floor(segundos / 60);
    const resto = segundos % 60;
    return `${String(minutos).padStart(2, '0')}:${String(resto).padStart(2, '0')}`;
  }
}
