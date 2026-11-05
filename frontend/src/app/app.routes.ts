import { Routes } from '@angular/router';
import { Consola } from './paginas/consola/consola';
import { Simulador } from './paginas/simulador/simulador';

/**
 * Rutas del PRD. En este hito estan la consola del agente
 * y el panel que simula la llamada entrante.
 */
export const routes: Routes = [
  { path: '', redirectTo: 'consola', pathMatch: 'full' },
  { path: 'consola', component: Consola, title: 'Consola del agente' },
  { path: 'simulador', component: Simulador, title: 'Teléfono del paciente' },
  { path: '**', redirectTo: 'consola' }
];
