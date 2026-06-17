// Polyfills de APIs Web que o jsdom nao expoe, necessarios para o MSW v2.
// Usamos require() (em vez de import) de proposito: os globais de
// TextEncoder/TextDecoder/streams precisam estar definidos ANTES de carregar
// o undici, e os imports ES sao icados (hoisted), o que quebraria essa ordem.
/* eslint-disable @typescript-eslint/no-require-imports */
// export {} torna este arquivo um modulo, evitando que os const abaixo
// colidam com os tipos globais do DOM (TextDecoder, fetch, etc.).
export {};

const { TextDecoder, TextEncoder } = require("node:util");
const {
  ReadableStream,
  TransformStream,
  WritableStream,
} = require("node:stream/web");
const { MessagePort, BroadcastChannel } = require("node:worker_threads");

// configurable: true e essencial — o @mswjs/interceptors redefine alguns
// destes globais (ex.: Request) ao iniciar; sem isso o defineProperty falha.
function definirGlobais(valores: Record<string, unknown>) {
  for (const [nome, value] of Object.entries(valores)) {
    Object.defineProperty(globalThis, nome, {
      value,
      writable: true,
      configurable: true,
    });
  }
}

definirGlobais({
  TextDecoder,
  TextEncoder,
  ReadableStream,
  TransformStream,
  WritableStream,
  MessagePort,
  BroadcastChannel,
});

const { fetch, Headers, FormData, Request, Response } = require("undici");

definirGlobais({ fetch, Headers, FormData, Request, Response });
