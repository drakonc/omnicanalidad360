/**
 * DATOS DE PRUEBA, TODOS INVENTADOS.
 *
 * Alimentan las pantallas de hitos que todavia no estan construidos, para
 * poder recorrer el diseno completo. Ninguno sale de la plataforma ni de un
 * sistema real, y ninguno corresponde a una persona real (RN-12 del PRD).
 *
 * Cuando cada hito se construya, su pantalla deja de leer de aqui y pasa a
 * leer de la plataforma.
 */

export interface CausalDemostracion {
  nivel1: string;
  nivel2: string | null;
  exigeCita: boolean;
}

/** Hito 3 · Resumen de la llamada que se acaba de terminar. */
export const GESTION_DEMOSTRACION = {
  paciente: 'María Fernanda Ruiz Pérez',
  tipoDocumento: 'CC',
  numeroDocumento: '1000000001',
  telefono: '+573001234567',
  eps: 'Nueva EPS',
  sede: 'Sede Norte',
  cola: 'NEPS Imagenología',
  duracionLlamada: '06:31',
  horarioSesion: '10:14 a. m. - 10:20 a. m.',
  idGrabacion: 'rec-7f3c2a10.wav',
  notas:
    'Paciente solicita cita de ecografía abdominal total en la sede Norte. ' +
    'Confirmó disponibilidad en la mañana.',
  cita: {
    idCita: 'LUM-884213',
    examen: 'Ecografía abdominal total',
    sede: 'Sede Norte',
    fechaHora: '27 nov 2026 · 7:40 a. m.'
  }
};

/** Hito 4 · Colas de atencion. En el piloto solo existe una linea. */
export const COLAS_DEMOSTRACION = [
  { nombre: 'NEPS Imagenología', agentes: 5, enEspera: 0, nivelServicio: 82, abandono: 3.1 },
  { nombre: 'Imagenología especialistas', agentes: 0, enEspera: 0, nivelServicio: 0, abandono: 0 },
  { nombre: 'Autorizaciones', agentes: 0, enEspera: 0, nivelServicio: 0, abandono: 0 }
];

/** Hito 4 · Gestiones registradas en dias anteriores. */
export const GESTIONES_DEMOSTRACION = [
  {
    fecha: '04 nov 2026 10:14',
    agente: 'AG-01',
    documento: '1000000003',
    nombre: 'Luz Dary Moreno Castaño',
    nivel1: 'ASIGNACION',
    nivel2: 'CITA ASIGNADA',
    idCita: 'LUM-884190',
    tmo: '04:12'
  },
  {
    fecha: '04 nov 2026 10:41',
    agente: 'AG-02',
    documento: '1000000027',
    nombre: 'Nubia Stella Carvajal Durán',
    nivel1: 'ESCALAMIENTO - NO CITA',
    nivel2: 'DERECHOS NO VIGENTES',
    idCita: null,
    tmo: '02:48'
  },
  {
    fecha: '04 nov 2026 11:03',
    agente: 'AG-01',
    documento: '1000000009',
    nombre: 'Sandra Milena Ospina Tobón',
    nivel1: 'INFORMACION',
    nivel2: 'REQUISITOS DEL EXAMEN',
    idCita: null,
    tmo: '03:05'
  },
  {
    fecha: '04 nov 2026 11:37',
    agente: 'AG-03',
    documento: '1000000014',
    nombre: 'Mauricio Alejandro Benítez Correa',
    nivel1: 'ASIGNACION',
    nivel2: 'CITA ASIGNADA',
    idCita: 'LUM-884201',
    tmo: '05:50'
  }
];

/**
 * Campana saliente.
 * ATENCION: el PRD excluye expresamente las llamadas salientes del piloto
 * ("Llamadas salientes - tienen otro flujo, con HIS y Saludtools").
 * Esta pantalla existe porque esta en el diseno, y es solo una maqueta.
 */
export const CAMPANA_DEMOSTRACION = {
  nombre: 'Confirmación de citas · Imagenología',
  totalContactos: 120,
  gestionados: 47,
  pendientes: 73,
  contactoActual: {
    nombre: 'Jorge Enrique Castillo Parra',
    documento: '1000000008',
    telefono: '+573104567890',
    eps: 'Aliansalud EPS',
    cita: 'Radiografía de tórax · 12 nov 2026 · 9:00 a. m. · Sede Sur'
  },
  guion: [
    'Buenos días, le habla [nombre] de NEPS Imagenología.',
    'Lo llamo para confirmar su cita del 12 de noviembre a las 9:00 de la mañana en la Sede Sur.',
    '¿Va a poder asistir a su cita?'
  ]
};

/** Flujo del IVR, con los mensajes que hoy reproduce la llamada simulada. */
export interface NodoIvr {
  id: string;
  tipo: 'mensaje' | 'captura' | 'cola' | 'fin';
  titulo: string;
  detalle: string;
  activo: boolean;
}

export const FLUJO_IVR_DEMOSTRACION: NodoIvr[] = [
  {
    id: 'entrada',
    tipo: 'mensaje',
    titulo: 'Entra la llamada',
    detalle: 'Línea de prueba de NEPS Imagenología',
    activo: true
  },
  {
    id: 'aviso',
    tipo: 'mensaje',
    titulo: 'Aviso de grabación',
    detalle: 'Esta llamada puede ser grabada con fines de calidad y seguridad.',
    activo: true
  },
  {
    id: 'documento',
    tipo: 'captura',
    titulo: 'Captura del documento',
    detalle: 'Marque su número de documento y la tecla numeral. 2 intentos de 10 segundos.',
    activo: true
  },
  {
    id: 'cola',
    tipo: 'cola',
    titulo: 'Cola de NEPS Imagenología',
    detalle: 'Se entrega al agente disponible que lleva más tiempo libre.',
    activo: true
  },
  {
    id: 'menu',
    tipo: 'mensaje',
    titulo: 'Menú de opciones',
    detalle: 'No existe en el piloto: la línea tiene una sola opción.',
    activo: false
  },
  {
    id: 'transferencia',
    tipo: 'fin',
    titulo: 'Transferencia a la gerencia',
    detalle: 'El PRD lo deja para la siguiente versión.',
    activo: false
  }
];
