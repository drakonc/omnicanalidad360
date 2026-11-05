import { Component, input } from '@angular/core';

/**
 * Franja de estado prominente de 64 px que el diseno EverCall pone
 * arriba de cada pantalla del agente. Cambia de color segun el estado.
 */
@Component({
  selector: 'app-franja',
  template: `
    <header class="franja" [attr.data-estado]="estado()">
      <span class="estado-actual">
        <span class="punto"></span>
        {{ etiquetaEstado() }}
      </span>

      <ng-content />
    </header>
  `,
  styles: `
    .franja {
      height: var(--alto-franja);
      flex-shrink: 0;
      display: flex;
      align-items: center;
      gap: 1.5rem;
      padding: 0 1.5rem;
      background: var(--desconectado);
      color: var(--texto-claro);
      overflow-x: auto;
    }

    .franja[data-estado="disponible"]   { background: var(--disponible); }
    .franja[data-estado="en_cola"]      { background: var(--en-llamada); }
    .franja[data-estado="en_llamada"]   { background: var(--en-llamada); }
    .franja[data-estado="post_llamada"] { background: var(--post-llamada); }
    .franja[data-estado="pausa"]        { background: var(--pausa); }
    .franja[data-estado="supervisor"]   { background: var(--tinta); }

    .estado-actual {
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      background: rgba(255, 255, 255, 0.18);
      border-radius: var(--radio-pildora);
      padding: 0.4rem 0.95rem;
      font-size: 0.78rem;
      font-weight: 700;
      letter-spacing: 0.07em;
      text-transform: uppercase;
      white-space: nowrap;
      flex-shrink: 0;
    }

    .punto {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      background: currentColor;
    }

    .franja[data-estado="en_cola"] .punto {
      animation: latido 1s ease-in-out infinite;
    }

    @keyframes latido {
      0%, 100% { opacity: 1; }
      50% { opacity: 0.2; }
    }
  `
})
export class Franja {
  readonly estado = input.required<string>();
  readonly etiquetaEstado = input.required<string>();
}
