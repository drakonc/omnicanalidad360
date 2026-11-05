import { Component, input } from '@angular/core';

/**
 * Dice en pantalla si lo que se esta viendo es de ahora o de hace rato.
 *
 * Nace del hallazgo del analisis de uContact: su canal en vivo estaba roto
 * en produccion y nadie lo noto, porque la pantalla nunca dijo que el dato
 * estuviera viejo. Aqui el dato viejo se ve.
 */
@Component({
  selector: 'app-indicador-conexion',
  template: `
    @if (conectado()) {
      <span class="indicador en-vivo" title="La pantalla se actualiza sola">
        <span class="punto"></span> En vivo
      </span>
    } @else {
      <span class="indicador sin-conexion">
        <span class="punto"></span>
        No se pudo actualizar; último dato hace {{ segundos() }} s
      </span>
    }
  `,
  styles: `
    .indicador {
      display: inline-flex;
      align-items: center;
      gap: 0.4rem;
      font-size: 0.78rem;
      padding: 0.15rem 0.65rem;
      border-radius: 999px;
      white-space: nowrap;
    }

    .punto {
      width: 7px;
      height: 7px;
      border-radius: 50%;
      background: currentColor;
    }

    .en-vivo {
      background: #e4f4ec;
      color: var(--exito);
    }

    .en-vivo .punto {
      animation: pulso 2s ease-in-out infinite;
    }

    .sin-conexion {
      background: #fdecea;
      color: var(--peligro);
    }

    @keyframes pulso {
      0%, 100% { opacity: 1; }
      50% { opacity: 0.25; }
    }

    @media (prefers-reduced-motion: reduce) {
      .en-vivo .punto { animation: none; }
    }
  `
})
export class IndicadorConexion {
  readonly conectado = input.required<boolean>();
  readonly segundos = input.required<number>();
}
