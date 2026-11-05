import { Injectable, Signal, computed, signal } from '@angular/core';
import { Observable, Subscription } from 'rxjs';

/**
 * Lo que una pantalla sabe en todo momento sobre su propio dato.
 */
export interface EstadoSincronizado<T> {
  /** El ultimo dato bueno que se recibio. Se conserva aunque la conexion falle. */
  datos: T | null;

  /** True mientras la plataforma responde. */
  conectado: boolean;

  /** Segundos transcurridos desde el ultimo dato bueno. */
  segundosDesdeUltimoDato: number;

  /** True solo hasta que llega el primer dato. */
  cargando: boolean;
}

/**
 * Mantiene una pantalla al dia preguntandole a la plataforma cada cierto tiempo.
 *
 * Por que preguntar en vez de esperar un aviso:
 * el analisis de uContact (docs/decisions/0001-tiempo-real-polling.md) mostro
 * que su canal de avisos en vivo llevaba meses roto en produccion y nadie lo
 * noto, porque el sistema seguia funcionando preguntando cada cierto tiempo.
 * Preguntar es la base confiable; el aviso en vivo se agrega despues encima,
 * como mejora, sin cambiar las pantallas.
 *
 * Cuando algo falla, la pantalla NO se queda en blanco: conserva el ultimo
 * dato bueno y avisa hace cuanto es.
 */
@Injectable({ providedIn: 'root' })
export class Sincronizacion {
  /**
   * Empieza a vigilar una consulta y devuelve su estado siempre actualizado.
   * Hay que llamar a detener() cuando la pantalla se cierra.
   */
  vigilar<T>(consulta: () => Observable<T>, cadaMilisegundos: number): Vigilancia<T> {
    return new Vigilancia<T>(consulta, cadaMilisegundos);
  }
}

export class Vigilancia<T> {
  private readonly datos = signal<T | null>(null);
  private readonly conectado = signal(true);
  private readonly cargando = signal(true);
  private readonly instanteUltimoDato = signal<number | null>(null);

  /** Se actualiza en cada vuelta para que el contador de segundos avance. */
  private readonly ahora = signal(Date.now());

  private temporizador?: ReturnType<typeof setInterval>;
  private enCurso?: Subscription;

  readonly estado: Signal<EstadoSincronizado<T>> = computed(() => ({
    datos: this.datos(),
    conectado: this.conectado(),
    cargando: this.cargando(),
    segundosDesdeUltimoDato: this.calcularSegundos()
  }));

  constructor(
    private readonly consulta: () => Observable<T>,
    private readonly cadaMilisegundos: number
  ) {
    this.preguntar();
    this.temporizador = setInterval(() => this.preguntar(), cadaMilisegundos);
  }

  /** Pide el dato de inmediato, sin esperar la siguiente vuelta. */
  refrescarYa(): void {
    this.preguntar();
  }

  detener(): void {
    if (this.temporizador) {
      clearInterval(this.temporizador);
      this.temporizador = undefined;
    }
    this.enCurso?.unsubscribe();
  }

  private preguntar(): void {
    this.ahora.set(Date.now());

    // Si la vuelta anterior sigue en el aire, no se encima otra.
    if (this.enCurso && !this.enCurso.closed) {
      return;
    }

    this.enCurso = this.consulta().subscribe({
      next: (recibido) => {
        this.datos.set(recibido);
        this.instanteUltimoDato.set(Date.now());
        this.conectado.set(true);
        this.cargando.set(false);
      },
      error: () => {
        // Degradar con gracia: el ultimo dato bueno se queda en pantalla.
        this.conectado.set(false);
        this.cargando.set(false);
      }
    });
  }

  private calcularSegundos(): number {
    const ultimo = this.instanteUltimoDato();
    if (ultimo === null) {
      return 0;
    }
    return Math.max(0, Math.floor((this.ahora() - ultimo) / 1000));
  }
}
