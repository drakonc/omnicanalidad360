# Omnicanalidad 360 — Consola de atención con registro único

Plataforma de atención telefónica para la subcampaña NEPS Imagenología de Linkai. El PRD completo está en `docs/prd.md` y manda sobre este archivo.

**Qué se construye hoy**: una versión local, en este computador, con la llamada, Everest y LUMIER simulados. Demuestra el registro único: el paciente aparece solo al contestar, la cita se agenda desde la consola y la gestión se guarda una sola vez. La telefonía en Azure y las conexiones de prueba siguen el plan de 4 semanas del PRD.

## Stack
- Backend: .NET 10 (`backend/`), base de datos SQLite local, pensada para migrar después a SQL Server.
- Frontend: Angular 22 (`frontend/`). Node 24.22.2, gestionado con nvm (`nvm use 24.22.2`). Angular 22 exige 24.15.0 o superior.
- Pruebas: xUnit en `backend/tests/Omnicanalidad360.Tests`.

## Cómo abrir la aplicación
1. `cd backend/src/Omnicanalidad360.Api && dotnet run` — deja el backend corriendo.
2. En otra terminal: `cd frontend && npm start` — abre `http://localhost:4200` en el navegador.
3. Pruebas: `cd backend && dotnet test`.

## Hitos
- [x] Consola y datos de prueba
- [ ] Paciente precargado
- [ ] Registro único
- [ ] Supervisor y exportación

## Reglas de trabajo
- Solo datos inventados. Ningún dato real de pacientes entra a la plataforma (RN-12).
- Cada sistema simulado (llamada, Everest y LUMIER) va por separado, para cambiarlo luego por el real sin tocar la consola.
- Si Everest no confirma derechos, no se agenda (RN-4 y RN-7).
- Seguir el PRD sin agregar nada que no esté en él.
- Explicar todo en lenguaje sencillo, sin términos técnicos.
- Después de cada cambio, decir cómo comprobarlo en la pantalla.
- Responder en español. Comentarios de código en español. Sin emojis.
