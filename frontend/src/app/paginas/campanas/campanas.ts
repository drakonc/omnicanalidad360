import { Component, computed, signal } from '@angular/core';
import { Franja } from '../../diseno/franja';
import { DatoFranja } from '../../diseno/dato-franja';
import { CAMPANA_DEMOSTRACION } from '../../nucleo/datos-demostracion';

/**
 * Campana saliente en modo vista previa.
 *
 * MAQUETA COMPLETA, con datos inventados. Esta pantalla existe porque esta
 * en el diseno, pero OJO: el PRD excluye expresamente las llamadas salientes
 * del piloto ("Llamadas salientes: tienen otro flujo, con HIS y Saludtools").
 * No hay nada construido detras y ningun boton llama a nadie.
 */
@Component({
  selector: 'app-campanas',
  imports: [Franja, DatoFranja],
  templateUrl: './campanas.html',
  styleUrl: './campanas.scss'
})
export class Campanas {
  protected readonly campana = CAMPANA_DEMOSTRACION;

  protected readonly avance = computed(() =>
    Math.round((this.campana.gestionados / this.campana.totalContactos) * 100)
  );

  protected readonly marcando = signal(false);

  protected alternarMarcacion(): void {
    this.marcando.update((valor) => !valor);
  }
}
