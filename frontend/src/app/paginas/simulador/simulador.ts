import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Api } from '../../nucleo/api';
import { Franja } from '../../diseno/franja';
import { DatoFranja } from '../../diseno/dato-franja';
import { DocumentoDePrueba, LlamadaEntrante } from '../../nucleo/modelos';

/**
 * Panel que hace de telefono del paciente.
 * Reemplaza a la telefonia real mientras no este el numero de prueba en Azure.
 */
@Component({
  selector: 'app-simulador',
  imports: [FormsModule, Franja, DatoFranja],
  templateUrl: './simulador.html',
  styleUrl: './simulador.scss'
})
export class Simulador implements OnInit {
  private readonly api = inject(Api);

  protected readonly telefono = signal('+573001234567');
  protected readonly documento = signal('1000000001');
  protected readonly marcaDocumento = signal(true);

  protected readonly documentosDePrueba = signal<DocumentoDePrueba[]>([]);
  protected readonly ultimaLlamada = signal<LlamadaEntrante | null>(null);
  protected readonly mensaje = signal<string | null>(null);
  protected readonly colgada = signal(false);

  ngOnInit(): void {
    this.api.documentosDePrueba().subscribe({
      next: (lista) => this.documentosDePrueba.set(lista),
      error: () => this.mensaje.set('No se pudo conectar con la plataforma. Revisa que el backend este corriendo.')
    });
  }

  protected llamar(): void {
    this.mensaje.set(null);
    this.colgada.set(false);

    // Si el paciente no marca, el documento va vacio y la llamada igual entra.
    const documentoMarcado = this.marcaDocumento() ? this.documento() : null;

    this.api.llamar(this.telefono(), documentoMarcado).subscribe({
      next: (llamada) => {
        this.ultimaLlamada.set(llamada);
        this.mensaje.set('La llamada entro y esta esperando agente.');
      },
      error: () => this.mensaje.set('No se pudo iniciar la llamada.')
    });
  }

  protected colgarPaciente(): void {
    const llamada = this.ultimaLlamada();
    if (!llamada) {
      return;
    }

    this.api.colgarPaciente(llamada.idLlamada).subscribe({
      next: () => {
        this.colgada.set(true);
        this.mensaje.set('El paciente colgo antes de que le contestaran: la llamada quedo abandonada.');
      },
      error: () => this.mensaje.set('Ya no se puede colgar: un agente contesto la llamada.')
    });
  }

  protected usarDocumento(numero: string): void {
    this.documento.set(numero);
    this.marcaDocumento.set(true);
  }

  protected textoDerechos(derechos: string): string {
    return derechos === 'vigentes' ? 'Con derechos' : 'Sin derechos';
  }
}
