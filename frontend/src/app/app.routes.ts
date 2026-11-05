import { Routes } from '@angular/router';
import { Consola } from './paginas/consola/consola';
import { Simulador } from './paginas/simulador/simulador';
import { Tipificacion } from './paginas/tipificacion/tipificacion';
import { Supervisor } from './paginas/supervisor/supervisor';
import { Campanas } from './paginas/campanas/campanas';
import { Ivr } from './paginas/ivr/ivr';

/**
 * Pantallas del diseno EverCall.
 * Las de hitos futuros estan construidas con datos de prueba y lo advierten
 * en la propia pantalla.
 */
export const routes: Routes = [
  { path: '', redirectTo: 'consola', pathMatch: 'full' },
  { path: 'consola', component: Consola, title: 'Mi escritorio · EverCall' },
  { path: 'tipificacion', component: Tipificacion, title: 'Tipificacion · EverCall' },
  { path: 'campanas', component: Campanas, title: 'Campanas · EverCall' },
  { path: 'simulador', component: Simulador, title: 'Telefono de prueba · EverCall' },
  { path: 'supervisor', component: Supervisor, title: 'Tablero en vivo · EverCall' },
  { path: 'ivr', component: Ivr, title: 'Constructor de IVR · EverCall' },
  { path: '**', redirectTo: 'consola' }
];
