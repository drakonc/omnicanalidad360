/**
 * Lo que el backend devuelve a las pantallas.
 * Los nombres siguen el glosario del PRD: llamada, gestion, agente.
 */

export interface Agente {
  idUsuario: string;
  nombre: string;
  estadoAgente: string;
}

export interface LlamadaEnConsola {
  idLlamada: string;
  telefonoOrigen: string;
  documentoIvr: string | null;
  estado: string;
  horaEntrada: string;
  horaContesta: string | null;
  avisoGrabacionReproducido: boolean;
}

export interface EstadoConsola {
  idAgente: string;
  nombre: string;
  estadoAgente: string;
  enEstadoDesde: string | null;
  llamada: LlamadaEnConsola | null;
  llamadasEnCola: number;
}

export interface DocumentoDePrueba {
  numeroDocumento: string;
  nombreCompleto: string;
  derechos: string;
}

export interface LlamadaEntrante {
  idLlamada: string;
  estado: string;
  documentoIvr: string | null;
  avisoReproducido: string;
}

/** Estados del agente, tal como los nombra el PRD. */
export const ESTADO_AGENTE = {
  disponible: 'disponible',
  enLlamada: 'en_llamada',
  pausa: 'pausa',
  desconectado: 'desconectado'
} as const;

/** Estados de la llamada, tal como los nombra el PRD. */
export const ESTADO_LLAMADA = {
  enIvr: 'en_ivr',
  enCola: 'en_cola',
  enCurso: 'en_curso',
  finalizada: 'finalizada',
  abandonada: 'abandonada'
} as const;
