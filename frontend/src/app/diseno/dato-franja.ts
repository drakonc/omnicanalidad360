import { Component, input } from '@angular/core';

/**
 * Un dato dentro de la franja de estado: rotulo pequeno arriba y
 * valor grande abajo. El cronometro usa 32 px, como pide el diseno.
 */
@Component({
  selector: 'app-dato-franja',
  template: `
    <div class="bloque" [class.grande]="grande()">
      <span class="rotulo-franja">{{ rotulo() }}</span>
      <span class="valor tabular">{{ valor() }}</span>
    </div>
  `,
  styles: `
    .bloque {
      display: flex;
      flex-direction: column;
      line-height: 1.2;
      white-space: nowrap;
      flex-shrink: 0;
    }

    .rotulo-franja {
      font-size: 0.68rem;
      font-weight: 600;
      letter-spacing: 0.08em;
      text-transform: uppercase;
      opacity: 0.8;
    }

    .valor {
      font-size: 1.05rem;
      font-weight: 600;
    }

    .bloque.grande .valor {
      font-size: 32px;
      font-weight: 700;
      letter-spacing: -0.02em;
    }
  `
})
export class DatoFranja {
  readonly rotulo = input.required<string>();
  readonly valor = input.required<string>();
  readonly grande = input(false);
}
