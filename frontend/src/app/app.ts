import { Component, computed, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { filter, map, startWith } from 'rxjs';
import { inject } from '@angular/core';
import { MENU_AGENTE, MENU_SUPERVISOR } from './diseno/navegacion';

/**
 * Marco de la aplicacion: barra lateral oscura fija y area de trabajo.
 * Sigue el diseno EverCall del proyecto de Stitch.
 */
@Component({
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html'
})
export class App {
  private readonly router = inject(Router);

  protected readonly menuAgente = MENU_AGENTE;
  protected readonly menuSupervisor = MENU_SUPERVISOR;

  /** Ruta actual, para saber que perfil mostrar abajo en la barra lateral. */
  private readonly rutaActual = toSignal(
    this.router.events.pipe(
      filter((evento): evento is NavigationEnd => evento instanceof NavigationEnd),
      map((evento) => evento.urlAfterRedirects),
      startWith(this.router.url)
    ),
    { initialValue: '/' }
  );

  protected readonly esVistaSupervisor = computed(() => {
    const ruta = this.rutaActual();
    return ruta.startsWith('/supervisor') || ruta.startsWith('/ivr');
  });

  protected readonly usuario = computed(() =>
    this.esVistaSupervisor()
      ? { nombre: 'Supervisor de Prueba', detalle: 'SUP-01 · Supervisor', iniciales: 'SP' }
      : { nombre: 'Agente Prueba Uno', detalle: 'AG-01 · Ext. 1001', iniciales: 'AP' }
  );

  protected readonly menu = computed(() =>
    this.esVistaSupervisor() ? this.menuSupervisor : this.menuAgente
  );

  protected readonly anio = signal(new Date().getFullYear());
}
