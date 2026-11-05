/**
 * Menus de la barra lateral, segun el diseno EverCall.
 * Cada rol ve su propio menu.
 */

export interface OpcionMenu {
  ruta: string;
  etiqueta: string;
  icono: string;
  /** True si la pantalla todavia no existe en el piloto. */
  pendiente?: boolean;
}

export const MENU_AGENTE: OpcionMenu[] = [
  { ruta: '/consola', etiqueta: 'Mi escritorio', icono: 'desktop_windows' },
  { ruta: '/tipificacion', etiqueta: 'Tipificación', icono: 'fact_check' },
  { ruta: '/campanas', etiqueta: 'Campañas', icono: 'campaign' },
  { ruta: '/simulador', etiqueta: 'Teléfono de prueba', icono: 'dialpad' }
];

export const MENU_SUPERVISOR: OpcionMenu[] = [
  { ruta: '/supervisor', etiqueta: 'Tablero en vivo', icono: 'dashboard' },
  { ruta: '/ivr', etiqueta: 'Constructor de IVR', icono: 'account_tree' }
];
