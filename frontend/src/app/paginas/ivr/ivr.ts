import { Component, signal } from '@angular/core';
import { Franja } from '../../diseno/franja';
import { DatoFranja } from '../../diseno/dato-franja';
import { FLUJO_IVR_DEMOSTRACION, NodoIvr } from '../../nucleo/datos-demostracion';

/**
 * Constructor del flujo del IVR (pantalla /administracion del PRD).
 *
 * MAQUETA: muestra el arbol del flujo tal como esta hoy en la llamada
 * simulada. Los pasos marcados como activos SI corresponden a lo que hace
 * de verdad el simulador de telefonia; los demas son del diseno y no existen
 * en el piloto. Editar aqui no cambia nada: es solo el aspecto de la pantalla.
 */
@Component({
  selector: 'app-ivr',
  imports: [Franja, DatoFranja],
  templateUrl: './ivr.html',
  styleUrl: './ivr.scss'
})
export class Ivr {
  protected readonly flujo = FLUJO_IVR_DEMOSTRACION;
  protected readonly seleccionado = signal<NodoIvr>(FLUJO_IVR_DEMOSTRACION[0]);

  protected readonly pasosActivos = FLUJO_IVR_DEMOSTRACION.filter((n) => n.activo).length;

  protected seleccionar(nodo: NodoIvr): void {
    this.seleccionado.set(nodo);
  }

  protected iconoDe(tipo: NodoIvr['tipo']): string {
    switch (tipo) {
      case 'mensaje': return 'campaign';
      case 'captura': return 'dialpad';
      case 'cola': return 'groups';
      default: return 'flag';
    }
  }

  protected nombreTipo(tipo: NodoIvr['tipo']): string {
    switch (tipo) {
      case 'mensaje': return 'Mensaje hablado';
      case 'captura': return 'Captura por teclado';
      case 'cola': return 'Entrega a la cola';
      default: return 'Fin del flujo';
    }
  }
}
