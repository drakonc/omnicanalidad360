import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  Agente,
  Causal,
  DocumentoDePrueba,
  EstadoConsola,
  LlamadaEntrante,
  Tablero
} from './modelos';

/**
 * Unico punto por donde las pantallas hablan con el backend.
 */
@Injectable({ providedIn: 'root' })
export class Api {
  private readonly http = inject(HttpClient);
  private readonly base = 'http://localhost:5025/api';

  // Panel que simula la llamada entrante.

  llamar(telefonoOrigen: string, documentoMarcado: string | null): Observable<LlamadaEntrante> {
    return this.http.post<LlamadaEntrante>(`${this.base}/simulacion/llamadas`, {
      telefonoOrigen,
      documentoMarcado
    });
  }

  colgarPaciente(idLlamada: string): Observable<unknown> {
    return this.http.post(`${this.base}/simulacion/llamadas/${idLlamada}/colgar-paciente`, {});
  }

  documentosDePrueba(): Observable<DocumentoDePrueba[]> {
    return this.http.get<DocumentoDePrueba[]>(`${this.base}/simulacion/documentos-de-prueba`);
  }

  // Consola del agente.

  agentes(): Observable<Agente[]> {
    return this.http.get<Agente[]>(`${this.base}/consola/agentes`);
  }

  consola(idAgente: string): Observable<EstadoConsola> {
    return this.http.get<EstadoConsola>(`${this.base}/consola/${idAgente}`);
  }

  cambiarEstado(idAgente: string, estado: string): Observable<unknown> {
    return this.http.post(`${this.base}/consola/${idAgente}/estado`, { estado });
  }

  contestar(idAgente: string, idLlamada: string): Observable<unknown> {
    return this.http.post(`${this.base}/consola/${idAgente}/contestar/${idLlamada}`, {});
  }

  colgar(idAgente: string, idLlamada: string): Observable<unknown> {
    return this.http.post(`${this.base}/consola/${idAgente}/colgar/${idLlamada}`, {});
  }

  // Supervision.

  tablero(): Observable<Tablero> {
    return this.http.get<Tablero>(`${this.base}/supervisor/tablero`);
  }

  tipificaciones(): Observable<Causal[]> {
    return this.http.get<Causal[]>(`${this.base}/catalogo/tipificaciones`);
  }
}
